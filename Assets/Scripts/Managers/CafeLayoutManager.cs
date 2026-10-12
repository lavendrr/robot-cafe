using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class LevelLayout
{
    public List<CafeElement> elements = new List<CafeElement>();
    public (int rows, int cols) dimensions = (6, 10);
    public (int col, int row) deliveryTileCoords;
    public GameObject floorPrefab;
    public GameObject ceilingPrefab;
    public GameObject straightWallPrefab;
    public GameObject cornerWallPrefab;
    public FurnitureData deliveryTileData;
    public GameObject windowEndcapPrefab;
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
    public CafeLayoutPreset activePreset;
    public GameObject deliveryWindowObject;

    void Start()
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
                Debug.LogError("No cafe layout found in SaveManager, spawning Starter preset.");
                SaveManager.Instance.SaveCafeLayout(CafeLayoutPresets.GetPreset("Starter"));
            }

            DestroyCafeFurniture();
            PopulateCafeLevel(SaveManager.Instance.GetCafeLayout());

            if (StateManager.Instance.GetCurrentState().GetType() == typeof(ShiftState) && OrderManager.Instance)
            {
                OrderManager.Instance.NewCustomer();
            }
        }
    }
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
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

    public void PopulateCeilingTiles(LevelLayout layout)
    {
        // Instantiate ceilings to cover the entire area
        if (layout.ceilingPrefab == null)
        {
            Debug.LogError("CeilingPrefab not assigned in CafeLayoutManager.");
            return;
        }

        for (int row = 0; row < layout.dimensions.rows; row++)
        {
            for (int col = 0; col < layout.dimensions.cols; col++)
            {
                Vector3 ceilingPosition = ConvertGridToWorldPosition(col, row);
                GameObject ceilingInstance = Instantiate(layout.ceilingPrefab, ceilingPosition, Quaternion.identity);
                ceilingInstance.name = $"Ceiling_{row}_{col}";
                if (CafeRoot != null)
                {
                    ceilingInstance.transform.SetParent(CafeRoot.transform);
                }
            }
        }
    }

    public void PopulateWalls(LevelLayout layout)
    {
        if (layout.straightWallPrefab == null || layout.cornerWallPrefab == null || layout.deliveryTileData.prefab == null
            || layout.windowEndcapPrefab == null)
        {
            Debug.LogError("Wall prefabs not assigned in CafeLayoutManager.");
            return;
        }

        int width = layout.dimensions.cols;
        int height = layout.dimensions.rows;

        (int col, int row) deliveryTileRoot = layout.deliveryTileCoords;
        int deliveryTileWidth = layout.deliveryTileData.gridOffsets.Count + 1;

        // Compute the two adjacent edge tiles beside the delivery tile (left and right along the edge)
        (int col, int row)? leftAdjacent = null;
        (int col, int row)? rightAdjacent = null;

        if (deliveryTileRoot.row == 0) // Top edge -> left/right along columns
        {
            leftAdjacent = (deliveryTileRoot.col - deliveryTileWidth, deliveryTileRoot.row);
            rightAdjacent = (deliveryTileRoot.col + 1, deliveryTileRoot.row);
        }
        else if (deliveryTileRoot.col == width - 1) // Right edge -> up/down along rows
        {
            leftAdjacent = (deliveryTileRoot.col, deliveryTileRoot.row - deliveryTileWidth); // up
            rightAdjacent = (deliveryTileRoot.col, deliveryTileRoot.row + 1); // down
        }
        else if (deliveryTileRoot.row == height - 1) // Bottom edge -> left/right along columns
        {
            leftAdjacent = (deliveryTileRoot.col - deliveryTileWidth, deliveryTileRoot.row);
            rightAdjacent = (deliveryTileRoot.col + 1, deliveryTileRoot.row);
        }
        else if (deliveryTileRoot.col == 0) // Left edge -> up/down along rows
        {
            leftAdjacent = (deliveryTileRoot.col, deliveryTileRoot.row - deliveryTileWidth); // up
            rightAdjacent = (deliveryTileRoot.col, deliveryTileRoot.row + 1); // down
        }

        // Filter out-of-bounds adjacent tiles
        bool leftValid = leftAdjacent.HasValue && leftAdjacent.Value.col >= 0 && leftAdjacent.Value.col < width && leftAdjacent.Value.row >= 0 && leftAdjacent.Value.row < height;
        bool rightValid = rightAdjacent.HasValue && rightAdjacent.Value.col >= 0 && rightAdjacent.Value.col < width && rightAdjacent.Value.row >= 0 && rightAdjacent.Value.row < height;

        // Prevents the delivery window from being adjacent to a corner (no current support for corner window endcaps)
        if (leftValid)
        {
            var t = leftAdjacent.Value;
            if ((t.col == 0 || t.col == width - 1) && (t.row == 0 || t.row == height - 1))
            {
                Debug.LogError("Invalid Delivery Tile. Left adjacent tile is a corner.");
                return;
            }
        }
        if (rightValid)
        {
            var t = rightAdjacent.Value;
            if ((t.col == 0 || t.col == width - 1) && (t.row == 0 || t.row == height - 1))
            {
                Debug.LogError("Invalid Delivery Tile. Right adjacent tile is a corner.");
                return;
            }
        }

        for (int col = 0; col < width; col++)
        {
            for (int row = 0; row < height; row++)
            {
                // Check if the tile is an edge tile
                bool isLeftEdge = (col == 0);
                bool isRightEdge = (col == width - 1);
                bool isTopEdge = (row == 0);
                bool isBottomEdge = (row == height - 1);

                // Skip interior tiles
                if (!isLeftEdge && !isRightEdge && !isBottomEdge && !isTopEdge)
                    continue;

                // Check for corner tiles (two perpendicular edges at once)
                bool isCorner = (isLeftEdge || isRightEdge) && (isBottomEdge || isTopEdge);

                // Check if this is the delivery tile
                bool isDeliveryTile = IsDeliveryTile(layout, col, row);

                // If this is the delivery tile, handle instantiation (or error if it's a corner)
                if (isDeliveryTile)
                {
                    if (isCorner)
                    {
                        Debug.LogError("Invalid Delivery Tile. Tile is a corner cell.");
                        continue;
                    }

                    if (col != layout.deliveryTileCoords.col || row != layout.deliveryTileCoords.row)
                    {
                        // This is a delivery offset tile, we only want to instantiate on the root tile
                        continue;
                    }

                    Quaternion deliveryRotation = Quaternion.identity;
                    if (isTopEdge) deliveryRotation = Quaternion.Euler(0, 0, 0);
                    else if (isRightEdge) deliveryRotation = Quaternion.Euler(0, 90, 0);
                    else if (isBottomEdge) deliveryRotation = Quaternion.Euler(0, 180, 0);
                    else if (isLeftEdge) deliveryRotation = Quaternion.Euler(0, 270, 0);

                    Vector3 deliveryPos = ConvertGridToWorldPosition(col, row);
                    GameObject deliveryInstance = Instantiate(layout.deliveryTileData.prefab, deliveryPos, deliveryRotation);
                    deliveryInstance.name = $"DeliveryTile{col}_{row}";
                    deliveryWindowObject = deliveryInstance;
                    if (CafeRoot != null)
                        deliveryInstance.transform.SetParent(CafeRoot.transform);

                    continue;
                }

                bool isLeftWindowAdj = leftValid && col == leftAdjacent.Value.col && row == leftAdjacent.Value.row;
                bool isRightWindowAdj = rightValid && col == rightAdjacent.Value.col && row == rightAdjacent.Value.row;
                bool isWindowAdj = isLeftWindowAdj || isRightWindowAdj;

                GameObject wallPrefab;
                Quaternion rotation = Quaternion.identity;

                if (isCorner)
                {
                    wallPrefab = layout.cornerWallPrefab;
                    if (isLeftEdge && isTopEdge) rotation = Quaternion.Euler(0, 0, 0);
                    else if (isRightEdge && isTopEdge) rotation = Quaternion.Euler(0, 90, 0);
                    else if (isRightEdge && isBottomEdge) rotation = Quaternion.Euler(0, 180, 0);
                    else if (isLeftEdge && isBottomEdge) rotation = Quaternion.Euler(0, 270, 0);
                }
                else
                {
                    if (isWindowAdj)
                    {
                        wallPrefab = layout.windowEndcapPrefab;
                    }
                    else
                    {
                        wallPrefab = layout.straightWallPrefab;
                    }

                    if (isTopEdge) rotation = Quaternion.Euler(0, 0, 0);
                    else if (isRightEdge) rotation = Quaternion.Euler(0, 90, 0);
                    else if (isBottomEdge) rotation = Quaternion.Euler(0, 180, 0);
                    else if (isLeftEdge) rotation = Quaternion.Euler(0, 270, 0);
                }

                Vector3 wallPosition = ConvertGridToWorldPosition(col, row);
                GameObject wallInstance = Instantiate(wallPrefab, wallPosition, rotation);

                // If this is the right adjacent window endcap, flip its local scale on the appropriate axis.
                if (isRightWindowAdj)
                {
                    Vector3 s = wallInstance.transform.localScale;
                    float yaw = rotation.eulerAngles.y % 360f;
                    // For horizontal-facing walls (0 or 180) mirror X; for vertical-facing (90 or 270) mirror Z
                    if (Mathf.Approximately(yaw, 0f) || Mathf.Approximately(yaw, 180f))
                    {
                        s.x *= -1f;
                    }
                    else
                    {
                        s.z *= -1f;
                    }
                    wallInstance.transform.localScale = s;
                }

                if (isWindowAdj)
                {
                    if (isLeftWindowAdj) wallInstance.name = $"WindowEndcap_Left{col}_{row}";
                    else wallInstance.name = $"WindowEndcap_Right{col}_{row}";
                }
                else
                {
                    wallInstance.name = isCorner ? $"Corner{col}_{row}" : $"Wall{col}_{row}";
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
        if (cafeLayout == null)
        {
            Debug.LogError("Cannot populate cafe level: layout is null.");
            return;
        }

        // Validate only at runtime. The editor preview (DebouncedRebuild) already validates
        // before calling this, so gating on isPlaying avoids re-running the same checks in-editor.
        if (Application.isPlaying)
        {
            List<LayoutIssue> issues = CafeLayoutValidator.Validate(cafeLayout);
            if (CafeLayoutValidator.HasErrors(issues))
            {
                string details = string.Join("\n  - ", issues
                    .Where(i => i.severity == LayoutIssueSeverity.Error)
                    .Select(i => i.message));
                Debug.LogError($"Cannot populate cafe level; layout has errors:\n  - {details}");
                return;
            }
        }

        PopulateFloorTiles(cafeLayout);
        PopulateCeilingTiles(cafeLayout);
        PopulateWalls(cafeLayout);

        // Iterate through the saved layout and instantiate the furniture objects
        foreach (var element in cafeLayout.elements)
        {
            if (element.furnitureData == null)
            {
                continue;
            }
            GameObject furniturePrefab = element.furnitureData.prefab;
            if (furniturePrefab == null)
            {
                Debug.LogError($"Furniture prefab for {element.furnitureData.furnitureName} not found.");
                continue;
            }

            Vector3 spawnPosition = ConvertGridToWorldPosition(element.rootGridCoord.col, element.rootGridCoord.row);
            GameObject furnitureInstance = Instantiate(furniturePrefab, spawnPosition, Quaternion.Euler(0, element.rotation, 0));
            furnitureInstance.name = element.furnitureData.furnitureName;
            if (CafeRoot != null)
            {
                furnitureInstance.transform.SetParent(CafeRoot.transform);
            }
        }
    }

    public void DestroyCafeFurniture()
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
        TestLayout = activePreset != null ? activePreset.ToLevelLayout() : new LevelLayout();
    }

    private bool IsDeliveryTile(LevelLayout layout, int col, int row)
    {
        if (layout == null || layout.deliveryTileData == null || layout.deliveryTileData.gridOffsets == null)
            return false;

        int width = layout.dimensions.cols;
        int height = layout.dimensions.rows;

        (int col, int row) root = layout.deliveryTileCoords;
        int deliveryTileWidth = layout.deliveryTileData.gridOffsets.Count + 1;
        if (deliveryTileWidth < 1) deliveryTileWidth = 1;

        // Exact root tile is always part of the delivery tile
        if (col == root.col && row == root.row)
            return true;

        // Horizontal edge (top or bottom): tile spans columns [root.col - (w-1) .. root.col] on the same row
        if (root.row == 0 || root.row == height - 1)
        {
            int startCol = root.col - (deliveryTileWidth - 1);
            int endCol = root.col;
            return row == root.row && col >= startCol && col <= endCol;
        }

        // Vertical edge (left or right): tile spans rows [root.row - (w-1) .. root.row] on the same column
        if (root.col == 0 || root.col == width - 1)
        {
            int startRow = root.row - (deliveryTileWidth - 1);
            int endRow = root.row;
            return col == root.col && row >= startRow && row <= endRow;
        }

        return false;
    }

#if UNITY_EDITOR
    private CafeLayoutPreset _subscribedPreset;

    // Debounce so a drag-scrub (which streams many intermediate values) rebuilds only once,
    // from the final settled value
    private const double RebuildDebounceSeconds = 0.1;
    private double _rebuildDueAt;
    private bool _rebuildScheduled;

    private void OnEnable()
    {
        SubscribeToPreset();
    }

    private void OnDisable()
    {
        UnsubscribeFromPreset();
        if (_rebuildScheduled)
        {
            EditorApplication.update -= DebouncedRebuild;
            _rebuildScheduled = false;
        }
    }

    // Keep the preview rebuilding whenever the active preset is edited (either in this
    // manager's inspector or directly in the Project window)
    private void SubscribeToPreset()
    {
        if (_subscribedPreset == activePreset)
            return;

        UnsubscribeFromPreset();
        if (activePreset != null)
        {
            activePreset.Changed += RebuildPreview;
            _subscribedPreset = activePreset;
        }
    }

    private void UnsubscribeFromPreset()
    {
        if (_subscribedPreset != null)
        {
            _subscribedPreset.Changed -= RebuildPreview;
            _subscribedPreset = null;
        }
    }

    // Requests a preview rebuild, debounced
    private void RebuildPreview()
    {
        _rebuildDueAt = EditorApplication.timeSinceStartup + RebuildDebounceSeconds;
        if (_rebuildScheduled)
            return;

        _rebuildScheduled = true;
        EditorApplication.update += DebouncedRebuild;
    }

    private void DebouncedRebuild()
    {
        // Wait until changes have stopped for the debounce window
        if (EditorApplication.timeSinceStartup < _rebuildDueAt)
            return;

        EditorApplication.update -= DebouncedRebuild;
        _rebuildScheduled = false;

        if (!Application.isPlaying && this != null && CafeRoot != null && TestInEditor)
        {
            UpdateTestLayout();
            // Keep the last good geometry when the layout has errors, but continue on warnings
            if (CafeLayoutValidator.HasErrors(CafeLayoutValidator.Validate(TestLayout)))
                return;

            DestroyCafeFurniture();
            PopulateCafeLevel(TestLayout);
            MarkPreviewGeometryTransient();
        }
    }

    // Flags every spawned preview instance so it is never serialized into the scene when the
    // user saves. The preview is parented under CafeRoot and regenerated on demand, so it should
    // never live in the committed scene asset. Done as a single sweep (rather than at each
    // Instantiate site) to keep the shared, runtime Populate* methods free of editor-only code.
    // The flag is per-object and doesn't propagate, so we apply it to every object in each
    // instance's hierarchy. These objects are still destroyed normally by DestroyCafeFurniture.
    private void MarkPreviewGeometryTransient()
    {
        if (CafeRoot == null)
            return;

        foreach (Transform child in CafeRoot.transform)
        {
            foreach (Transform t in child.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.hideFlags = HideFlags.DontSaveInEditor;
            }
        }
    }

    private void OnValidate()
    {
        // Re-point the Changed subscription if the active preset was swapped
        SubscribeToPreset();

        // TestInEditor was just turned off: clear the preview
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
        else if (TestInEditor && CafeRoot != null)
        {
            // Turned on, or any inspector change while on: rebuild
            RebuildPreview();
        }

        _prevTestInEditor = TestInEditor;
    }
#endif
}