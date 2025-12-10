using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using FMODUnity;


public class SaveData
{
    public int saveDataVersion = 0;
    public int currentDay = 0;
    public string playerName = "";
    public string cafeName = "";
    public int highScore = 0;
    public int dayCount = 0;
    public int playerMoney = 0;
    public LevelLayout cafeLayout = null;
}


public struct CafeElement
{
    public FurnitureObject furnitureObject;
    public GridCoord rootGridCoord;
    public int rotation; // 0, 90, 180, 270 degrees
}

    [System.Serializable]
    public struct SerializableCafeElement
    {
        public string furnitureObjectName;
        public GridCoord rootGridCoord;
        public int rotation;
    }

    [System.Serializable]
    public class SerializableLevelLayout
    {
        public List<SerializableCafeElement> elements = new();
        public int rows;
        public int cols;
        public int deliveryTileCol;
        public int deliveryTileRow;
        public string floorPrefabName;
        public string straightWallPrefabName;
        public string cornerWallPrefabName;
        public string deliveryTilePrefabName;
        public string windowEndcapPrefabName;
    }

[System.Serializable]
public class SerializableSaveData
{

    public int saveDataVersion;
    public int currentDay;
    public string playerName;
    public string cafeName;
    public int highScore;
    public int dayCount;
    public int playerMoney;
    public SerializableLevelLayout cafeLayout;
}


public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField]
    private SaveData saveData = new SaveData();
    private string path = "Assets/SaveData/saveData.json";
    // bump this when introducing breaking changes to the save data format
    private const int CURRENT_SAVEDATA_VERSION = 1;

    void Awake()
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
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        // Create initial save data if the json doesn't exist yet
        if (!File.Exists(path))
        {
            saveData = new SaveData
            {
                saveDataVersion = CURRENT_SAVEDATA_VERSION,
                currentDay = 0,
                playerName = "Player",
                cafeName = "Robot Cafe",
                highScore = 0,
                dayCount = 0,
                playerMoney = 0,
                cafeLayout = CafeLayoutPresets.GetPreset("Starter")
            };
            Save();
        }
        else
        {
            Load();
        }
    }

    [ContextMenu("Save")]
    public void Save()
    {
        // Convert to serializable form
        SerializableSaveData serializable = new SerializableSaveData
        {
            saveDataVersion = saveData.saveDataVersion,
            currentDay = saveData.currentDay,
            playerName = saveData.playerName,
            cafeName = saveData.cafeName,
            highScore = saveData.highScore,
            dayCount = saveData.dayCount,
            playerMoney = saveData.playerMoney,
            cafeLayout = LevelLayoutToSerializable(saveData.cafeLayout)
        };
        // Create the file path and appropriate stream and writer objects
        FileStream stream = new(path, FileMode.Create); // The Create mode allows the program to overwrite the existing file with the same name if it exists
        StreamWriter writer = new(stream);

        // Write the data and close the objects
        writer.WriteLine(JsonUtility.ToJson(serializable));
        writer.Close();
        stream.Close();
    }

    public void Load()
    {
        string json = File.ReadAllText(path);
        var serialized = JsonUtility.FromJson<SerializableSaveData>(json);

        if (serialized == null || serialized.saveDataVersion != CURRENT_SAVEDATA_VERSION)
        {
            Debug.LogWarning($"Save file version outdated or invalid. Recreating fresh save.");
            // Recreate fresh save data
            saveData = new SaveData
            {
                saveDataVersion = CURRENT_SAVEDATA_VERSION,
                currentDay = 0,
                playerName = "Player",
                cafeName = "Robot Cafe",
                highScore = 0,
                dayCount = 0,
                playerMoney = 0,
                cafeLayout = CafeLayoutPresets.GetPreset("Starter")
            };
            Save();
            return;
        }

        // Version matches, load normally
        saveData = new SaveData
        {
            saveDataVersion = serialized.saveDataVersion,
            currentDay = serialized.currentDay,
            playerName = serialized.playerName,
            cafeName = serialized.cafeName,
            highScore = serialized.highScore,
            dayCount = serialized.dayCount,
            playerMoney = serialized.playerMoney,
            cafeLayout = SerializableToLevelLayout(serialized.cafeLayout)
        };
    }

#region Serialization
    public SerializableLevelLayout LevelLayoutToSerializable(LevelLayout layout)
    {
        var serializable = new SerializableLevelLayout
        {
            elements = new List<SerializableCafeElement>(),
            rows = layout.dimensions.rows,
            cols = layout.dimensions.cols,
            deliveryTileCol = layout.deliveryTileCoords.col,
            deliveryTileRow = layout.deliveryTileCoords.row,
            floorPrefabName = layout.floorPrefab != null ? "Meshes/" + layout.floorPrefab.name : "",
            straightWallPrefabName = layout.straightWallPrefab != null ? "Prefabs/" + layout.straightWallPrefab.name : "",
            cornerWallPrefabName = layout.cornerWallPrefab != null ? "Prefabs/" + layout.cornerWallPrefab.name : "",
            deliveryTilePrefabName = layout.deliveryTileFO != null ? "Prefabs/FurnitureObjects/" + layout.deliveryTileFO.name : "",
            windowEndcapPrefabName = layout.windowEndcapPrefab != null ? "Prefabs/" + layout.windowEndcapPrefab.name : ""
        };

        foreach (var element in layout.elements)
        {
            serializable.elements.Add(new SerializableCafeElement
            {
                furnitureObjectName = element.furnitureObject.prefab != null ? "Prefabs/FurnitureObjects/" + element.furnitureObject.name : "",
                rootGridCoord = element.rootGridCoord,
                rotation = element.rotation
            });
        }

        return serializable;
    }

    public LevelLayout SerializableToLevelLayout(SerializableLevelLayout serializable)
    {
        var layout = new LevelLayout
        {
            dimensions = (serializable.rows, serializable.cols),
            deliveryTileCoords = (serializable.deliveryTileCol, serializable.deliveryTileRow),
            floorPrefab = !string.IsNullOrEmpty(serializable.floorPrefabName) ? Resources.Load<GameObject>(serializable.floorPrefabName) : null,
            straightWallPrefab = !string.IsNullOrEmpty(serializable.straightWallPrefabName) ? Resources.Load<GameObject>(serializable.straightWallPrefabName) : null,
            cornerWallPrefab = !string.IsNullOrEmpty(serializable.cornerWallPrefabName) ? Resources.Load<GameObject>(serializable.cornerWallPrefabName) : null,
            deliveryTileFO = !string.IsNullOrEmpty(serializable.deliveryTilePrefabName) ? Resources.Load<FurnitureObject>(serializable.deliveryTilePrefabName) : null,
            windowEndcapPrefab = !string.IsNullOrEmpty(serializable.windowEndcapPrefabName) ? Resources.Load<GameObject>(serializable.windowEndcapPrefabName) : null,
            elements = new List<CafeElement>()
        };

        foreach (var serializableElement in serializable.elements)
        {
            var furnitureObj = Resources.Load<FurnitureObject>(serializableElement.furnitureObjectName);
            layout.elements.Add(new CafeElement
            {
                furnitureObject = furnitureObj,
                rootGridCoord = serializableElement.rootGridCoord,
                rotation = serializableElement.rotation
            });
        }

        return layout;
    }
#endregion

#region Getters and Setters
    public void SetDay(int day)
    {
        saveData.currentDay = day;
    }

    public int GetCurrentDay()
    {
        return saveData.currentDay;
    }

    public void SetPlayerName(string name)
    {
        saveData.playerName = name;
    }

    public string GetPlayerName()
    {
        return saveData.playerName;
    }

    public void SetCafeName(string name)
    {
        saveData.cafeName = name;
    }

    public string GetCafeName()
    {
        return saveData.cafeName;
    }

    public void SetHighScore(int score)
    {
        saveData.highScore = score;
    }

    public int GetHighScore()
    {
        return saveData.highScore;
    }

    public void SetDayCount(int dayCount)
    {
        saveData.dayCount = dayCount;
    }

    public int GetDayCount()
    {
        return saveData.dayCount;
    }

    public void AdjustPlayerMoney(int money)
    {
        saveData.playerMoney += money;
    }

    public int GetPlayerMoney()
    {
        return saveData.playerMoney;
    }

    public LevelLayout GetCafeLayout()
    {
        return saveData.cafeLayout;
    }

    public void SaveCafeLayout(LevelLayout cafeLayout)
    {
        saveData.cafeLayout = cafeLayout;
    }
#endregion
}

