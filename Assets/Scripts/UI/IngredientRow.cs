using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientRow : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI LabelText;
    [SerializeField]
    private Button DecreaseButton, IncreaseButton;
    public IngredientData Ingredient;
    public IngredientCategory Category;

    // The label holds both name and portion (e.g. "Vanilla 3 shots" / "Unleaded 50%"). Rebuild
    // the row and its containing list so the resized text reflows immediately.
    public void SetLabel(string text)
    {
        LabelText.text = text;
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        if (transform.parent is RectTransform parent)
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
    }

    // Toggle the +/- buttons at the quantity bounds. Driven by DrinkEditorUI, which owns the
    // min/max and the current count.
    public void SetQuantityButtonsInteractable(bool canDecrease, bool canIncrease)
    {
        if (DecreaseButton != null) DecreaseButton.interactable = canDecrease;
        if (IncreaseButton != null) IncreaseButton.interactable = canIncrease;
    }

    public void RemoveIngredient()
    {
        if (Category == IngredientCategory.Base)
            DrinkEditorUI.Instance.RemoveBaseIngredient(Ingredient);
        else
            DrinkEditorUI.Instance.RemoveAddOn(Ingredient, Category);
    }

    // Hooked to the row's IncreaseButton / DecreaseButton. Base rows adjust their portion via the
    // cup slider; add-on rows adjust their discrete count.
    public void IncreaseQuantity()
    {
        if (Category == IngredientCategory.Base)
            DrinkEditorUI.Instance.ChangeBasePortion(Ingredient, +1);
        else
            DrinkEditorUI.Instance.ChangeAddOnQuantity(Ingredient, Category, +1);
    }

    public void DecreaseQuantity()
    {
        if (Category == IngredientCategory.Base)
            DrinkEditorUI.Instance.ChangeBasePortion(Ingredient, -1);
        else
            DrinkEditorUI.Instance.ChangeAddOnQuantity(Ingredient, Category, -1);
    }
}
