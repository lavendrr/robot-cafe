using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Single button used for every ingredient category (Base, MixIn, Topping). The category is
// set per-instance in the inspector and drives which ingredients the shared picker offers.
public class AddIngredientButton : MonoBehaviour
{
    [SerializeField] private IngredientPickerPanel pickerPanel;
    [SerializeField] private Button button;
    [SerializeField] private IngredientCategory category;

    private bool subscribed;

    void OnEnable()
    {
        // This button can be enabled (e.g. during menu/scene spawn) before DrinkEditorUI has
        // initialized its singleton. Wait for it rather than silently skipping the subscription,
        // otherwise the button would never refresh its interactable state again.
        StartCoroutine(SubscribeWhenReady());
    }

    IEnumerator SubscribeWhenReady()
    {
        while (DrinkEditorUI.Instance == null)
            yield return null;

        DrinkEditorUI.Instance.OnIngredientsChanged += RefreshInteractable;
        subscribed = true;
        RefreshInteractable();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (subscribed && DrinkEditorUI.Instance != null)
            DrinkEditorUI.Instance.OnIngredientsChanged -= RefreshInteractable;
        subscribed = false;
    }

    public void OnPressed()
    {
        pickerPanel.OnItemSelected = OnIngredientPicked;
        pickerPanel.Open(category, DrinkEditorUI.Instance.GetIngredients(category));
    }

    void OnIngredientPicked(IngredientData ingredient)
    {
        DrinkEditorUI.Instance.AddIngredient(ingredient, category);
    }

    void RefreshInteractable()
    {
        if (DrinkEditorUI.Instance == null) return;
        button.interactable = pickerPanel.HasEligibleItems(
            category, DrinkEditorUI.Instance.GetIngredients(category));
    }
}
