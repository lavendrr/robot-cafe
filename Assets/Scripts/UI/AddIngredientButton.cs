using UnityEngine;

public class AddIngredientButton : MonoBehaviour
{
    [SerializeField] private IngredientPickerPanel pickerPanel;

    public void OnPressed()
    {
        pickerPanel.OnIngredientSelected = OnIngredientPicked;
        pickerPanel.Open();
    }

    void OnIngredientPicked(FuelType ingredient)
    {
        DrinkEditorUI.Instance.AddBaseIngredient(ingredient, 10f);
    }
}
