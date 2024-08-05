using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridSlot : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private bool occupied = false;
    private (int, int) coords;

    public void SetCoords((int, int) coords)
    {
        this.coords = coords;
    }

    public (int, int) GetCoords()
    {
        return coords;
    }

    public bool AttemptItemSlot(GameObject dropped)
    {
        // Returns true if item was successfully slotted in
        DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
        List<(int, int)> offsets = draggableItem.GetOffsets();
        if (occupied == false)
        {
            if (offsets != null)
            {
                try
                {
                    List<GridSlot> offsetCells = new List<GridSlot>();
                    // Checks the occupied status of the slot at each offset of the item, and fails if any are occupied
                    foreach ((int, int) offset in offsets)
                    {
                        GridSlot offsetCell = PlanningManager.Instance.gridArray[coords.Item1 + offset.Item1, coords.Item2 + offset.Item2].GetComponent<GridSlot>();
                        if (offsetCell.GetOccupiedStatus())
                        {
                            return false;
                        }
                        offsetCells.Add(offsetCell);
                    }
                    // If all necessary slots were found to be empty, then set them to be occupied and reparent the item
                    foreach (GridSlot cell in offsetCells)
                    {
                        cell.SetOccupiedStatus(true);
                    }
                    SetOccupiedStatus(true);
                    dropped.transform.SetParent(transform);
                    dropped.GetComponent<DraggableItem>().previousParent = transform.root;
                    // dropped.transform.SetAsLastSibling();
                    return true;
                }
                catch (IndexOutOfRangeException)
                {
                    // If the offset is out of range of where the method is trying to check, the method fails
                    Debug.Log("index out of range");
                    return false;
                }
            }
            else
            {
                // draggableItem.parentAfterDrag = transform;
                SetOccupiedStatus(true);
                dropped.transform.SetParent(transform);
                dropped.GetComponent<DraggableItem>().previousParent = null;
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
        occupied = occ;
        // Debug.Log(string.Format("Cell {0}, {1} set to {2}", coords.Item1, coords.Item2, occ));
    }

    public void OnDrop(PointerEventData eventData)
    {
        // AttemptItemSlot(eventData.pointerDrag);
    }

    public void OnRemove(DraggableItem item)
    {
        // itemObj.transform.SetParent(transform.root);
        SetOccupiedStatus(false);

        List<(int, int)> offsets = item.GetOffsets();
        if (offsets != null)
        {
            foreach ((int, int) offset in offsets)
            {
                PlanningManager.Instance.gridArray[coords.Item1 + offset.Item1, coords.Item2 + offset.Item2].GetComponent<GridSlot>().SetOccupiedStatus(false);
            }
        }
    }
}
