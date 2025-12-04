using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using JetBrains.Annotations;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
    public Transform previousParent;
    private GameObject previousHoverCell, currentHoverCell = null;
    public FurnitureObject furnitureObject { get; private set; }
    private List<GridCoord> itemCoords;
    // TODO: this is part of an eventual fix for a bug where if you pick up an object, rotate it to a position
    // where it would no longer be valid in its original cell, then drop it, it would snap back to the original cell even if it no longer fit there
    //private int beginDragRotation = 0;
    public int rotation = 0;

    public void Init(FurnitureObject f)
    {
        if (f == null)
        {
            Debug.LogError("FurnitureObject passed to DraggableItem.Init() was null.");
            return;
        }
        furnitureObject = f;
        itemCoords = new List<GridCoord>(furnitureObject.gridOffsets);
    }

    void Start()
    {
        previousParent = transform.root;
    }

    public void RotateCounterclockwise()
    {
        rotation = (rotation + 90) % 360;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
        GridSlot currentSlot = null;
        if (previousHoverCell != null)
        {
            currentSlot = previousHoverCell.GetComponent<GridSlot>();
            currentSlot.DisableHoverColor(this);
        }

        // Rotates 90 degrees counterclockwise
        for (int i = 0; i < itemCoords.Count; i++)
        {
            if (currentSlot != null)
            {
                currentSlot.DisableHoverColor(this);
            }
            GridCoord temp = itemCoords[i];
            temp.col = itemCoords[i].row;
            temp.row = itemCoords[i].col * -1;
            itemCoords[i] = temp;
        }

        if (currentSlot != null)
        {
            currentSlot.HoverColor(this);
        }
    }

    public void RotateClockwise()
    {
        rotation = (rotation - 90) % 360;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
        if (rotation < 0) rotation += 360;
        GridSlot currentSlot = null;
        if (previousHoverCell != null)
        {
            currentSlot = previousHoverCell.GetComponent<GridSlot>();
            currentSlot.DisableHoverColor(this);
        }

        // Rotates 90 degrees clockwise
        for (int i = 0; i < itemCoords.Count; i++)
        {
            if (currentSlot != null)
            {
                currentSlot.DisableHoverColor(this);
            }
            GridCoord temp = itemCoords[i];
            temp.col = itemCoords[i].row * -1;
            temp.row = itemCoords[i].col;
            itemCoords[i] = temp;
        }

        if (currentSlot != null)
        {
            currentSlot.HoverColor(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnHover();
        PlanningManager.Instance.SetCurrentItem(this);

        // If the item was slotted into a cell, tell that cell to call its removal method and update the previous parent
        if (transform.parent != transform.root)
        {
            previousParent = transform.parent;
            transform.parent.GetComponent<GridSlot>().OnRemove(this);
        }
        // Unparent the cell, set it as last sibling so it's on top of the rest of the UI, and turn raycasting off so it doesn't obscure the cursor's detection
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
        image.sprite = furnitureObject.catalogSprite ?? null;
        //beginDragRotation = rotation;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // OnHover();
        // Update the item's position to match the cursor
        transform.position = Input.mousePosition;
        GameObject hoverCell = null;
        foreach (var element in eventData.hovered)
        {
            if (element.name.Contains("Cell"))
            {
                hoverCell = element;
            }
        }

        if (hoverCell != null)
        {
            if (previousHoverCell == null)
            {
                previousHoverCell = hoverCell;
                previousHoverCell.GetComponent<GridSlot>().HoverColor(this);
            }
            else if (previousHoverCell != hoverCell)
            {
                previousHoverCell.GetComponent<GridSlot>().DisableHoverColor(this);
                previousHoverCell = hoverCell;
                previousHoverCell.GetComponent<GridSlot>().HoverColor(this);
            }
            currentHoverCell = hoverCell;
        }
        else
        {
            if (previousHoverCell != null)
            {
                previousHoverCell.GetComponent<GridSlot>().DisableHoverColor(this);
                previousHoverCell = null;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        PlanningManager.Instance.SetCurrentItem(null);

        // Re-enable raycasting so it can be detected by the cursor
        image.raycastTarget = true;
        image.sprite = furnitureObject.gridSprites[0] ?? null;

        // Checks if there are any cells underneath the item where it was released
        GameObject cell = null, spawner = null;
        foreach (var element in eventData.hovered)
        {
            if (element.name.Contains("Cell"))
            {
                cell = element;
            }
            else if (element.name.Contains("CatalogPanel") || element.name.Contains("CatalogIcon"))
            {
                spawner = element;
            }
        }

        if (spawner != null)
        {
            if (previousParent != transform.root)
            {
                previousParent.GetComponent<GridSlot>().OnRemove(this);
            }

            Destroy(gameObject);
            return;
        }

        // If a cell was found, attempt to slot the item into the cell. If slotting fails, reset the item to its previous parent
        if (cell != null)
        {
            // Slotting failed
            if (!cell.GetComponent<GridSlot>().AttemptItemSlot(gameObject, rotation))
            {
                // Reset the color of the previous hover cell, then keep going
                if (previousHoverCell != null)
                {
                    previousHoverCell.GetComponent<GridSlot>().DisableHoverColor(this);
                }
                // If this item was previously slotted to another cell, slot it back and return
                if (previousParent != transform.root)
                {
                    previousParent.GetComponent<GridSlot>().AttemptItemSlot(gameObject, rotation);
                    return;
                }
            }
            // Slotting succeeded
            else
            {
                previousHoverCell = null;
                return;
            }
        }
        // If a cell wasn't found (meaning it was dropped in the void), but the object previously belonged to a cell, reset it to that cell
        else if (previousParent != transform.root)
        {
            if (!previousParent.GetComponent<GridSlot>().AttemptItemSlot(gameObject, rotation))
            {
                transform.SetParent(previousParent);
                return;
            }
            // Slotting succeeded
            else
            {
                return;
            }
        }

        Destroy(gameObject);
    }

    public List<GridCoord> GetOffsets()
    {
        return itemCoords;
    }

    public void OnHover()
    {
        PlanningManager.Instance.ShowTooltip(furnitureObject);
    }

    public void OnUnhover()
    {
        PlanningManager.Instance.HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnUnhover();
    }
}
