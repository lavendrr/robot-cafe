using UnityEngine;

namespace Orders
{
    public class CoffeeMachine : MonoBehaviour
    {
        [SerializeField]
        private FuelType coffeeMachineType;
        public void FillCup()
        {
            Debug.Log("Trying to fill");
            var cupObj = gameObject.GetComponentInChildren<Slot>().GetSlottedObj();
            if (cupObj != null)
            {
                cupObj.GetComponent<Cup>().Fill(coffeeMachineType);
            }
            else
            {
                Debug.Log("No cup in the machine.");
            }
        }
    }
}
