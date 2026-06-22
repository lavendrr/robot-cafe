using TMPro;
using UnityEngine;

public class IngredientRow : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI LabelText, PortionText;
    public IngredientData Ingredient;
    public IngredientCategory Category;

    public void RemoveIngredient()
    {
        if (Category == IngredientCategory.Base)
            DrinkEditorUI.Instance.RemoveBaseIngredient(Ingredient);
        else
            DrinkEditorUI.Instance.RemoveAddOn(Ingredient, Category);
    }
}
