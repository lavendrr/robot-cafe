using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Reusable "save your changes?" confirmation modal. Lives on the SaveChangesModal GameObject in
// the Planning scene and starts inactive; callers hold a serialized reference and invoke Show(...)
// with the actions to run for each button. The modal hides itself before invoking a callback, so
// each Show fully re-wires the buttons (listeners are cleared first) — callbacks never stack.
public class SaveChangesModal : MonoBehaviour
{
    [SerializeField]
    private TMP_Text titleText;
    [SerializeField]
    private Button saveButton, discardButton, cancelButton;

    private const string DefaultTitle = "Save your changes?";

    void Start()
    {
        if (saveButton == null || discardButton == null || cancelButton == null)
            Debug.LogError("[SaveChangesModal] Button references not set. Please assign them in the inspector.");
    }

    // Opens the modal. onSave / onDiscard close out of whatever the caller is doing; onCancel simply
    // dismisses the modal and returns to the caller's screen (defaults to a no-op dismiss).
    public void Show(Action onSave, Action onDiscard, Action onCancel = null, string title = null)
    {
        if (titleText != null)
            titleText.text = string.IsNullOrEmpty(title) ? DefaultTitle : title;

        Wire(saveButton, onSave);
        Wire(discardButton, onDiscard);
        Wire(cancelButton, onCancel);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // Each button hides the modal first, then runs the caller's action (if any). Listeners are
    // cleared each time so repeated Show calls don't accumulate handlers.
    private void Wire(Button button, Action action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            Hide();
            action?.Invoke();
        });
    }
}
