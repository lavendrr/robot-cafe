using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IngredientPickerPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private IngredientPickerButton buttonPrefab;

    public Action<FuelType> OnIngredientSelected;

    // Stub ingredient list
    private readonly FuelType[] stubIngredients =
    {
        FuelType.Unleaded,
        FuelType.Premium,
        FuelType.Diesel
    };

    void Awake()
    {
        BuildList();
        Close();
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void BuildList()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        foreach (var ingredient in stubIngredients)
        {
            var button = Instantiate(buttonPrefab, contentRoot);
            button.Initialize(ingredient, () =>
            {
                OnIngredientSelected?.Invoke(ingredient);
                Close();
            });
        }
    }
}
