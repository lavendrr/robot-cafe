using UnityEngine;
using System.Collections.Generic;
using System.IO;

public enum FuelType
{
    None,
    Unleaded,
    Diesel,
    Premium
}

public class MenuItem
{
    public string name;
    public FuelType fuelType;
    public int cost;
    public List<FurnitureObject> requiredFurniture;

    public MenuItem(string _name, FuelType _fuelType, int _cost, List<FurnitureObject> _requiredFurniture = null)
    {
        name = _name;
        fuelType = _fuelType;
        cost = _cost;
        requiredFurniture = _requiredFurniture == null ? new List<FurnitureObject>() : _requiredFurniture;
    }
}

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    private List<MenuItem> menu = new List<MenuItem>();
    string[] FO_names;
    public Dictionary<string, FurnitureObject> FO_dictionary = new Dictionary<string, FurnitureObject>();
    string prefix = "Prefabs/FurnitureObjects/";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            string folderPath = Path.Combine(Application.dataPath, "Resources/Prefabs/FurnitureObjects");
            if (Directory.Exists(folderPath))
            {
                FO_names = Directory.GetFiles(folderPath, "*.asset", SearchOption.TopDirectoryOnly);
            }
            else
            {
                FO_names = new string[0];
            }

            foreach (string file in FO_names)
            {
                FO_dictionary.Add(Path.GetFileNameWithoutExtension(file), null);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public FurnitureObject AccessFurnitureObject(string name)
    {
        if (FO_dictionary[name] == null || !FO_dictionary.ContainsKey(name))
        {
            FO_dictionary[name] = Resources.Load<FurnitureObject>(prefix + name);
        }

        return FO_dictionary[name];
    }

    public MenuItem[] ListItems()
    {
        return menu.ToArray();
    }

    public void AddItem(string name, FuelType fuelType, int cost, List<FurnitureObject> requiredFurniture = null)
    {
        menu.Add(new MenuItem(name, fuelType, cost, requiredFurniture));
    }

    public void RemoveItem(string itemName)
    {
        for (int i = 0; i < menu.Count; i++)
        {
            if (menu[i].name == itemName)
            {
                menu.RemoveAt(i);
                break;
            }
        }
    }
    
    // For now, add the default menu items on start
    void Start()
    {
        AddItem("Unleaded",FuelType.Unleaded,2);
        AddItem("Diesel",FuelType.Diesel,3);
        AddItem("Premium",FuelType.Premium,5);
    }
}