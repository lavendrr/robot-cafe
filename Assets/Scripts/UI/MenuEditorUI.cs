using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using Unity.VisualScripting;
using System.Linq;
using System.Collections.Generic;

public class MenuEditorUI : MonoBehaviour
{
    [SerializeField]
    private GameObject drinkEditorUI, iconColumn, costColumn, nameColumn, iconPrefab, costPrefab, namePrefab;
    private List<GameObject> menuGridElements = new List<GameObject>();

    private void Start()
    {
        PopulateMenu();
    }

    public void Close()
    {
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
        }
    }

    private GameObject SpawnIconGroup(MenuItem item)
    {
       var iconGroup = Instantiate(iconPrefab, iconColumn.transform);
       iconGroup.GetComponent<MenuGridIconGroup>().Initialize(item.name, () =>
            {
                drinkEditorUI.SetActive(true);
                DrinkEditorUI.Instance.LoadItem(item);
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
