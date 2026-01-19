using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

public class DrinkEditorUI : MonoBehaviour
{
    public static DrinkEditorUI Instance { get; private set; }
    public MenuItem CurrentItem { get; private set; }
    [SerializeField]
    public MultiSliderController CupSlider;
    [SerializeField]
    private GameObject IngredientRowPrefab, BaseIngredientList;
    [SerializeField]
    private ErrorableButton saveDrinkButton;

    private List<IngredientRow> ingRows = new();

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
        if (CupSlider == null | IngredientRowPrefab == null | BaseIngredientList == null | saveDrinkButton == null)
        {
            Debug.LogError("[DrinkEditorUI] GameObject references not properly set. Please set all references in the inspector panel.");
        }
        CreateNewItem();
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
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Open()
    {
        gameObject.SetActive(true);
    }

    void Close()
    {
        gameObject.SetActive(false);
    }

    public void CreateNewItem()
    {
        CurrentItem = new MenuItem(
            "",
            new Dictionary<FuelType, float>(),
            5
        );
    }

    public void LoadItem(MenuItem item)
    {
        CurrentItem = item;
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
        string[] furnitureNames = System.Array.ConvertAll<FurnitureObject, string>(clonedDrink.requiredFurniture.ToArray(), f => f.name);
        MenuManager.Instance.AddItem(clonedDrink.name, clonedDrink.drink.comp, clonedDrink.cost, furnitureNames);
        Close();
    }


    #endregion

    #region Base Ingredients

    public bool AddBaseIngredient(FuelType fuelType)
    {
        // Add to MenuItem
        if (CurrentItem.drink.comp.ContainsKey(fuelType))
            return false;

        CurrentItem.drink.comp.Add(fuelType, 10f);

        // Add ingredient row to panel
        var ingRow = Instantiate(IngredientRowPrefab, BaseIngredientList.transform);
        var ingRowScript = ingRow.GetComponent<IngredientRow>();
        ingRowScript.LabelText.text = fuelType.ToString();
        ingRows.Add(ingRowScript);

        // Add segment and handle in Cup Slider
        UpdateCupSlider();
        return true;
    }

    public void SetBaseIngredientValue(FuelType fuelType, float percent)
    {
        if (!CurrentItem.drink.comp.ContainsKey(fuelType))
            return;

        CurrentItem.drink.comp[fuelType] = Mathf.Clamp(percent, 0f, 100f);
        NormalizeBaseIngredients();
    }

    public void RemoveIngredient(string ingName)
    {
        if (Enum.TryParse(typeof(FuelType), ingName, true, out object result))
        {
            RemoveBaseIngredient((FuelType)result);
        } else {
            Debug.LogWarning("Removing ingredients for any type other than Fuel not supported yet.");
        }
    }

    public void RemoveBaseIngredient(FuelType fuelType)
    {
        // Delete ingredient row
        int ingIndex = CurrentItem.drink.comp.Keys.ToList().IndexOf(fuelType);
        Destroy(ingRows[ingIndex].gameObject);
        ingRows.RemoveAt(ingIndex);

        // Remove ingredient from drink comp
        if (!CurrentItem.drink.comp.Remove(fuelType))
            return;

        // Update cup to rebuild slider
        NormalizeBaseIngredients();
        UpdateCupSlider();
    }

    private void UpdateCupSlider()
    {
        CupSlider.Build();
    }

    private void UpdateRatios()
    {
        if (CurrentItem == null || CupSlider == null || CupSlider.segmentPercentages == null)
            return;

        List<FuelType> keys = new(CurrentItem.drink.comp.Keys);
        List<float> segments = CupSlider.segmentPercentages;

        // Map each segment percentage (0..1) to the corresponding ingredient (0..100)
        for (int i = 0; i < keys.Count && i < segments.Count; i++)
        {
            SetBaseIngredientValue(keys[i], segments[i] * 100f);
            ingRows[i].PortionText.text = (segments[i] * 100f).ToString();
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                ingRows[i].PortionText.rectTransform.parent as RectTransform
            );
        }
    }

    #endregion

    #region Toppings

    public bool AddTopping(string topping)
    {
        if (CurrentItem.drink.toppings.Contains(topping))
            return false;

        CurrentItem.drink.toppings.Add(topping);
        return true;
    }

    public void RemoveTopping(string topping)
    {
        CurrentItem.drink.toppings.Remove(topping);
    }

    #endregion

    #region Metadata

    public void SetItemName(string name)
    {
        CurrentItem.name = name.Trim();
    }

    public void SetItemCost(int cost)
    {
        CurrentItem.cost = Mathf.Max(1, cost);
    }

    #endregion

    #region Helpers

    bool IsValidDrink(MenuItem item)
    {
        if (string.IsNullOrWhiteSpace(item.name))
        {
            saveDrinkButton.FlashError("Missing name!", 0.5f);
            return false;
        }

        if (item.drink.comp.Count < 1)
        {
            saveDrinkButton.FlashError("No base ingredients!", 0.5f);
            return false;
        }

        return true;
    }

    MenuItem CloneMenuItem(MenuItem source)
{
    var compCopy = new Dictionary<FuelType, float>(source.drink.comp);
    var clone = new MenuItem(
        source.name,
        compCopy,
        source.cost
    );

    clone.drink.toppings = new List<string>(source.drink.toppings);
    return clone;
}

    void NormalizeBaseIngredients()
    {
        if (CurrentItem.drink.comp.Count == 0)
            return;

        float total = 0f;
        foreach (var v in CurrentItem.drink.comp.Values)
            total += v;

        if (Mathf.Approximately(total, 0f))
        {
            float even = 100f / CurrentItem.drink.comp.Count;
            List<FuelType> keys = new(CurrentItem.drink.comp.Keys);
            foreach (var k in keys)
                CurrentItem.drink.comp[k] = even;
            return;
        }

        float scale = 100f / total;
        List<FuelType> normalizeKeys = new(CurrentItem.drink.comp.Keys);
        foreach (var k in normalizeKeys)
            CurrentItem.drink.comp[k] *= scale;

        // Round all values to integers
        int roundedTotal = 0;
        foreach (var k in normalizeKeys)
        {
            CurrentItem.drink.comp[k] = Mathf.Round(CurrentItem.drink.comp[k]);
            roundedTotal += (int)CurrentItem.drink.comp[k];
        }

        // Redistribute rounding error to maintain 100% sum
        int remainder = 100 - roundedTotal;
        if (remainder != 0)
        {
            CurrentItem.drink.comp[normalizeKeys[0]] += remainder;
        }
    }

    #endregion
}
