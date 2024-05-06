using System;
using System.Collections.Generic;
using UnityEngine;

namespace Orders
{
    public enum FuelType
    {
        None,
        Unleaded,
        Diesel,
        Premium
    }

    public class OrderManager : MonoBehaviour
    {
        public static OrderManager instance { get; private set; }

        [SerializeField]
        private GameObject cupPrefab, customerPrefab;
        private GameObject cupSpawn, customerRoot;
        public List<GameObject> customerList = new();
        public int completedCounter = 0;

        private void Awake() 
        { 
            // If there is an instance, and it's not me, delete myself.
            if (instance != null && instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                instance = this; 
            }

            cupSpawn = GameObject.Find("CupSpawn");
            customerRoot = GameObject.Find("CustomerRoot");
        }

        // Start is called before the first frame update
        void Start()
        {
            NewCustomer();
        }

        public void NewCustomer()
        {
            customerList.Add(Instantiate(customerPrefab, customerRoot.transform));
        }

        public void FillOrder(GameObject cupObj)
        {
            Customer currentCustomer = customerList[0].GetComponent<Customer>();
            if (currentCustomer.GetOrder().orderType == cupObj.GetComponent<Cup>().GetFuelType())
            {
                // Remove the order from the list, add a new order, and spawn a new cup and delete the one used to fill the order
                Debug.Log("Order filled!");
                currentCustomer.Leave();
                customerList.RemoveAt(0);
                UIManager.instance.SetOrderInfo("");
                NewCustomer();
                completedCounter++;
                UIManager.instance.CompleteOrder(completedCounter);
                SpawnCup();
                Destroy(cupObj);
            }
            else
            {
                Debug.Log("Incorrect order.");
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
        public Order()
        {
            // Randomly assign the order type from the available enums
            orderType = (FuelType)Enum.GetValues(typeof(FuelType)).GetValue(UnityEngine.Random.Range(1, 4));
            UIManager.instance.SetOrderInfo(orderType.ToString());
            Debug.Log("New order's type is " + orderType.ToString());
        }
    }
}