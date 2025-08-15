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
    private GameObject gridObj, cellPrefab;

    [SerializeField]
    private int rows, cols;

    public GameObject[,] gridArray;
    [SerializeField]
    private GameObject storageIconPrefab, storageGridContent, draggableItemPrefab;
    [SerializeField]
    public List<FurnitureObject> testFurniture;

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
            }
        }

        PopulateStorageGrid();
    }

    private void PopulateStorageGrid()
    {
        Dictionary<FurnitureObject, int> ownedFurniture = new Dictionary<FurnitureObject, int>();//SaveManager.Instance.GetStorageDict();
        // For each furniture object in the test list, add a value of 5 to the dictionary
        foreach (FurnitureObject f in testFurniture)
        {
            ownedFurniture.Add(f, 5);
        }
        foreach (KeyValuePair<FurnitureObject, int> element in ownedFurniture)
        {
            // Spawn a StorageItem prefab under the storage grid Content object
            GameObject item = Instantiate(storageIconPrefab, storageGridContent.transform);
            // Initialize the StorageItem with the furniture icon and name
            item.GetComponent<GridItemSpawn>().furnitureType = element.Key;
            item.GetComponent<GridItemSpawn>().draggableItemPrefab = draggableItemPrefab;
            item.GetComponent<Image>().sprite = element.Key.sprite;
            item.transform.Find("NumberPip").GetComponentInChildren<TextMeshProUGUI>().text = element.Value.ToString();
            item.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = element.Key.furnitureName;
        }
    }

    public List<FurnitureObject> GetFinalGrid()
    {
        List<FurnitureObject> output = new();

        foreach (GameObject cell in gridArray)
        {
            if (cell.GetComponent<GridSlot>().GetOccupiedStatus())
            {
                output.Add(cell.GetComponentInChildren<DraggableItem>().furnitureObject);
            }
        }

        return output;
    }
}
