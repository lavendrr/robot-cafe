using UnityEngine;
using UnityEngine.UI;

public class AddAddOnButton : MonoBehaviour
{
    [SerializeField] private AddOnPickerPanel pickerPanel;
    [SerializeField] private Button button;
    [SerializeField] private AddOnCategory category;

    void OnEnable()
    {
        if (DrinkEditorUI.Instance == null) return;
        DrinkEditorUI.Instance.OnIngredientsChanged += RefreshInteractable;
        RefreshInteractable();
    }

    void OnDisable()
    {
        if (DrinkEditorUI.Instance != null)
            DrinkEditorUI.Instance.OnIngredientsChanged -= RefreshInteractable;
    }

    public void OnPressed()
    {
        pickerPanel.OnItemSelected = OnAddOnPicked;
        pickerPanel.Open(category, DrinkEditorUI.Instance.GetAddOns(category));
    }

    void OnAddOnPicked(AddOnData addOn)
    {
        DrinkEditorUI.Instance.AddAddOn(addOn, category);
    }

    void RefreshInteractable()
    {
        if (DrinkEditorUI.Instance == null) return;
        button.interactable = pickerPanel.HasEligibleItems(
            category, DrinkEditorUI.Instance.GetAddOns(category));
    }
}
