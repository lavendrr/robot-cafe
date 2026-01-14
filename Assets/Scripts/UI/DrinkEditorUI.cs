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
    private GameObject IngredientRowPrefab, BasePanel;

    private List<GameObject> ingRows = new();

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
        var ingRow = Instantiate(IngredientRowPrefab, BasePanel.transform);
        // TODO: harden this code against null errors
        var textList = ingRow.GetComponentsInChildren<TextMeshProUGUI>();
        textList[0].text = fuelType.ToString();
        textList[1].text = initialPercent.ToString();
        ingRows.Add(ingRow);

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
        if (!CurrentItem.drink.comp.Remove(fuelType))
            return;

        NormalizeBaseIngredients();
        UpdateCupSlider();
        OnDrinkChanged?.Invoke();
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
            UpdateIngredientRowPortion(ingRows[i], (segments[i] * 100f).ToString());
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

    void UpdateIngredientRowPortion(GameObject ingRow, string text)
    {
        ingRow.GetComponentsInChildren<TextMeshProUGUI>()[1].text = text;
    }

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
