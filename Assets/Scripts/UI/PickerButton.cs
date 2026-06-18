using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickerButton : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button button;

    public void Initialize(string labelText, Action onClick)
    {
        label.text = labelText;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());
    }
}
