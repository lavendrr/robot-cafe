using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridItemSpawn : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    public FurnitureObject furnitureType;
    public GameObject draggableItemPrefab;
    private DraggableItem draggableItem;

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("onbegindrag");
        GameObject spawnedItem = Instantiate(draggableItemPrefab, transform.root);
        spawnedItem.GetComponent<Image>().sprite = furnitureType.catalogSprite;
        draggableItem = spawnedItem.GetComponent<DraggableItem>();
        draggableItem.Init(furnitureType);
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
