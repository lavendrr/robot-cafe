using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MenuGridIconGroup : MonoBehaviour
{
    [SerializeField] private Button deleteButton, editButton;
    public string itemName;

    public void Initialize(string _itemName, Action editOnClick)
    {
        itemName = _itemName;
        editButton.onClick.RemoveAllListeners();
        editButton.onClick.AddListener(() => editOnClick());
    }
}
