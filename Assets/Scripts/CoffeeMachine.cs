using UnityEngine;

public class CoffeeMachine : MonoBehaviour
{
    public void FillCup()
    {
        Debug.Log("Trying to fill");
        var cupObj = gameObject.GetComponentInChildren<Slot>().GetSlottedObj();
        if (cupObj != null)
        {
            cupObj.GetComponent<Cup>().Fill();
        }
        else
        {
            Debug.Log("No cup in the machine.");
        }
    }
}
