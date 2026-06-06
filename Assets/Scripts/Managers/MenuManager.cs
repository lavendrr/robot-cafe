using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

public enum FuelType
{
    None,
    Unleaded,
    Diesel,
    Premium
}

public class Drink
{
    public Dictionary<FuelType, float> comp = new();
    public List<string> toppings = new();
}

public class MenuItem
{
    public string name;
    public Drink drink = new();
    public int cost;
    public List<FurnitureData> requiredFurniture;

    public MenuItem(string _name, Dictionary<FuelType, float> _drinkComp, int _cost, string[] _requiredFurniture = null)
    {
        name = _name;
        drink.comp = _drinkComp;
        cost = _cost;
        requiredFurniture = GenerateFurnitureDataList(_requiredFurniture);
    }

    private List<FurnitureData> GenerateFurnitureDataList(string[] input)
    {
        List<FurnitureData> output = new();
        if (input is not null)
        {
            foreach (string item in input)
            {
                output.Add(MenuManager.Instance.GetFurnitureData(item));
            }
        }
        return output;
    }

    public bool CompareDrink(Drink compare)
    {
        if (drink.comp.Count != compare.comp.Count)
        {
            return false;
        }
        
        foreach (var x in drink.comp)
        {
            if (!compare.comp.Keys.Contains(x.Key))
            {
                return false;
            }
            if (!(x.Value - 20f <= compare.comp[x.Key] && compare.comp[x.Key] <= x.Value + 20f))
            {
                return false;
            }
        }

        return true;
    }
}

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    private List<MenuItem> menu = new List<MenuItem>();
    string[] furnitureDataNames;
    private Dictionary<string, FurnitureData> furnitureDataDictionary = new Dictionary<string, FurnitureData>();
    string prefix = "Prefabs/FurnitureData/";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            string folderPath = Path.Combine(Application.dataPath, "Resources/Prefabs/FurnitureData");
            if (Directory.Exists(folderPath))
            {
                furnitureDataNames = Directory.GetFiles(folderPath, "*.asset", SearchOption.TopDirectoryOnly);
            }
            else
            {
                furnitureDataNames = new string[0];
            }

            foreach (string file in furnitureDataNames)
            {
                furnitureDataDictionary.Add(file.Split("Assets/Resources/Prefabs/FurnitureData/")[1].Split(".asset")[0], null);
            }

            // Test print of all located FurnitureData
            // foreach (var key in furnitureDataDictionary.Keys)
            // {
            //     Debug.Log(key);
            // }
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

    public FurnitureData GetFurnitureData(string name)
    {
        if (furnitureDataDictionary[name] == null || !furnitureDataDictionary.ContainsKey(name))
        {
            furnitureDataDictionary[name] = Resources.Load<FurnitureData>(prefix + name);
        }

        return furnitureDataDictionary[name];
    }

    public MenuItem[] ListItems()
    {
        return menu.ToArray();
    }

    public void AddItem(string name, Dictionary<FuelType, float> drinkComp, int cost, string[] requiredFurniture = null)
    {
        menu.Add(new MenuItem(name, drinkComp, cost, requiredFurniture));
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
        AddItem("Unleaded", new Dictionary<FuelType, float>() {{FuelType.Unleaded, 100f}}, 2, new string[] {"FD_CoffeeMachine"});
        AddItem("Diesel", new Dictionary<FuelType, float>() {{FuelType.Diesel, 100f}},3);
        AddItem("Premium", new Dictionary<FuelType, float>() {{FuelType.Premium, 100f}}, 5);

        // foreach (MenuItem item in menu)
        // {
        //     foreach (FurnitureData obj in item.requiredFurniture)
        //     {
        //         Debug.Log($"{item.name} requires {obj.furnitureName}");
        //     }
        // }
    }
}