using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class IngredientPickerButton : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button button;

    public void Initialize(FuelType ingredient, Action onClick)
    {
        label.text = ingredient.ToString();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());
    }
}
