// Script for each customer limb that catches the use interaction and tells the parent Customer GameObject to place the order
using UnityEngine;

public class CustomerUse : MonoBehaviour
{
    void OnUse()
    {
        transform.parent.gameObject.GetComponent<Customer>().PlaceOrder();
    }
}
