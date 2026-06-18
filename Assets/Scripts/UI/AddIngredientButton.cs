using UnityEngine;
using UnityEngine.UI;

public class AddIngredientButton : MonoBehaviour
{
    [SerializeField] private FuelPickerPanel pickerPanel;
    [SerializeField] private Button button;

    void OnEnable()
    {
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
        pickerPanel.OnItemSelected = OnIngredientPicked;
        var added = DrinkEditorUI.Instance.CurrentItem?.drink.comp.Keys;
        pickerPanel.Open(added);
    }

    void OnIngredientPicked(FuelType ingredient)
    {
        DrinkEditorUI.Instance.AddBaseIngredient(ingredient, 10f);
    }

    void RefreshInteractable()
    {
        var added = DrinkEditorUI.Instance.CurrentItem?.drink.comp.Keys;
        button.interactable = pickerPanel.HasEligibleItems(added);
    }
}
