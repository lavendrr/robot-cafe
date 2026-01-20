using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting.AssemblyQualifiedNameParser;

public class MenuEditorUI : MonoBehaviour
{
    [SerializeField]
    private GameObject iconColumn, costColumn, nameColumn, iconPrefab, costPrefab, namePrefab;

    private void Start()
    {
        PopulateMenu();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void PopulateMenu()
    {
        MenuItem[] menuItems = MenuManager.Instance.ListItems();
        if (menuItems.Length > 0)
        {
            foreach (var item in menuItems)
            {
                SpawnIconGroup();
                SpawnCostText(item.cost);
                SpawnNameText(item.name);
            }
        }
    }

    private void SpawnIconGroup()
    {
       var iconGroup = Instantiate(iconPrefab, iconColumn.transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            iconGroup.transform as RectTransform
        );
    }

    private void SpawnCostText(int cost)
    {
        if (Instantiate(costPrefab, costColumn.transform).TryGetComponent<TextMeshProUGUI>(out var costText))
        {
            costText.text = cost.ToString() + "C";
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                costText.rectTransform.parent as RectTransform
            );
        }
        else
            Debug.LogError("[MenuEditorUI] Unable to get cost prefab text component.");
    }

    private void SpawnNameText(string name)
    {
        if (Instantiate(namePrefab, nameColumn.transform).TryGetComponent<TextMeshProUGUI>(out var nameText))
        {
            nameText.text = name;
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                nameText.rectTransform.parent as RectTransform
            ); 
        }
        else
            Debug.LogError("[MenuEditorUI] Unable to get name prefab text component.");
    }
}
