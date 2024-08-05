using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    public Transform previousParent;
    private List<(int, int)> offsets;

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

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Start drag");
        if (transform.parent != transform.root)
        {
            previousParent = transform.parent;
            transform.parent.GetComponent<GridSlot>().OnRemove(this);
        }
        // parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End drag");
        if (eventData.hovered.Count > 0)
        {
            GameObject cell = null;
            foreach (var item in eventData.hovered)
            {
                Debug.Log(item.name);
                if (item.name.Contains("Cell"))
                {
                    cell = item;
                }
            }
            // Debug.Log(eventData.hovered[0].name);
            if (cell != null && !eventData.hovered[0].GetComponent<GridSlot>().AttemptItemSlot(gameObject))
            {
                transform.SetParent(previousParent);
            }
        }
        else
        {
            if (previousParent != transform.root)
            {
                if (!previousParent.GetComponent<GridSlot>().AttemptItemSlot(gameObject))
                {
                    transform.SetParent(previousParent);
                }
            }
            Debug.Log("Dropped in the void");
        }
        image.raycastTarget = true;
    }

    public List<(int, int)> GetOffsets()
    {
        return offsets;
    }
}