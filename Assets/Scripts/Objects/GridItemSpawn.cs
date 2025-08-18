using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridItemSpawn : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
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

    public void OnHover()
    {
        PlanningManager.Instance.ShowTooltip(furnitureType);
    }

    public void OnUnhover()
    {
        PlanningManager.Instance.HideTooltip();
    }

    // Hook up to Unity UI events
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnUnhover();
    }
}
