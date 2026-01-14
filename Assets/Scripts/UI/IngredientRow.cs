using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

public class IngredientRow : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI LabelText, PortionText;

    public void RemoveIngredient()
    {
        DrinkEditorUI.Instance.RemoveIngredient(LabelText.text);
    }
}
