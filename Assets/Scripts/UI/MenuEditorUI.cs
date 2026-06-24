using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using Unity.VisualScripting;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Assertions.Must;

public class MenuEditorUI : MonoBehaviour
{
    [SerializeField]
    private GameObject drinkEditorUI, iconColumn, costColumn, nameColumn, iconPrefab, costPrefab, namePrefab, newDrinkButtonPrefab;
    private List<GameObject> menuGridElements = new List<GameObject>();
    [SerializeField]
    private ErrorableButton saveMenuButton;

    private void Start()
    {
        PopulateMenu();
    }

    public void Close()
    {
        if (MenuManager.Instance.ListItems().Count() < 1)
        {
            saveMenuButton.FlashError("Menu cannot be empty!", 0.5f);
            return;
        }
        gameObject.SetActive(false);
    }

    public void ClearMenu()
    {
        if (menuGridElements.Count == 0)
            return;

        foreach (var element in menuGridElements)
        {
            Destroy(element);
        }
        menuGridElements.Clear();
    }

    public void PopulateMenu()
    {
        ClearMenu();
        MenuItem[] menuItems = MenuManager.Instance.ListItems();
        if (menuItems.Length > 0)
        {
            foreach (var item in menuItems)
            {
                menuGridElements.Add(SpawnIconGroup(item));
                menuGridElements.Add(SpawnCostText(item.cost));
                menuGridElements.Add(SpawnNameText(item.name));
            }
            if (menuItems.Length < 10)
            {
                var newDrinkButton = Instantiate(newDrinkButtonPrefab, nameColumn.transform);
                newDrinkButton.GetComponent<Button>().onClick.AddListener(() => {
                    drinkEditorUI.SetActive(true);
                    DrinkEditorUI.Instance.CreateNewItem();
                });
                menuGridElements.Add(newDrinkButton);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(iconColumn.transform.parent as RectTransform);
        }
    }

    private GameObject SpawnIconGroup(MenuItem item)
    {
       var iconGroup = Instantiate(iconPrefab, iconColumn.transform);
       iconGroup.GetComponent<MenuGridIconGroup>().Initialize(item.name, () =>
            {
                drinkEditorUI.SetActive(true);
                DrinkEditorUI.Instance.LoadItem(item);
            }, () =>
            {
                MenuManager.Instance.RemoveItem(item.name);
                PopulateMenu();
            });
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            iconGroup.transform as RectTransform
        );
        return iconGroup;
    }

    private GameObject SpawnCostText(int cost)
    {
        var costObj = Instantiate(costPrefab, costColumn.transform);
        if (costObj.TryGetComponent<TextMeshProUGUI>(out var costText))
        {
            costText.text = cost.ToString() + "C";
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                costText.rectTransform.parent as RectTransform
            );
            return costObj;
        }
        else
            Debug.LogError("[MenuEditorUI] Unable to get cost prefab text component.");
            return null;
    }

    private GameObject SpawnNameText(string name)
    {
        var nameObj = Instantiate(namePrefab, nameColumn.transform);
        if (nameObj.TryGetComponent<TextMeshProUGUI>(out var nameText))
        {
            nameText.text = name;
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                nameText.rectTransform.parent as RectTransform
            ); 
            return nameObj;
        }
        else
            Debug.LogError("[MenuEditorUI] Unable to get name prefab text component.");
            return null;
    }
}
