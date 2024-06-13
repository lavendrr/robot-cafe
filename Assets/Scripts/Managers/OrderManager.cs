using System;
using System.Collections.Generic;
using UnityEngine;

public enum FuelType
{
    None,
    Unleaded,
    Diesel,
    Premium
}

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
        if (currentCustomer.GetOrder().orderType == cupObj.GetComponent<Cup>().GetFuelType())
        {
            // Remove the order from the list, initialize a new customer, and spawn a new cup and delete the one used to fill the order
            Debug.Log("Order filled!");
            SaveManager.Instance.AdjustPlayerMoney(currentCustomer.GetOrder().orderValue);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.bellDing, position);

            currentCustomer.Leave();
            customerList.RemoveAt(0);

            NewCustomer();
            completedCounter++;

            UIManager.Instance.CompleteOrder(completedCounter);
            UIManager.Instance.SetOrderInfo("");

            SpawnCup();
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
        Instantiate(cupPrefab, cupSpawn.transform.position, Quaternion.identity);
    }
}

public class Order
{
    public FuelType orderType;
    public int orderValue = 5;
    public Order()
    {
        // Randomly assign the order type from the available enums
        orderType = (FuelType)Enum.GetValues(typeof(FuelType)).GetValue(UnityEngine.Random.Range(1, 4));
        UIManager.Instance.SetOrderInfo(orderType.ToString());
        Debug.Log("New order's type is " + orderType.ToString());
    }
}
