using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridItemSpawn : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private GameObject itemPrefab;

    private GameObject item;
    private DraggableItem draggableItem;

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("onbegindrag");
        GameObject item = Instantiate(itemPrefab, transform.root);
        draggableItem = item.GetComponent<DraggableItem>();
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



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
