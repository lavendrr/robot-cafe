using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Unity.VisualScripting;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    public Transform previousParent;
    private List<(int, int)> offsets;
    private GameObject previousHoverCell = null;

    public DraggableItem(List<(int, int)> offsets = null)
    {
        if (offsets != null)
        {
            this.offsets = offsets;
        }
    }

    void Start()
    {
        offsets = new List<(int, int)> { (0, 1), (1, 0) };
        previousParent = transform.root;
    }

    [ContextMenu("Rotate offsets clockwise")]
    public void RotateOffsetsClockwise()
    {
        // Rotates 90 degrees clockwise
        for (int i = 0; i < offsets.Count; i++)
        {
            offsets[i] = (offsets[i].Item2, offsets[i].Item1 * -1);
        }
    }

    [ContextMenu("Rotate offsets counterclockwise")]
    public void RotateOffsetsCounterclockwise()
    {
        // Rotates 90 degrees clockwise
        for (int i = 0; i < offsets.Count; i++)
        {
            offsets[i] = (offsets[i].Item2 * -1, offsets[i].Item1);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Start drag");
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
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Update the item's position to match the cursor
        transform.position = Input.mousePosition;
        GameObject hoverCell = null;
        foreach (var element in eventData.hovered)
        {
            Debug.Log(element.name);
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
        Debug.Log("End drag");

        // Re-enable raycasting so it can be detected by the cursor
        image.raycastTarget = true;

        // Checks if there are any cells underneath the item where it was released
        GameObject cell = null, spawner = null;
        foreach (var element in eventData.hovered)
        {
            Debug.Log(element.name);
            if (element.name.Contains("Cell"))
            {
                cell = element;
            }
            else if (element.name.Contains("ItemSpawner"))
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
            if (!cell.GetComponent<GridSlot>().AttemptItemSlot(gameObject))
            {
                if (previousParent != transform.root)
                {
                    if (previousHoverCell != null)
                    {
                        previousHoverCell.GetComponent<GridSlot>().DisableHoverColor(this);
                    }

                    previousParent.GetComponent<GridSlot>().AttemptItemSlot(gameObject);
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
            if (!previousParent.GetComponent<GridSlot>().AttemptItemSlot(gameObject))
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

    public List<(int, int)> GetOffsets()
    {
        return offsets;
    }
}