using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;

// TODO - change the color thing from colliders to cursor hover to avoid overlapping cell issue

public class PlanningManager : MonoBehaviour
{
    public static PlanningManager Instance { get; private set; }

    [SerializeField]
    private GameObject gridObj, catalogGridContent, tooltipPanel, cellPrefab, catalogIconPrefab, draggableItemPrefab;

    public GameObject[,] gridArray;
    [SerializeField]
    public List<FurnitureObject> testFurniture;

    public DraggableItem currentItem = null;

    // Start is called before the first frame update
    void Start()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        CreateBlueprintGrid();
        PopulateCatalogPanel();
    }

    private void CreateBlueprintGrid()
    {
        LevelLayout layout = SaveManager.Instance.GetCafeLayout();
        if (layout == null)
        {
            Debug.LogError("LevelLayout retrieved from SaveManager was null.");
            return;
        }
        int rows = layout.dimensions.rows;
        int cols = layout.dimensions.cols;

        gridObj.GetComponent<GridLayoutGroup>().constraintCount = rows;

        gridArray = new GameObject[rows, cols];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                // Create an instance of the prefab and child it to the gridRoot
                GameObject clone = Instantiate(cellPrefab, gridObj.transform);
                clone.GetComponent<GridSlot>().SetCoords((row, col));
                // Add names to the cells to help differentiate the generated objects
                clone.name = string.Format("Cell {0}, {1}", row, col);
                // Save it to the corresponding spot in the array
                gridArray[row, col] = clone;
                // Color it orange if it's a delivery tile
                if (layout.deliveryTileCoords.col == col && layout.deliveryTileCoords.row == row)
                {
                    clone.GetComponent<Image>().color = Color.yellow;
                }
            }
        }

        LoadFurnitureIntoGrid(layout);
    }

    public void LoadFurnitureIntoGrid(LevelLayout layout)
    {
        foreach (var element in layout.elements)
        {
            int row = element.rootGridCoord.row;
            int col = element.rootGridCoord.col;
            if (row >= 0 && row < layout.dimensions.rows && col >= 0 && col < layout.dimensions.cols)
            {
                var cell = gridArray[row, col];
                GameObject spawnedItem = Instantiate(draggableItemPrefab, gridObj.transform);
                spawnedItem.GetComponent<Image>().sprite = element.furnitureObject.catalogSprite;
                DraggableItem draggableItem = spawnedItem.GetComponent<DraggableItem>();
                draggableItem.Init(element.furnitureObject);
                draggableItem.rotation = element.rotation;
                draggableItem.image.sprite = element.furnitureObject.gridSprites[0];
                if (!cell.GetComponent<GridSlot>().AttemptItemSlot(spawnedItem, draggableItem.screenRotation))
                {
                    Debug.LogError("Failed to slot " + element.furnitureObject.furnitureName + " into cell at (" + row + ", " + col + ")");
                    Destroy(spawnedItem);
                }
            }
        }
    }

    private void PopulateCatalogPanel()
    {
        Dictionary<FurnitureObject, int> catalogFurniture = new Dictionary<FurnitureObject, int>();//SaveManager.Instance.GetCatalogList();
        // For each furniture object in the test list, add a value of 5 to the dictionary
        foreach (FurnitureObject f in testFurniture)
        {
            catalogFurniture.Add(f, f.cost);
        }
        foreach (KeyValuePair<FurnitureObject, int> element in catalogFurniture)
        {
            // Spawn a CatalogItem prefab under the catalog grid Content object
            GameObject item = Instantiate(catalogIconPrefab, catalogGridContent.transform);
            // Initialize the CatalogItem with the furniture icon and name
            item.GetComponent<GridItemSpawn>().furnitureType = element.Key;
            item.GetComponent<GridItemSpawn>().draggableItemPrefab = draggableItemPrefab;
            item.GetComponent<Image>().sprite = element.Key.catalogSprite;
            item.transform.Find("NumberPip").GetComponentInChildren<TextMeshProUGUI>().text = element.Value.ToString() + "c";
            item.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = element.Key.furnitureName;
        }
    }

    public void ShowTooltip(FurnitureObject furnitureObject)
    {
        tooltipPanel.SetActive(true);
        tooltipPanel.transform.Find("ToolTipTitle").GetComponent<TextMeshProUGUI>().text = furnitureObject.furnitureName;
        tooltipPanel.transform.Find("ToolTipText").GetComponent<TextMeshProUGUI>().text = ParseText(furnitureObject.tooltipText);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel.GetComponent<RectTransform>());
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }

    public string ParseText(string input)
    {
        return input.Replace("\\n", "\n");
    }

    public List<CafeElement> GetFinalGrid()
    {
        List<CafeElement> output = new();

        foreach (GameObject cell in gridArray)
        {
            if (cell.GetComponent<GridSlot>().GetOccupiedStatus())
            {
                if (cell.GetComponentInChildren<DraggableItem>() == null)
                {
                    continue; // This is just an offset cell, not a root cell
                }
                var coords = cell.GetComponent<GridSlot>().GetCoords();
                output.Add(new CafeElement
                {
                    furnitureObject = cell.GetComponentInChildren<DraggableItem>().furnitureObject,
                    rootGridCoord = new GridCoord { col = coords.Item2, row = coords.Item1 },
                    rotation = cell.GetComponentInChildren<DraggableItem>().rotation
                });
            }
        }
        return output;
    }

    public void SetCurrentItem(DraggableItem item)
    {
        currentItem = item;
    }

    void OnRotateRight()
    {
        if (currentItem != null)
        {
            currentItem.RotateClockwise();
        }
    }

    void OnRotateLeft()
    {
        if (currentItem != null)
        {
            currentItem.RotateCounterclockwise();
        }
    }
}
