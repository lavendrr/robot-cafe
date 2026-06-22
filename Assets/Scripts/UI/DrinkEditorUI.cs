using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrinkEditorUI : MonoBehaviour
{
    public static DrinkEditorUI Instance { get; private set; }
    public MenuItem CurrentItem { get; private set; }
    private string originalItemName = "";
    [SerializeField]
    public MultiSliderController CupSlider;
    [SerializeField]
    private MenuEditorUI menuEditor;
    [SerializeField]
    private GameObject IngredientRowPrefab, BaseIngredientList, MixInIngredientList, ToppingIngredientList;
    [SerializeField]
    private TMP_InputField nameInputField;
    [SerializeField]
    private ErrorableButton saveDrinkButton;

    public event Action OnIngredientsChanged;

    private const int MinAddOnQuantity = 1;
    private const int MaxAddOnQuantity = 9;

    // Minimum portion (0..100) any single base ingredient may hold. Doubles as the slider's
    // handle padding (via MultiSliderController) and the +/- button step for base portions.
    public const int minimumBasePortion = 10;

    private readonly Dictionary<IngredientData, IngredientRow> baseRows    = new();
    private readonly Dictionary<IngredientData, IngredientRow> mixInRows   = new();
    private readonly Dictionary<IngredientData, IngredientRow> toppingRows = new();

    #region Initialization

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        if (CupSlider == null | IngredientRowPrefab == null | BaseIngredientList == null | MixInIngredientList == null | ToppingIngredientList == null | saveDrinkButton == null)
        {
            Debug.LogError("[DrinkEditorUI] GameObject references not properly set. Please set all references in the inspector panel.");
        }
    }

    void OnEnable()
    {
        if (CupSlider != null)
        {
            CupSlider.OnMultiSliderChanged -= UpdateRatios;
            CupSlider.OnMultiSliderChanged += UpdateRatios;
        }
    }

    void OnDisable()
    {
        if (CupSlider != null)
            CupSlider.OnMultiSliderChanged -= UpdateRatios;
        
        nameInputField.text = "";
        originalItemName = "";
        foreach (var rows in new[] { baseRows, mixInRows, toppingRows })
        {
            foreach (var ingRow in rows.Values)
                Destroy(ingRow.gameObject);
            rows.Clear();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void CreateNewItem()
    {
        CurrentItem = new MenuItem(
            "",
            new Dictionary<IngredientData, int>(),
            5
        );
    }

    public void LoadItem(MenuItem item)
    {
        CreateNewItem();
        SetItemName(item.name);
        originalItemName = item.name;
        CurrentItem.cost = item.cost;
        foreach (var ing in item.drink.bases)
        {
            AddBaseIngredient(ing.Key, ing.Value);
        }
        foreach (var ing in item.drink.mixIns)
        {
            AddAddOn(ing.Key, IngredientCategory.MixIn, ing.Value);
        }
        foreach (var ing in item.drink.toppings)
        {
            AddAddOn(ing.Key, IngredientCategory.Topping, ing.Value);
        }
    }

    public void SaveCurrentItem()
    {
        if (CurrentItem == null)
        {
            saveDrinkButton.FlashError("Error creating drink object!", 0.5f);
            return;
        }

        if (!IsValidDrink(CurrentItem))
        {
            return;
        }

        MenuItem clonedDrink = CloneMenuItem(CurrentItem);
        string[] furnitureNames = System.Array.ConvertAll<FurnitureData, string>(clonedDrink.requiredFurniture.ToArray(), f => f.name);
        if (originalItemName != "")
        {
            MenuManager.Instance.OverwriteItem(originalItemName, clonedDrink);
        } else
        {
            MenuManager.Instance.AddItem(clonedDrink.name, clonedDrink.drink.bases, clonedDrink.cost, furnitureNames);
        }
        
        menuEditor.PopulateMenu();
        Close();
    }


    #endregion

    #region Base Ingredients

    public bool AddBaseIngredient(IngredientData ingredient, int amount)
    {
        // Add to MenuItem
        if (CurrentItem.drink.bases.ContainsKey(ingredient))
            return false;

        CurrentItem.drink.bases.Add(ingredient, amount);

        // Add ingredient row to panel
        var ingRow = Instantiate(IngredientRowPrefab, BaseIngredientList.transform);
        var ingRowScript = ingRow.GetComponent<IngredientRow>();
        ingRowScript.Ingredient = ingredient;
        ingRowScript.Category = IngredientCategory.Base;
        ingRowScript.SetLabel(FormatRowLabel(ingredient, IngredientCategory.Base, amount));
        baseRows.Add(ingredient, ingRowScript);

        // Add segment and handle in Cup Slider
        UpdateCupSlider();
        OnIngredientsChanged?.Invoke();
        return true;
    }

    public void SetBaseIngredientValue(IngredientData ingredient, int percent)
    {
        if (!CurrentItem.drink.bases.ContainsKey(ingredient))
            return;

        CurrentItem.drink.bases[ingredient] = Mathf.Clamp(percent, 0, 100);
        NormalizeBaseIngredients();
    }

    public void RemoveBaseIngredient(IngredientData ingredient)
    {
        // Delete ingredient row
        if (baseRows.TryGetValue(ingredient, out var row))
        {
            Destroy(row.gameObject);
            baseRows.Remove(ingredient);
        }

        // Remove ingredient from drink bases
        if (!CurrentItem.drink.bases.Remove(ingredient))
            return;

        // Update cup to rebuild slider
        NormalizeBaseIngredients();
        UpdateCupSlider();
        OnIngredientsChanged?.Invoke();
    }

    // Nudge a base ingredient's portion by one step of minimumBasePortion in the given direction
    // (+1 / -1). Routed through the cup slider so handle positions and base values stay in sync;
    // the slider's padding constraint enforces the per-ingredient minimum. UpdateRatios (fired by
    // the slider) writes the new values back into drink.bases and refreshes the row labels.
    public void ChangeBasePortion(IngredientData ingredient, int direction)
    {
        List<IngredientData> keys = new(CurrentItem.drink.bases.Keys);
        int index = keys.IndexOf(ingredient);
        if (index < 0)
            return;

        CupSlider.AdjustSegment(index, Mathf.Sign(direction) * minimumBasePortion / 100f);
    }

    private void UpdateCupSlider()
    {
        CupSlider.Build();
    }

    private void UpdateRatios()
    {
        if (CurrentItem == null || CupSlider == null || CupSlider.segmentPercentages == null)
            return;

        List<IngredientData> keys = new(CurrentItem.drink.bases.Keys);
        List<float> segments = CupSlider.segmentPercentages;

        // Map each segment percentage (0..1) to the corresponding ingredient (0..100)
        for (int i = 0; i < keys.Count && i < segments.Count; i++)
        {
            int percent = (int)(segments[i] * 100f);
            SetBaseIngredientValue(keys[i], percent);
            if (!baseRows.TryGetValue(keys[i], out var row))
                continue;
            row.SetLabel(FormatRowLabel(keys[i], IngredientCategory.Base, percent));
            CupSlider.GetSegmentAdjustable(i, out bool canShrink, out bool canGrow);
            row.SetQuantityButtonsInteractable(canShrink, canGrow);
        }
    }

    #endregion

    #region Add-Ons

    public bool AddAddOn(IngredientData addOn, IngredientCategory category, int quantity = 1)
    {
        var dict = GetCategoryDict(category);
        if (dict.ContainsKey(addOn))
            return false;

        dict[addOn] = quantity;

        // Add-on row to the category's list. Add-ons are discrete counts shown in the label
        // (e.g. "Vanilla 3 shots"), and they don't participate in the cup slider.
        var ingRow = Instantiate(IngredientRowPrefab, GetListForCategory(category).transform);
        var ingRowScript = ingRow.GetComponent<IngredientRow>();
        ingRowScript.Ingredient = addOn;
        ingRowScript.Category = category;
        ingRowScript.SetLabel(FormatRowLabel(addOn, category, quantity));
        ingRowScript.SetQuantityButtonsInteractable(
            quantity > MinAddOnQuantity, quantity < MaxAddOnQuantity);
        GetRowsForCategory(category).Add(addOn, ingRowScript);

        OnIngredientsChanged?.Invoke();
        return true;
    }

    // Adjust a discrete add-on count by delta (e.g. +1 / -1 from the row's buttons), clamped to
    // [MinAddOnQuantity, MaxAddOnQuantity]. Removal to zero is handled by the row's remove button,
    // not by decrementing past the minimum.
    public void ChangeAddOnQuantity(IngredientData addOn, IngredientCategory category, int delta)
    {
        var dict = GetCategoryDict(category);
        if (!dict.TryGetValue(addOn, out int current))
            return;

        int updated = Mathf.Clamp(current + delta, MinAddOnQuantity, MaxAddOnQuantity);
        if (updated == current)
            return;

        dict[addOn] = updated;
        if (GetRowsForCategory(category).TryGetValue(addOn, out var row))
        {
            row.SetLabel(FormatRowLabel(addOn, category, updated));
            row.SetQuantityButtonsInteractable(
                updated > MinAddOnQuantity, updated < MaxAddOnQuantity);
        }
    }

    // Builds the consolidated row label: "<name> <amount><unit>". Base ingredients always use a
    // bare "%" (no space) for the portion; add-ons use the ingredient's singular/plural unit.
    private static string FormatRowLabel(IngredientData ingredient, IngredientCategory category, int amount)
    {
        if (category == IngredientCategory.Base)
            return amount + " pct. " + ingredient.ingredientName;

        string unit = (amount > 1) ? ingredient.unitPlural : ingredient.unitSingular;
        return amount + " " + unit + " " + ingredient.ingredientName;
    }

    public void RemoveAddOn(IngredientData addOn, IngredientCategory category)
    {
        var rows = GetRowsForCategory(category);
        if (rows.TryGetValue(addOn, out var row))
        {
            Destroy(row.gameObject);
            rows.Remove(addOn);
        }

        GetCategoryDict(category).Remove(addOn);
        OnIngredientsChanged?.Invoke();
    }

    // Category-routed entry points shared by every AddIngredientButton (Base/MixIn/Topping).
    // Reads are uniform — every category is the same dict shape. Only the *add* behavior
    // differs: Base ingredients are normalized into a ratio and drive the cup slider, whereas
    // add-ons are discrete counts.
    public void AddIngredient(IngredientData ingredient, IngredientCategory category)
    {
        if (category == IngredientCategory.Base)
            AddBaseIngredient(ingredient, 10);
        else
            AddAddOn(ingredient, category);
    }

    public IEnumerable<IngredientData> GetIngredients(IngredientCategory category) =>
        CurrentItem != null ? GetCategoryDict(category).Keys : null;

    private Dictionary<IngredientData, int> GetCategoryDict(IngredientCategory category) => category switch
    {
        IngredientCategory.Base    => CurrentItem.drink.bases,
        IngredientCategory.MixIn   => CurrentItem.drink.mixIns,
        IngredientCategory.Topping => CurrentItem.drink.toppings,
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };

    private Dictionary<IngredientData, IngredientRow> GetRowsForCategory(IngredientCategory category) => category switch
    {
        IngredientCategory.Base    => baseRows,
        IngredientCategory.MixIn   => mixInRows,
        IngredientCategory.Topping => toppingRows,
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };

    private GameObject GetListForCategory(IngredientCategory category) => category switch
    {
        IngredientCategory.Base    => BaseIngredientList,
        IngredientCategory.MixIn   => MixInIngredientList,
        IngredientCategory.Topping => ToppingIngredientList,
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };

    #endregion

    #region Metadata

    public void SetItemName(string name)
    {
        CurrentItem.name = name.Trim();
        nameInputField.text = name.Trim();
    }

    public void SetItemCost(int cost)
    {
        CurrentItem.cost = Mathf.Max(1, cost);
    }

    #endregion

    #region Helpers

    bool IsValidDrink(MenuItem item)
    {
        if (item.drink.bases.Count < 1)
        {
            saveDrinkButton.FlashError("No base ingredients!", 0.5f);
            return false;
        }

        if (string.IsNullOrWhiteSpace(item.name))
        {
            saveDrinkButton.FlashError("Missing name!", 0.5f);
            return false;
        }

        if (item.name != originalItemName && !MenuManager.Instance.IsNameUnique(item.name))
        {
            saveDrinkButton.FlashError("Name already exists!", 0.5f);
            return false;
        }

        return true;
    }

    MenuItem CloneMenuItem(MenuItem source)
    {
        var clone = new MenuItem(
            source.name,
            new Dictionary<IngredientData, int>(source.drink.bases),
            source.cost
        );

        clone.drink.mixIns  = new Dictionary<IngredientData, int>(source.drink.mixIns);
        clone.drink.toppings = new Dictionary<IngredientData, int>(source.drink.toppings);
        return clone;
    }

    void NormalizeBaseIngredients()
    {
        if (CurrentItem.drink.bases.Count == 0)
            return;

        float total = 0f;
        foreach (var v in CurrentItem.drink.bases.Values)
            total += v;

        if (Mathf.Approximately(total, 0f))
        {
            int even = Mathf.RoundToInt(100f / CurrentItem.drink.bases.Count);
            List<IngredientData> keys = new(CurrentItem.drink.bases.Keys);
            foreach (var k in keys)
                CurrentItem.drink.bases[k] = even;
            return;
        }

        float scale = 100f / total;
        List<IngredientData> normalizeKeys = new(CurrentItem.drink.bases.Keys);

        // Round all values to integers
        int roundedTotal = 0;
        foreach (var k in normalizeKeys)
        {
            CurrentItem.drink.bases[k] = Mathf.RoundToInt(CurrentItem.drink.bases[k] * scale);
            roundedTotal += CurrentItem.drink.bases[k];
        }

        // Redistribute rounding error to maintain 100% sum
        int remainder = 100 - roundedTotal;
        if (remainder != 0)
        {
            CurrentItem.drink.bases[normalizeKeys[0]] += remainder;
        }
    }

    #endregion
}
