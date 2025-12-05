
using System.Collections.Generic;
using UnityEngine;

public static class CafeLayoutPresets
{
    public static Dictionary<string, LevelLayout> Presets =
        new(){
        {
            "Starter", new LevelLayout
            {
                dimensions = (6, 6),
                deliveryTileCoords = (4, 5),
                floorPrefab = Resources.Load<GameObject>("Meshes/SM_floor"),
                straightWallPrefab = Resources.Load<GameObject>("Prefabs/WallPlain"),
                cornerWallPrefab = Resources.Load<GameObject>("Prefabs/WallCorner"),
                deliveryTilePrefab = Resources.Load<GameObject>("Prefabs/DeliveryTile"),
                elements = new List<CafeElement>
                {
                    new() {
                        furnitureObject = Resources.Load<FurnitureObject>("Prefabs/FurnitureObjects/FO_ChargingDock"),
                        rootGridCoord = new GridCoord { col = 3, row = 2 },
                        rotation = 0
                    },
                    new() {
                        furnitureObject = Resources.Load<FurnitureObject>("Prefabs/FurnitureObjects/FO_CoffeeMachine"),
                        rootGridCoord = new GridCoord { col = 2, row = 5 },
                        rotation = 180
                    },
                    new() {
                        furnitureObject = Resources.Load<FurnitureObject>("Prefabs/FurnitureObjects/FO_CupDispenser"),
                        rootGridCoord = new GridCoord { col = 0, row = 3 },
                        rotation = -90
                    },
                    new() {
                        furnitureObject = Resources.Load<FurnitureObject>("Prefabs/FurnitureObjects/FO_TrashBin"),
                        rootGridCoord = new GridCoord { col = 0, row = 5 },
                        rotation = 0
                    },
                }
            }
        }
    };

    public static LevelLayout GetPreset(string name)
    {
        if (Presets.ContainsKey(name))
        {
            return Presets[name];
        }
        else
        {
            UnityEngine.Debug.LogWarning("Preset " + name + " not found.");
            return null;
        }
    }
}
