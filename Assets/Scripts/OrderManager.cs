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

        public List<Order> orderList = new List<Order>();
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
        }

        // Start is called before the first frame update
        void Start()
        {
            orderList.Add(new Order());
        }

        public void FillOrder(GameObject cupObj)
        {
            Order currentOrder = orderList[0];
            if (currentOrder.orderType == cupObj.GetComponent<Cup>().GetFuelType())
            {
                Debug.Log("Order filled!");
                orderList.RemoveAt(0);
                completedCounter++;
                orderList.Add(new Order());
                cupObj.GetComponent<Cup>().Empty();
            }
            else
            {
                Debug.Log("Incorrect order.");
            }
        }
    }

    public class Order
    {
        public FuelType orderType;
        public Order()
        {
            // Randomly assign the order type from the available enums
            orderType = (FuelType)Enum.GetValues(typeof(FuelType)).GetValue(UnityEngine.Random.Range(1, 4));
            Debug.Log("New order's type is " + orderType.ToString());
        }

    }
}
