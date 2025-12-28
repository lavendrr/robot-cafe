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
    public List<FurnitureObject> requiredFurniture;

    public MenuItem(string _name, Dictionary<FuelType, float> _drinkComp, int _cost, string[] _requiredFurniture = null)
    {
        name = _name;
        drink.comp = _drinkComp;
        cost = _cost;
        requiredFurniture = GenerateFOList(_requiredFurniture);
    }

    private List<FurnitureObject> GenerateFOList(string[] input)
    {
        List<FurnitureObject> output = new();
        if (input is not null)
        {
            foreach (string item in input)
            {
                output.Add(MenuManager.Instance.GetFurnitureObject(item));
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
    string[] FO_names;
    private Dictionary<string, FurnitureObject> FO_dictionary = new Dictionary<string, FurnitureObject>();
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
                FO_dictionary.Add(file.Split("Assets/Resources/Prefabs/FurnitureObjects/")[1].Split(".asset")[0], null);
            }

            // Test print of all located FurnitureObjects
            // foreach (var key in FO_dictionary.Keys)
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

    public FurnitureObject GetFurnitureObject(string name)
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
        AddItem("Unleaded", new Dictionary<FuelType, float>() {{FuelType.Unleaded, 100f}}, 2, new string[] {"FO_CoffeeMachine"});
        AddItem("Diesel", new Dictionary<FuelType, float>() {{FuelType.Diesel, 100f}},3);
        AddItem("Premium", new Dictionary<FuelType, float>() {{FuelType.Premium, 100f}}, 5);

        // foreach (MenuItem item in menu)
        // {
        //     foreach (FurnitureObject obj in item.requiredFurniture)
        //     {
        //         Debug.Log($"{item.name} requires {obj.furnitureName}");
        //     }
        // }
    }
}