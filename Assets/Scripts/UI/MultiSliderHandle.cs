using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MultiSliderHandle : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public RectTransform track;
    public MultiSliderController sliderController;
    public float currentValue;

    public float snapIncrement;

    RectTransform rect;

    private Vector2 dragStartPointerLocal;
    private float dragStartAnchoredY;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void SetNormalizedValue(float value)
    {
        currentValue = value;
        float y = Mathf.Lerp(0, track.rect.height, value);
        rect.anchoredPosition = new Vector2(0, y);
    }

    public float GetNormalizedValue()
    {
        return rect.anchoredPosition.y / track.rect.height;
    }

    // capture initial pointer and handle positions when drag begins
    public void OnBeginDrag(PointerEventData eventData)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            track,
            eventData.position,
            eventData.pressEventCamera,
            out local
        );

        dragStartPointerLocal = local;
        dragStartAnchoredY = rect.anchoredPosition.y;
    }

    // apply pointer delta to the starting anchored position
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            track,
            eventData.position,
            eventData.pressEventCamera,
            out local
        );

        float deltaY = local.y - dragStartPointerLocal.y;
        float y = dragStartAnchoredY + deltaY;
        y = Mathf.Clamp(y, 0, track.rect.height);
        float value = y / track.rect.height;

        // Hand the raw target to the controller; it snaps, clamps, and pushes neighbouring handles
        // so the drag cascades past any segments already pinned at the minimum.
        sliderController.MoveHandle(sliderController.handles.IndexOf(this), value);
    }
}
