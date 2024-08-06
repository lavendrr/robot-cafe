using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using System.Linq;

// TODO - change the color thing from colliders to cursor hover to avoid overlapping cell issue

public class PlanningManager : MonoBehaviour
{
    public static PlanningManager Instance { get; private set; }

    [SerializeField]
    private GameObject gridObj, cellPrefab;

    [SerializeField]
    private int rows, cols;

    public GameObject[,] gridArray;

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
    }

    // Update is called once per frame
    void Update()
    {

    }
}
