
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
                deliveryTileCoords = (3, 5),
                floorPrefab = Resources.Load<GameObject>("Meshes/SM_floor"),
                straightWallPrefab = Resources.Load<GameObject>("Prefabs/WallPlain"),
                cornerWallPrefab = Resources.Load<GameObject>("Prefabs/WallCorner"),
                deliveryTileData = Resources.Load<FurnitureData>("Prefabs/FurnitureData/FD_WindowDelivery"),
                windowEndcapPrefab = Resources.Load<GameObject>("Prefabs/WindowEndcap"),
                elements = new List<CafeElement>
                {
                    new() {
                        furnitureData = Resources.Load<FurnitureData>("Prefabs/FurnitureData/FD_ChargingDock"),
                        rootGridCoord = new GridCoord { col = 3, row = 2 },
                        rotation = 0
                    },
                    new() {
                        furnitureData = Resources.Load<FurnitureData>("Prefabs/FurnitureData/FD_CoffeeMachine"),
                        rootGridCoord = new GridCoord { col = 0, row = 4 },
                        rotation = 90
                    },
                    new() {
                        furnitureData = Resources.Load<FurnitureData>("Prefabs/FurnitureData/FD_CupDispenser"),
                        rootGridCoord = new GridCoord { col = 0, row = 5 },
                        rotation = -270
                    },
                    new() {
                        furnitureData = Resources.Load<FurnitureData>("Prefabs/FurnitureData/FD_TrashBin"),
                        rootGridCoord = new GridCoord { col = 4, row = 5 },
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
