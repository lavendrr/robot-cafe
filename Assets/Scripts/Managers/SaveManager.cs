using UnityEngine;
using UnityEditor;
using System.IO;

[System.Serializable]
public class SaveData
{
    public int currentDay = 0;
    public string playerName = "";
    public string cafeName = "";
    public int highScore = 0;
    public int dayCount = 0;
    public int playerMoney = 0;
    public GameObject[,] planningGrid;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField]
    private SaveData saveData = new SaveData();
    private string path = "Assets/SaveData/saveData.json";

    private void Awake()
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

    void Start()
    {
        // Create initial save data if the json doesn't exist yet
        if (!File.Exists(path))
        {
            Save();
            Debug.Log("Created initial save data");
        }
        // Read the json into the saveData object
        JsonUtility.FromJsonOverwrite(File.ReadAllText(path), saveData);
    }

    [ContextMenu("Save")]
    public void Save()
    {
        // Create the file path and appropriate stream and writer objects
        FileStream stream = new(path, FileMode.Create); // The Create mode allows the program to overwrite the existing file with the same name if it exists
        StreamWriter writer = new(stream);

        // Write the data and close the objects
        writer.WriteLine(JsonUtility.ToJson(saveData));
        writer.Close();
        stream.Close();

        // Reimport the new json file
        // AssetDatabase.ImportAsset(path);
        Debug.Log("Successfully saved!");
    }

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

    public GameObject[,] GetPlanningGrid()
    {
        return saveData.planningGrid;
    }

    public void SavePlanningGrid(GameObject[,] planningGrid)
    {
        saveData.planningGrid = planningGrid;
    }
}

