using UnityEngine;

public class FocusInteractable : MonoBehaviour
{
    [Tooltip("Where the focus camera sits while focused. Falls back to a child named \"FocusPoint\".")]
    [SerializeField] public Transform focusPoint;

    protected virtual void Awake()
    {
        // Convenience fallback so prefabs only need a correctly-named child.
        if (focusPoint == null)
        {
            Transform found = transform.Find("FocusPoint");
            if (found != null) focusPoint = found;
        }
    }

    public virtual void OnFocusEnter() { }
    public virtual void OnFocusExit() { }
}
