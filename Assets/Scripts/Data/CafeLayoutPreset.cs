using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CLP_New", menuName = "Robot Cafe/Cafe Layout Preset")]
public class CafeLayoutPreset : ScriptableObject
{
    [Min(1)] public int rows = 6;
    [Min(1)] public int cols = 6;
    public GridCoord deliveryTileCoords;
    public GameObject floorPrefab;
    public GameObject ceilingPrefab;
    public GameObject straightWallPrefab;
    public GameObject cornerWallPrefab;
    public FurnitureData deliveryTileData;
    public GameObject windowEndcapPrefab;
    public List<CafeElement> elements = new();

#if UNITY_EDITOR
    // Fired whenever the asset is edited, so an in-editor preview can rebuild live
    public event System.Action Changed;

    private void OnValidate()
    {
        Changed?.Invoke();
    }
#endif

    public LevelLayout ToLevelLayout()
    {
        return new LevelLayout
        {
            dimensions = (rows, cols),
            deliveryTileCoords = (deliveryTileCoords.col, deliveryTileCoords.row),
            floorPrefab = floorPrefab,
            ceilingPrefab = ceilingPrefab,
            straightWallPrefab = straightWallPrefab,
            cornerWallPrefab = cornerWallPrefab,
            deliveryTileData = deliveryTileData,
            windowEndcapPrefab = windowEndcapPrefab,
            elements = elements != null ? new List<CafeElement>(elements) : new List<CafeElement>()
        };
    }
}
