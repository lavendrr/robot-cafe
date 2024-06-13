using UnityEngine;
using System.Collections.Generic;

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

    public MenuItem(string _name, FuelType _fuelType, int _cost)
    {
        name = _name;
        fuelType = _fuelType;
        cost = _cost;
    }
}

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    private List<MenuItem> menu = new List<MenuItem>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public MenuItem[] ListItems()
    {
        return menu.ToArray();
    }

    public void AddItem(string name, FuelType fuelType, int cost)
    {
        menu.Add(new MenuItem(name, fuelType, cost));
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