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
    public int cost;
    public Drink recipe;
    public MenuItem(string _name, int _cost, Drink _recipe)
    {
        name = _name;
        cost = _cost;
        recipe = _recipe;
    }
}

public class Drink
{
    public FuelType fuelType = FuelType.None;
    public bool oxidized = false;
    public Drink(FuelType _fuelType, bool _oxidized = false)
    {
        fuelType = _fuelType;
        oxidized = _oxidized;
    }
    public void ResetDrink()
    {
        fuelType = FuelType.None;
        oxidized = false;
    }
    public bool CompareDrink(Drink _drink)
    {
        if (_drink.fuelType == fuelType && _drink.oxidized == oxidized)
        {
            return true;
        }
        
        return false;
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

    public void AddItem(string name, FuelType fuelType, int cost, bool oxidant = false)
    {
        menu.Add(new MenuItem(name, cost, new Drink(fuelType, oxidant)));
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
        AddItem("Unleaded", FuelType.Unleaded, 2);
        AddItem("Diesel", FuelType.Diesel, 3);
        AddItem("Premium", FuelType.Premium, 5);
        AddItem("Oxidized Diesel", FuelType.Diesel, 8, true);
    }
}