using Orders;
using UnityEngine;

public class Delivery : MonoBehaviour
{
    public void Deliver()
    {
        Debug.Log("Trying to deliver");
        var cupObj = gameObject.transform.parent.gameObject.GetComponentInChildren<Slot>().GetSlottedObj();
        if (cupObj != null)
        {
            OrderManager.instance.FillOrder(cupObj, transform.position);
        }
    }
}
