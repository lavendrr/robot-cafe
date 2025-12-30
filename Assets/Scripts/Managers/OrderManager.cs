using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [SerializeField]
    private GameObject cupPrefab, customerPrefab;
    private GameObject cupSpawn, customerTarget;
    public List<GameObject> customerList = new();
    public int completedCounter = 0;

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

        cupSpawn = GameObject.Find("CupSpawn");
        // Subscribe to the state change event
        StateManager.Instance.OnStateChanged += HandleStateChange;

        if (StateManager.Instance.GetCurrentState().GetType() == typeof(ShiftState))
        {
            ResetOrders();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the state change event
        StateManager.Instance.OnStateChanged -= HandleStateChange;
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void HandleStateChange(State newState)
    {
        if (newState.GetType() == typeof(ShiftState))
        {
            ResetOrders();
        }
    }

    public void NewCustomer()
    {
        if (customerTarget == null)
        {
            customerTarget = CafeLayoutManager.Instance.deliveryWindowObject.transform.Find("CustomerTarget").gameObject;
        }
        customerList.Add(Instantiate(customerPrefab, customerTarget.transform));
    }

    public bool FillOrder(GameObject cupObj, Vector3 position)
    {
        // Check if the current customer's order matches the delivered order
        Customer currentCustomer = customerList[0].GetComponent<Customer>();
        if (currentCustomer.GetOrder() == null)
        {
            currentCustomer.WrongOrder();
            return false;
        }
        else if (currentCustomer.GetOrder().orderItem.CompareDrink(cupObj.GetComponent<Cup>().drink))
        {
            // Remove the order from the list, initialize a new customer, and spawn a new cup and delete the one used to fill the order
            var gain = currentCustomer.GetOrder().orderItem.cost;
            SaveManager.Instance.AdjustPlayerMoney(gain);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.bellDing, position);

            currentCustomer.Leave();
            customerList.RemoveAt(0);

            NewCustomer();
            completedCounter++;

            UIManager.Instance.SetOrdersCompleted(completedCounter, gain);
            UIManager.Instance.SetOrderInfo("");

            //SpawnCup();
            Destroy(cupObj);
            return true;
        }
        else
        {
            currentCustomer.WrongOrder();
            return false;
        }
    }

    public void SpawnCup()
    {
        Instantiate(cupPrefab, cupSpawn.transform.position, Quaternion.identity, cupSpawn.transform);
    }

    public void ResetOrders()
    {
        customerList = new List<GameObject>();
        completedCounter = 0;
    }
}

public class Order
{
    public MenuItem orderItem;
    public Order()
    {
        // Randomly choose an order from the available menu items
        MenuItem[] menuItems = MenuManager.Instance.ListItems();
        if (menuItems.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, menuItems.Length);
            orderItem = menuItems[randomIndex];
            GetWeightedOrder();
        }
        else
        {
            Debug.LogError("OrderManager;; The menu is empty.");
        }
        UIManager.Instance.SetOrderInfo(orderItem.name);
    }

    public MenuItem GetWeightedOrder()
    {
        MenuItem[] menu = MenuManager.Instance.ListItems();
        // Get the average menu item cost
        float avg = (float) menu.Sum(x => x.cost) / menu.Length;
        
        // Fill an array with default probabilities
        float[] probs = Enumerable.Repeat(1 / (float) menu.Length, menu.Length).ToArray();
        foreach (float f in probs)
        {
            Debug.Log(f);
        }
        Debug.Log("---");
        probs = probs.Select((x, y) => x * (avg / menu[y].cost)).ToArray();
        foreach (float f in probs)
        {
            Debug.Log(f);
        }
        Debug.Log("---");
        float preScaleSum = probs.Sum();
        Debug.Log($"pre scale sum: {preScaleSum}");
        probs = probs.Select(x => x * (1 / preScaleSum)).ToArray();
        foreach (float f in probs)
        {
            Debug.Log(f);
        }
        Debug.Log($"post scale sum: {probs.Sum()}");

        return menu[0];
    }
}
