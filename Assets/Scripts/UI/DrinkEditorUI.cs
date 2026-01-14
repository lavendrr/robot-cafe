using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

public class DrinkEditorUI : MonoBehaviour
{
    public static DrinkEditorUI Instance { get; private set; }
    public MenuItem CurrentItem { get; private set; }
    [SerializeField]
    public MultiSliderController CupSlider;
    [SerializeField]
    private GameObject IngredientRowPrefab, BaseIngredientList;

    private List<IngredientRow> ingRows = new();

    public Action OnDrinkChanged;
    public Action OnToppingsChanged;
    public Action OnMetaChanged;

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
        CreateNewItem();
        AddBaseIngredient(FuelType.Premium, 50f);
        AddBaseIngredient(FuelType.Unleaded, 50f);
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

    public void CreateNewItem()
    {
        CurrentItem = new MenuItem(
            "New Drink",
            new Dictionary<FuelType, float>(),
            5
        );

        NotifyAll();
    }

    public void LoadItem(MenuItem item)
    {
        CurrentItem = item;
        NotifyAll();
    }

    #endregion

    #region Base Ingredients

    public bool AddBaseIngredient(FuelType fuelType, float initialPercent = -1f)
    {
        // Add to MenuItem
        if (CurrentItem.drink.comp.ContainsKey(fuelType))
            return false;

        if (initialPercent < 0)
            initialPercent = GetDefaultSplitValue();

        CurrentItem.drink.comp.Add(fuelType, initialPercent);

        // Add ingredient row to panel
        var ingRow = Instantiate(IngredientRowPrefab, BaseIngredientList.transform);
        var ingRowScript = ingRow.GetComponent<IngredientRow>();
        ingRowScript.LabelText.text = fuelType.ToString();
        ingRowScript.PortionText.text = initialPercent.ToString();
        ingRows.Add(ingRowScript);

        // Add segment and handle in Cup Slider
        NormalizeBaseIngredients();
        UpdateCupSlider();
        OnDrinkChanged?.Invoke();
        return true;
    }

    public void SetBaseIngredientValue(FuelType fuelType, float percent)
    {
        if (!CurrentItem.drink.comp.ContainsKey(fuelType))
            return;

        CurrentItem.drink.comp[fuelType] = Mathf.Clamp(percent, 0f, 100f);
        NormalizeBaseIngredients();
        OnDrinkChanged?.Invoke();
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
        OnDrinkChanged?.Invoke();

        // Update remaining ingredient rows
        UpdateRatios();
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
        }
    }

    #endregion

    #region Toppings

    public bool AddTopping(string topping)
    {
        if (CurrentItem.drink.toppings.Contains(topping))
            return false;

        CurrentItem.drink.toppings.Add(topping);
        OnToppingsChanged?.Invoke();
        return true;
    }

    public void RemoveTopping(string topping)
    {
        if (CurrentItem.drink.toppings.Remove(topping))
        {
            OnToppingsChanged?.Invoke();
        }
    }

    #endregion

    #region Metadata

    public void SetItemName(string name)
    {
        CurrentItem.name = name;
        OnMetaChanged?.Invoke();
    }

    public void SetItemCost(int cost)
    {
        CurrentItem.cost = Mathf.Max(1, cost);
        OnMetaChanged?.Invoke();
    }

    #endregion

    #region Helpers
    float GetDefaultSplitValue()
    {
        int count = CurrentItem.drink.comp.Count + 1;
        return 100f / count;
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
    }

    void NotifyAll()
    {
        OnDrinkChanged?.Invoke();
        OnToppingsChanged?.Invoke();
        OnMetaChanged?.Invoke();
    }

    #endregion
}
