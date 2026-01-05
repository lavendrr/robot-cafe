using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Diagnostics.Tracing;

public class GridSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private bool occupied = false;
    [SerializeField]
    private Image image;
    private (int, int) coords;
    public bool seating { get; private set; } = false;

    void Start()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }
    }

    public void SetCoords((int, int) coords)
    {
        this.coords = coords;
    }

    public (int, int) GetCoords()
    {
        return coords;
    }

    public void HighlightSquare()
    {

    }

    // Returns true if item was successfully slotted in
    public bool AttemptItemSlot(GameObject dropped, int rotation = 0)
    {
        GridItem gridItem = dropped.GetComponent<GridItem>();
        if (gridItem == null)
            return false;

        List<GridCoord> offsets = gridItem.GetOffsets();
        List<Sprite> gridSprites = gridItem.furnitureObject.gridSprites;

        // Checks if the cell already has something slotted in
        if (!GetOccupiedStatus(gridItem.isSeating))
        {
            // Call this next block if the item being slotted extends beyond one cell
            if (offsets != null)
            {
                try
                {
                    // Makes a list to store the cells at the corresponding offsets
                    List<GridSlot> offsetCells = new();
                    // Checks the occupied status of the slot at each offset of the item, stores it in the list, and fails if any are occupied
                    foreach (GridCoord offset in offsets)
                    {
                        GridSlot offsetCell = PlanningManager.Instance.gridArray[coords.Item1 + offset.row, coords.Item2 + offset.col].GetComponent<GridSlot>();
                        if (offsetCell.GetOccupiedStatus(gridItem.isSeating))
                        {
                            return false;
                        }
                        offsetCells.Add(offsetCell);
                    }
                    // If all necessary slots were found to be empty, then set them to be occupied
                    for (int i = 0; i < offsetCells.Count; i++)
                    {
                        Sprite s = (gridSprites != null && gridSprites.Count > i + 1) ? gridSprites[i + 1] : null;
                        offsetCells[i].SetOccupiedStatus(true, s, rotation);
                    }
                    // root sprite
                    Sprite rootSprite = (gridSprites != null && gridSprites.Count > 0) ? gridSprites[0] : null;
                    SetOccupiedStatus(true, rootSprite, rotation);

                    // Reparent the item and reset the previous parent
                    dropped.transform.SetParent(transform);
                    dropped.GetComponent<GridItem>().previousParent = transform.root;
                }
                catch (IndexOutOfRangeException)
                {
                    // If the offset is out of range of where the method is trying to check (i.e. the item is being slotted somewhere it won't fit within the grid), the method fails
                    return false;
                }
            }
            // If there are no offsets to check and this slot was unoccupied, then the method succeeds
            else
            {
                Sprite rootSprite = (gridSprites != null && gridSprites.Count > 0) ? gridSprites[0] : null;
                SetOccupiedStatus(true, rootSprite, rotation);
                dropped.transform.SetParent(transform);
                dropped.GetComponent<GridItem>().previousParent = transform.root;
            }
        }
        else
        {
            return false;
        }
        
        return true;
    }

    public bool GetOccupiedStatus(bool ?isSourceSeating = null)
    {
        // Return the actual occupied status if passed null or if the source seating type matches, otherwise always return occupied (incompatible)
        return isSourceSeating == seating || isSourceSeating is null ? occupied : true;
    }

    // Set occupied/unoccupied and optionally update sprite for this slot
    public void SetOccupiedStatus(bool occ, Sprite sprite = null, int rotation = 0)
    {
        // This method gets called from PlanningManager on Start, which may run before GridSlot's Start,
        // so we need to make sure image is assigned
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (occ)
        {
            if (sprite != null)
            {
                image.color = Color.white;
                image.sprite = sprite;
            }
            else
            {
                image.color = Color.blue;
            }
            // apply rotation to the image rect transform
            image.rectTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
        }
        else
        {
            image.color = Color.white;
            // clear the sprite when unoccupied
            image.sprite = null;
            image.rectTransform.localEulerAngles = Vector3.zero;
        }

        occupied = occ;
    }

    // Called when an item is removed from a slot
    public void OnRemove(GridItem item)
    {
        SetOccupiedStatus(false);

        // If the item took up more than one cell, set the corresponding cells to be empty as well
        List<GridCoord> offsets = item.GetOffsets();
        if (offsets != null)
        {
            foreach (GridCoord offset in offsets)
            {
                PlanningManager.Instance.gridArray[coords.Item1 + offset.row, coords.Item2 + offset.col].GetComponent<GridSlot>().SetOccupiedStatus(false);
            }
        }
    }

    public void HoverColor(GridItem item)
    {
        if (GetOccupiedStatus(item.isSeating))
        {
            image.color = Color.red;
        }
        else
        {
            image.color = Color.green;
        }

        List<GridCoord> offsets = item.GetOffsets();
        List<GameObject> changedCells = new();

        // If the item takes up more than one cell, set the colors for those cells as well
        try
        {
            if (offsets != null)
            {
                foreach (GridCoord offset in offsets)
                {
                    GameObject offsetCell = PlanningManager.Instance.gridArray[coords.Item1 + offset.row, coords.Item2 + offset.col];
                    if (offsetCell.GetComponent<GridSlot>().GetOccupiedStatus(item.isSeating))
                    {
                        offsetCell.GetComponent<Image>().color = Color.red;
                    }
                    else
                    {
                        offsetCell.GetComponent<Image>().color = Color.green;
                    }
                    changedCells.Add(offsetCell);
                }
            }
        }
        catch (IndexOutOfRangeException)
        {
            foreach (GameObject cell in changedCells)
            {
                cell.GetComponent<Image>().color = Color.red;
            }
            image.color = Color.red;
        }

    }

    public void DisableHoverColor(GridItem item)
    {
        // Set this cell's color back to blue if occupied, yellow if empty & seating, or white if empty and non-seating
        image.color = GetOccupiedStatus() ? Color.blue : seating ? Color.yellow : Color.white;
        try
        {
            // If the item took up more than one cell, set the corresponding cells to be empty as well
            List<GridCoord> offsets = item.GetOffsets();
            if (offsets != null)
            {
                foreach (GridCoord offset in offsets)
                {
                    GameObject cell = PlanningManager.Instance.gridArray[coords.Item1 + offset.row, coords.Item2 + offset.col];
                    // Reset this cell's color back to blue if occupied, yellow if empty & seating, or white if empty and non-seating
                    cell.GetComponent<Image>().color = cell.GetComponent<GridSlot>().GetOccupiedStatus() ? Color.blue : cell.GetComponent<GridSlot>().seating ? Color.yellow : Color.white;
                }
            }
        }
        catch (IndexOutOfRangeException)
        {
            return;
        }
    }

    public void ToggleSeating()
    {
        if (!GetOccupiedStatus())
        {
            seating = !seating;
            Debug.Log($"Cell {coords} is now {(seating ? "seating" : "furniture")}");
        }
        else
        {
            Debug.Log("Tile is occupied, can't change tile type");
        }
        
        image.color = seating ? Color.yellow : Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlanningManager.Instance.hoverCell = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PlanningManager.Instance.hoverCell = null;
    }
}
