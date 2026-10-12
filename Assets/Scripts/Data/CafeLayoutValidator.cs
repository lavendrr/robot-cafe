using System.Collections.Generic;

public enum LayoutIssueSeverity
{
    Warning,
    Error
}

// Identifies which preset field an issue belongs to, so the inspector can highlight it.
public enum LayoutField
{
    None,
    Dimensions,
    DeliveryTileCoords,
    FloorPrefab,
    CeilingPrefab,
    StraightWallPrefab,
    CornerWallPrefab,
    DeliveryTileData,
    WindowEndcapPrefab,
    Elements
}

public struct LayoutIssue
{
    public LayoutIssueSeverity severity;
    public string message;
    public LayoutField field;
    public int elementIndex; // -1 unless the issue targets a specific element

    public LayoutIssue(LayoutIssueSeverity severity, LayoutField field, string message, int elementIndex = -1)
    {
        this.severity = severity;
        this.field = field;
        this.message = message;
        this.elementIndex = elementIndex;
    }
}

// Pure validation of a LevelLayout. No Unity editor dependencies, so this is the single
// source of truth for both the in-editor preview and any runtime checks.
public static class CafeLayoutValidator
{
    public static List<LayoutIssue> Validate(LevelLayout layout)
    {
        var issues = new List<LayoutIssue>();
        if (layout == null)
        {
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.None, "Layout is null."));
            return issues;
        }

        int width = layout.dimensions.cols;
        int height = layout.dimensions.rows;

        ValidateDimensions(issues, width, height);
        ValidatePrefabs(issues, layout);

        // Delivery-tile checks only make sense once dimensions are valid.
        if (width > 0 && height > 0)
        {
            ValidateDeliveryTile(issues, layout, width, height);
            ValidateElements(issues, layout, width, height);
        }

        return issues;
    }

    public static bool HasErrors(List<LayoutIssue> issues)
    {
        foreach (LayoutIssue issue in issues)
        {
            if (issue.severity == LayoutIssueSeverity.Error)
                return true;
        }
        return false;
    }

    private static void ValidateDimensions(List<LayoutIssue> issues, int width, int height)
    {
        if (height <= 0 || width <= 0)
        {
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.Dimensions,
                $"Dimensions must be positive (got {height} rows × {width} cols)."));
        }
    }

    private static void ValidatePrefabs(List<LayoutIssue> issues, LevelLayout layout)
    {
        if (layout.floorPrefab == null)
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.FloorPrefab, "Floor prefab is not assigned."));
        if (layout.ceilingPrefab == null)
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.CeilingPrefab, "Ceiling prefab is not assigned."));
        if (layout.straightWallPrefab == null)
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.StraightWallPrefab, "Straight wall prefab is not assigned."));
        if (layout.cornerWallPrefab == null)
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.CornerWallPrefab, "Corner wall prefab is not assigned."));
        if (layout.windowEndcapPrefab == null)
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.WindowEndcapPrefab, "Window endcap prefab is not assigned."));

        if (layout.deliveryTileData == null)
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.DeliveryTileData, "Delivery tile data is not assigned."));
        else if (layout.deliveryTileData.prefab == null)
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.DeliveryTileData, "Delivery tile data has no prefab."));
    }

    private static void ValidateDeliveryTile(List<LayoutIssue> issues, LevelLayout layout, int width, int height)
    {
        (int col, int row) root = layout.deliveryTileCoords;

        bool inBounds = root.col >= 0 && root.col < width && root.row >= 0 && root.row < height;
        if (!inBounds)
        {
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.DeliveryTileCoords,
                $"Delivery tile ({root.col}, {root.row}) is outside the {height}×{width} grid."));
            return;
        }

        bool onEdge = root.col == 0 || root.col == width - 1 || root.row == 0 || root.row == height - 1;
        if (!onEdge)
        {
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.DeliveryTileCoords,
                $"Delivery tile ({root.col}, {root.row}) must be on an edge of the cafe."));
            return;
        }

        if (IsCorner(root.col, root.row, width, height))
        {
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.DeliveryTileCoords,
                $"Delivery tile ({root.col}, {root.row}) cannot be a corner cell."));
            return;
        }

        // The window footprint depends on the delivery width, which comes from deliveryTileData.
        if (layout.deliveryTileData == null || layout.deliveryTileData.gridOffsets == null)
            return;

        int deliveryTileWidth = layout.deliveryTileData.gridOffsets.Count + 1;

        // The delivery window is the tile plus an endcap on each side; along its edge the full
        // footprint runs from (root - deliveryTileWidth) to (root + 1), mirroring
        // CafeLayoutManager.PopulateWalls. That whole footprint must sit strictly between the
        // two corners, i.e. within indices [1 .. edgeLength - 2].
        bool horizontalEdge = root.row == 0 || root.row == height - 1;
        int edgeLength = horizontalEdge ? width : height;
        int rootIndex = horizontalEdge ? root.col : root.row;
        string axis = horizontalEdge ? "column" : "row";

        int footprintLow = rootIndex - deliveryTileWidth; // far endcap
        int footprintHigh = rootIndex + 1;                // near endcap

        if (footprintLow < 0 || footprintHigh > edgeLength - 1)
        {
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.DeliveryTileCoords,
                $"Delivery window (tile width {deliveryTileWidth} + 2 endcaps) does not fit at {axis} {rootIndex}; it would span {axis}s {footprintLow}–{footprintHigh} on a {edgeLength}-cell edge."));
        }
        else if (footprintLow == 0 || footprintHigh == edgeLength - 1)
        {
            issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.DeliveryTileCoords,
                $"Delivery window would touch a corner at {axis} {rootIndex}; its tile and endcaps must stay between the corners ({axis}s 1–{edgeLength - 2})."));
        }
    }

    private static void ValidateElements(List<LayoutIssue> issues, LevelLayout layout, int width, int height)
    {
        if (layout.elements == null)
            return;

        for (int i = 0; i < layout.elements.Count; i++)
        {
            CafeElement element = layout.elements[i];

            if (element.furnitureData == null)
            {
                issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.Elements,
                    $"Element [{i}] has no furniture data assigned.", i));
                continue;
            }

            string name = string.IsNullOrEmpty(element.furnitureData.furnitureName)
                ? element.furnitureData.name
                : element.furnitureData.furnitureName;

            if (element.furnitureData.prefab == null)
            {
                issues.Add(new LayoutIssue(LayoutIssueSeverity.Error, LayoutField.Elements,
                    $"Element [{i}] ('{name}') has no prefab.", i));
            }

            GridCoord coord = element.rootGridCoord;
            if (coord.col < 0 || coord.col >= width || coord.row < 0 || coord.row >= height)
            {
                issues.Add(new LayoutIssue(LayoutIssueSeverity.Warning, LayoutField.Elements,
                    $"Element [{i}] ('{name}') at ({coord.col}, {coord.row}) is outside the {height}×{width} grid.", i));
            }
        }
    }

    private static bool IsCorner(int col, int row, int width, int height)
    {
        return (col == 0 || col == width - 1) && (row == 0 || row == height - 1);
    }
}
