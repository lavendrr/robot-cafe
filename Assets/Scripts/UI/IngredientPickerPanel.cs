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
        Close();
    }

    public bool HasEligibleIngredients(IEnumerable<FuelType> excludeIngredients = null)
    {
        var excluded = excludeIngredients != null
            ? new HashSet<FuelType>(excludeIngredients)
            : new HashSet<FuelType>();

        foreach (var ingredient in stubIngredients)
            if (!excluded.Contains(ingredient))
                return true;

        return false;
    }

    public void Open(IEnumerable<FuelType> excludeIngredients = null)
    {
        BuildList(excludeIngredients);
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void BuildList(IEnumerable<FuelType> excludeIngredients = null)
    {
        var excluded = excludeIngredients != null
            ? new HashSet<FuelType>(excludeIngredients)
            : new HashSet<FuelType>();

        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        foreach (var ingredient in stubIngredients)
        {
            if (excluded.Contains(ingredient))
                continue;

            var button = Instantiate(buttonPrefab, contentRoot);
            button.Initialize(ingredient, () =>
            {
                OnIngredientSelected?.Invoke(ingredient);
                Close();
            });
        }
    }
}
