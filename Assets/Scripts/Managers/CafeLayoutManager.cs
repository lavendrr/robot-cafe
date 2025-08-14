using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class CafeLayoutManager : MonoBehaviour
{
    public static CafeLayoutManager Instance { get; private set; }

    private bool _prevTestInEditor = false;

    [SerializeField]
    public GameObject CafeRoot;
    public float GridCellSize = 1.0f;
    public bool TestInEditor = false;
    public (int rows, int cols) FloorDimensions = (6, 10);
    public GameObject FloorPrefab; // TODO: move these to a data/constants file
    public GameObject StraightWallPrefab;
    public GameObject CornerWallPrefab;
    public List<CafeElement> TestLayout = new List<CafeElement>();

    private void Start()
    {
        // If there is an instance, and it's not me, delete myself
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        #if !UNITY_EDITOR
        if (SaveManager.Instance.GetCafeLayout() != null)
        {
            DestroyCafeFurniture();
            PopulateCafeLevel();
        }
        #endif
    }

    private Vector3 ConvertGridToWorldPosition(int col, int row)
    {
        if (CafeRoot == null)
            return Vector3.zero;

        Vector3 rootPos = CafeRoot.transform.position;
        float worldX = rootPos.x - col * GridCellSize - GridCellSize / 2;
        float worldY = rootPos.y;
        float worldZ = rootPos.z + row * GridCellSize + GridCellSize / 2;
        return new Vector3(worldX, worldY, worldZ);
    }

    public void PopulateFloorTiles()
    {
        // Instantiate floors to cover the entire area
        if (FloorPrefab == null)
        {
            Debug.LogWarning("FloorPrefab not assigned in CafeLayoutManager.");
            return;
        }

        for (int row = 0; row < FloorDimensions.rows; row++)
        {
            for (int col = 0; col < FloorDimensions.cols; col++)
            {
                Vector3 floorPosition = ConvertGridToWorldPosition(col, row);
                GameObject floorInstance = Instantiate(FloorPrefab, floorPosition, Quaternion.identity);
                floorInstance.name = $"Floor_{row}_{col}";
                if (CafeRoot != null)
                {
                    floorInstance.transform.SetParent(CafeRoot.transform);
                }
            }
        }
    }

    public void PopulateWalls()
    {
        if (StraightWallPrefab == null || CornerWallPrefab == null)
        {
            Debug.LogWarning("Wall prefabs not assigned in CafeLayoutManager.");
            return;
        }

        int width = FloorDimensions.cols;
        int height = FloorDimensions.rows;
        GameObject wallPrefab;
        for (int col = 0; col < width; col++)
        {
            for (int row = 0; row < height; row++)
            {
                // Check if the tile is an edge tile
                bool isLeftEdge = (col == 0);
                bool isRightEdge = (col == width - 1);
                bool isTopEdge = (row == 0);
                bool isBottomEdge = (row == height - 1);

                if (!isLeftEdge && !isRightEdge && !isBottomEdge && !isTopEdge)
                {
                    continue;
                }

                // Check for corner tiles (two perpendicular edges at once)
                bool isCorner = (isLeftEdge || isRightEdge) && (isBottomEdge || isTopEdge);

                Quaternion rotation = Quaternion.identity;
                if (isCorner)
                {
                    wallPrefab = CornerWallPrefab;
                    if (isLeftEdge && isTopEdge) rotation = Quaternion.Euler(0, 0, 0);
                    else if (isRightEdge && isTopEdge) rotation = Quaternion.Euler(0, 90, 0);
                    else if (isRightEdge && isBottomEdge) rotation = Quaternion.Euler(0, 180, 0);
                    else if (isLeftEdge && isBottomEdge) rotation = Quaternion.Euler(0, 270, 0);
                }
                else
                {
                    wallPrefab = StraightWallPrefab;
                    if (isTopEdge) rotation = Quaternion.Euler(0, 0, 0);
                    else if (isRightEdge) rotation = Quaternion.Euler(0, 90, 0);
                    else if (isBottomEdge) rotation = Quaternion.Euler(0, 180, 0);
                    else if (isLeftEdge) rotation = Quaternion.Euler(0, 270, 0);
                }

                Vector3 wallPosition = ConvertGridToWorldPosition(col, row);
                GameObject wallInstance = Instantiate(wallPrefab, wallPosition, rotation);
                wallInstance.name = $"Wall_{col}_{row}";
                if (CafeRoot != null)
                {
                    wallInstance.transform.SetParent(CafeRoot.transform);
                }
            }
        }
    }

    public void PopulateCafeLevel()
    {
#if UNITY_EDITOR
        List<CafeElement> cafeLayout = TestLayout;
#else
        List<CafeElement> cafeLayout = SaveManager.Instance.GetCafeLayout();
#endif

        PopulateFloorTiles();
        PopulateWalls();

        // Iterate through the saved layout and instantiate the furniture objects
        foreach (var element in cafeLayout)
        {
            if (element.furnitureObject == null)
            {
                continue;
            }
            GameObject furniturePrefab = element.furnitureObject.prefab;
            if (furniturePrefab == null)
            {
                Debug.LogWarning($"Furniture prefab for {element.furnitureObject.furnitureName} not found.");
                continue;
            }

            Vector3 spawnPosition = ConvertGridToWorldPosition(element.rootGridCoord.col, element.rootGridCoord.row);
            GameObject furnitureInstance = Instantiate(furniturePrefab, spawnPosition, Quaternion.Euler(0, element.rotation, 0));
            furnitureInstance.name = element.furnitureObject.furnitureName;
            if (CafeRoot != null)
            {
                furnitureInstance.transform.SetParent(CafeRoot.transform);
            }
        }
    }

    private void DestroyCafeFurniture()
    {
        if (CafeRoot == null)
            return;

        for (int i = CafeRoot.transform.childCount - 1; i >= 0; i--)
        {
            #if UNITY_EDITOR
            DestroyImmediate(CafeRoot.transform.GetChild(i).gameObject);
            #else
            Destroy(CafeRoot.transform.GetChild(i).gameObject);
            #endif
        }
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        // If TestInEditor was just set to false, destroy all children
        if (_prevTestInEditor && !TestInEditor && CafeRoot != null)
        {
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying && this != null && CafeRoot != null)
                {
                    DestroyCafeFurniture();
                }
            };
        } else if (!_prevTestInEditor && TestInEditor && CafeRoot != null)
        {
            // If TestInEditor was just set to true, populate the cafe level
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying && this != null && CafeRoot != null)
                {
                    DestroyCafeFurniture(); // CafeRoot shouldn't have children, but run just in case
                    PopulateCafeLevel();
                }
            };
        }

        if (TestInEditor && TestLayout != null)
        {
            // Defer execution to avoid Unity's internal restrictions
            EditorApplication.delayCall += () =>
            {
                // Only run if not in play mode and this object still exists
                if (!Application.isPlaying && this != null && CafeRoot != null)
                {
                    DestroyCafeFurniture();
                    PopulateCafeLevel();
                }
            };
        }

        _prevTestInEditor = TestInEditor;
#endif
    }
}