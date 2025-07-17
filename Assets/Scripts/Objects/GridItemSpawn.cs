using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridItemSpawn : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private FurnitureObject furniture;
    private DraggableItem draggableItem;

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("onbegindrag");
        GameObject item = Instantiate(furniture.prefab, transform.root);
        draggableItem = item.GetComponent<DraggableItem>();
        draggableItem.Init(furniture.gridOffsets);
        draggableItem.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        draggableItem.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        draggableItem.OnEndDrag(eventData);
    }
}
