using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GridSlot : MonoBehaviour
{
    [SerializeField]
    private bool occupied = false;
    [SerializeField]
    private Image image;
    private (int, int) coords;

    void Start()
    {
        image = GetComponent<Image>();
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
    public bool AttemptItemSlot(GameObject dropped)
    {
        DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
        List<(int, int)> offsets = draggableItem.GetOffsets();
        // Checks if the cell already has something slotted in
        if (!GetOccupiedStatus())
        {
            // Call this next block if the item being slotted extends beyond one cell
            if (offsets != null)
            {
                try
                {
                    // Makes a list to store the cells at the corresponding offsets
                    List<GridSlot> offsetCells = new();
                    // Checks the occupied status of the slot at each offset of the item, stores it in the list, and fails if any are occupied
                    foreach ((int, int) offset in offsets)
                    {
                        GridSlot offsetCell = PlanningManager.Instance.gridArray[coords.Item1 + offset.Item1, coords.Item2 + offset.Item2].GetComponent<GridSlot>();
                        if (offsetCell.GetOccupiedStatus())
                        {
                            return false;
                        }
                        offsetCells.Add(offsetCell);
                    }
                    // If all necessary slots were found to be empty, then set them to be occupied
                    foreach (GridSlot cell in offsetCells)
                    {
                        cell.SetOccupiedStatus(true);
                    }
                    SetOccupiedStatus(true);
                    // Reparent the item and reset the previous parent
                    dropped.transform.SetParent(transform);
                    dropped.GetComponent<DraggableItem>().previousParent = transform.root;
                    return true;
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
                SetOccupiedStatus(true);
                dropped.transform.SetParent(transform);
                dropped.GetComponent<DraggableItem>().previousParent = transform.root;
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    public bool GetOccupiedStatus()
    {
        return occupied;
    }

    public void SetOccupiedStatus(bool occ)
    {
        if (occ)
        {
            image.color = Color.blue;
        }
        else
        {
            image.color = Color.white;
        }

        occupied = occ;
    }

    // Called when an item is removed from a slot
    public void OnRemove(DraggableItem item)
    {
        SetOccupiedStatus(false);

        // If the item took up more than one cell, set the corresponding cells to be empty as well
        List<(int, int)> offsets = item.GetOffsets();
        if (offsets != null)
        {
            foreach ((int, int) offset in offsets)
            {
                PlanningManager.Instance.gridArray[coords.Item1 + offset.Item1, coords.Item2 + offset.Item2].GetComponent<GridSlot>().SetOccupiedStatus(false);
            }
        }
    }

    public void HoverColor(DraggableItem item)
    {
        image.color = Color.green;

        // If the item took up more than one cell, set the corresponding cells to be empty as well
        try
        {
            List<(int, int)> offsets = item.GetOffsets();
            if (offsets != null)
            {
                foreach ((int, int) offset in offsets)
                {
                    PlanningManager.Instance.gridArray[coords.Item1 + offset.Item1, coords.Item2 + offset.Item2].GetComponent<Image>().color = Color.green;
                }
            }
        }
        catch (IndexOutOfRangeException)
        {
            Debug.Log("Out of range for hovering");
        }

    }

    public void DisableHoverColor(DraggableItem item)
    {
        image.color = Color.white;

        try
        {
            // If the item took up more than one cell, set the corresponding cells to be empty as well
            List<(int, int)> offsets = item.GetOffsets();
            if (offsets != null)
            {
                foreach ((int, int) offset in offsets)
                {
                    PlanningManager.Instance.gridArray[coords.Item1 + offset.Item1, coords.Item2 + offset.Item2].GetComponent<Image>().color = Color.white;
                }
            }
        }
        catch (IndexOutOfRangeException)
        {
            Debug.Log("Out of range for hovering");
        }
    }
}
