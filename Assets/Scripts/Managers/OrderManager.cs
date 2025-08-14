using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [SerializeField]
    private GameObject cupPrefab, customerPrefab;
    private GameObject cupSpawn, customerRoot;
    public List<GameObject> customerList = new();
    public int completedCounter = 0;

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

        cupSpawn = GameObject.Find("CupSpawn");
        customerRoot = GameObject.Find("CustomerRoot");
        // Subscribe to the state change event
        StateManager.Instance.OnStateChanged += HandleStateChange;

        if (StateManager.Instance.GetCurrentState().GetType() == typeof(ShiftState))
        {
            ResetOrders();
            NewCustomer();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the state change event
        StateManager.Instance.OnStateChanged -= HandleStateChange;
    }

    private void HandleStateChange(State newState)
    {
        if (newState.GetType() == typeof(ShiftState))
        {
            ResetOrders();
            NewCustomer();
        }
    }

    public void NewCustomer()
    {
        customerList.Add(Instantiate(customerPrefab, customerRoot.transform));
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
        else if (currentCustomer.GetOrder().orderItem.fuelType == cupObj.GetComponent<Cup>().GetFuelType())
        {
            // Remove the order from the list, initialize a new customer, and spawn a new cup and delete the one used to fill the order
            Debug.Log("Order filled!");
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
            Debug.Log("Incorrect order.");
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
        }
        else
        {
            Debug.LogError("OrderManager;; The menu is empty.");
        }
        UIManager.Instance.SetOrderInfo(orderItem.name);
        Debug.Log("New order's type is " + orderItem.name);
    }
}
