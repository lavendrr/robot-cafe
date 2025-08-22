using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable] // TODO: Make these properties serializable (no GameObjects, no tuples)
public class LevelLayout
{
    public List<CafeElement> elements = new List<CafeElement>();
    public (int rows, int cols) dimensions = (6, 10);
    public (int col, int row) deliveryTileCoords;
    public GameObject floorPrefab;
    public GameObject straightWallPrefab;
    public GameObject cornerWallPrefab;
    public GameObject deliveryTilePrefab;
}

[ExecuteAlways]
public class CafeLayoutManager : MonoBehaviour
{
    public static CafeLayoutManager Instance { get; private set; }

    private bool _prevTestInEditor = false;
    private LevelLayout TestLayout = new LevelLayout();

    [SerializeField]
    public GameObject CafeRoot;
    public float GridCellSize = 1.0f;
    public bool TestInEditor = false;
    public Vector2 dimensions = new Vector2(6, 10);
    public Vector2 deliveryTileCoords;
    public GameObject floorPrefab;
    public GameObject straightWallPrefab;
    public GameObject cornerWallPrefab;
    public GameObject deliveryTilePrefab;
    public List<CafeElement> elements = new List<CafeElement>();

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

        if (Application.isPlaying)
        {
            if (SaveManager.Instance.GetCafeLayout() == null)
            {
                Debug.LogError("No cafe layout found in SaveManager.");
            }

            DestroyCafeFurniture();
            PopulateCafeLevel(SaveManager.Instance.GetCafeLayout());
        }
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

    public void PopulateFloorTiles(LevelLayout layout)
    {
        // Instantiate floors to cover the entire area
        if (layout.floorPrefab == null)
        {
            Debug.LogError("FloorPrefab not assigned in CafeLayoutManager.");
            return;
        }

        for (int row = 0; row < layout.dimensions.rows; row++)
        {
            for (int col = 0; col < layout.dimensions.cols; col++)
            {
                Vector3 floorPosition = ConvertGridToWorldPosition(col, row);
                GameObject floorInstance = Instantiate(layout.floorPrefab, floorPosition, Quaternion.identity);
                floorInstance.name = $"Floor_{row}_{col}";
                if (CafeRoot != null)
                {
                    floorInstance.transform.SetParent(CafeRoot.transform);
                }
            }
        }
    }

    public void PopulateWalls(LevelLayout layout)
    {
        if (layout.straightWallPrefab == null || layout.cornerWallPrefab == null || layout.deliveryTilePrefab == null)
        {
            Debug.LogError("Wall prefabs not assigned in CafeLayoutManager.");
            return;
        }

        int width = layout.dimensions.cols;
        int height = layout.dimensions.rows;

        // Calculate delivery-adjacent tile coordinates (to the left of delivery tile, relative to orientation)
        (int col, int row) deliveryTile = layout.deliveryTileCoords;
        (int col, int row)? deliveryAdjacentTile = null;

        // Determine which edge the delivery tile is on and calculate the adjacent tile
        if (deliveryTile.row == 0) // Top edge
            deliveryAdjacentTile = (deliveryTile.col + 1, deliveryTile.row);
        else if (deliveryTile.col == width - 1) // Right edge
            deliveryAdjacentTile = (deliveryTile.col, deliveryTile.row + 1);
        else if (deliveryTile.row == height - 1) // Bottom edge
            deliveryAdjacentTile = (deliveryTile.col - 1, deliveryTile.row);
        else if (deliveryTile.col == 0) // Left edge
            deliveryAdjacentTile = (deliveryTile.col, deliveryTile.row - 1);

        // Check if the adjacent tile is a corner
        if (deliveryAdjacentTile.HasValue)
        {
            if ((deliveryAdjacentTile.Value.col == 0 || deliveryAdjacentTile.Value.col == width - 1) &&
                (deliveryAdjacentTile.Value.row == 0 || deliveryAdjacentTile.Value.row == height - 1))
            {
                Debug.LogError("Invalid Delivery Tile. Adjacent tile is a corner.");
            }
        }

        GameObject wallPrefab;
        for (int col = 0; col < width; col++)
        {
            for (int row = 0; row < height; row++)
            {
                // Skip the delivery-adjacent tile
                if (deliveryAdjacentTile.HasValue && col == deliveryAdjacentTile.Value.col && row == deliveryAdjacentTile.Value.row)
                    continue;

                // Check if the tile is an edge tile
                bool isLeftEdge = (col == 0);
                bool isRightEdge = (col == width - 1);
                bool isTopEdge = (row == 0);
                bool isBottomEdge = (row == height - 1);

                // Check if this is the delivery tile
                bool isDeliveryTile = (col == layout.deliveryTileCoords.col && row == layout.deliveryTileCoords.row);

                if (!isLeftEdge && !isRightEdge && !isBottomEdge && !isTopEdge)
                {
                    if (isDeliveryTile)
                    {
                        Debug.LogError("Invalid Delivery Tile. Tile is not an edge cell.");
                        continue;
                    }
                    continue;
                }

                // Check for corner tiles (two perpendicular edges at once)
                bool isCorner = (isLeftEdge || isRightEdge) && (isBottomEdge || isTopEdge);

                Quaternion rotation = Quaternion.identity;
                if (isCorner)
                {
                    if (isDeliveryTile)
                    {
                        Debug.LogError("Invalid Delivery Tile. Tile is a corner cell.");
                    }
                    wallPrefab = layout.cornerWallPrefab;
                    if (isLeftEdge && isTopEdge) rotation = Quaternion.Euler(0, 0, 0);
                    else if (isRightEdge && isTopEdge) rotation = Quaternion.Euler(0, 90, 0);
                    else if (isRightEdge && isBottomEdge) rotation = Quaternion.Euler(0, 180, 0);
                    else if (isLeftEdge && isBottomEdge) rotation = Quaternion.Euler(0, 270, 0);
                }
                else
                {
                    if (isDeliveryTile)
                    {
                        wallPrefab = layout.deliveryTilePrefab;
                    } else {
                        wallPrefab = layout.straightWallPrefab;
                    }
                    if (isTopEdge) rotation = Quaternion.Euler(0, 0, 0);
                    else if (isRightEdge) rotation = Quaternion.Euler(0, 90, 0);
                    else if (isBottomEdge) rotation = Quaternion.Euler(0, 180, 0);
                    else if (isLeftEdge) rotation = Quaternion.Euler(0, 270, 0);
                }

                Vector3 wallPosition = ConvertGridToWorldPosition(col, row);
                GameObject wallInstance = Instantiate(wallPrefab, wallPosition, rotation);
                if (isDeliveryTile)
                {
                    wallInstance.name = $"DeliveryTile{col}_{row}";
                } else {
                    wallInstance.name = $"Wall{col}_{row}";
                }
                if (CafeRoot != null)
                {
                    wallInstance.transform.SetParent(CafeRoot.transform);
                }
            }
        }
    }

    public void PopulateCafeLevel(LevelLayout cafeLayout)
    {
        PopulateFloorTiles(cafeLayout);
        PopulateWalls(cafeLayout);

        // Iterate through the saved layout and instantiate the furniture objects
        foreach (var element in cafeLayout.elements)
        {
            if (element.furnitureObject == null)
            {
                continue;
            }
            GameObject furniturePrefab = element.furnitureObject.prefab;
            if (furniturePrefab == null)
            {
                Debug.LogError($"Furniture prefab for {element.furnitureObject.furnitureName} not found.");
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

    private void UpdateTestLayout()
    {
        TestLayout = new LevelLayout
        {
            dimensions = ((int)dimensions.x, (int)dimensions.y),
            deliveryTileCoords = ((int)deliveryTileCoords.x, (int)deliveryTileCoords.y),
            floorPrefab = floorPrefab,
            straightWallPrefab = straightWallPrefab,
            cornerWallPrefab = cornerWallPrefab,
            deliveryTilePrefab = deliveryTilePrefab,
            elements = elements
        };
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
        }
        else if (!_prevTestInEditor && TestInEditor && CafeRoot != null)
        {
            // If TestInEditor was just set to true, populate the cafe level
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying && this != null && CafeRoot != null)
                {
                    DestroyCafeFurniture(); // CafeRoot shouldn't have children, but run just in case
                    UpdateTestLayout();
                    PopulateCafeLevel(TestLayout);
                }
            };
        }

        if (TestInEditor)
        {
            // Defer execution to avoid Unity's internal restrictions
            EditorApplication.delayCall += () =>
            {
                // Only run if not in play mode and this object still exists
                if (!Application.isPlaying && this != null && CafeRoot != null)
                {
                    DestroyCafeFurniture();
                    UpdateTestLayout();
                    PopulateCafeLevel(TestLayout);
                }
            };
        }

        _prevTestInEditor = TestInEditor;
#endif
    }
}