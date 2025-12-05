using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridItemSpawner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    public FurnitureObject furnitureType;
    public GameObject gridItemPrefab;
    private GridItem gridItem;

    public void OnBeginDrag(PointerEventData eventData)
    {
        GameObject spawnedItem = Instantiate(gridItemPrefab, transform.root);
        spawnedItem.GetComponent<Image>().sprite = furnitureType.catalogSprite;
        gridItem = spawnedItem.GetComponent<GridItem>();
        gridItem.Init(furnitureType, true);
        gridItem.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        gridItem.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        gridItem.OnEndDrag(eventData);
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
