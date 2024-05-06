using UnityEngine;

namespace Orders
{
    public class CoffeeMachine : MonoBehaviour
    {
        [SerializeField]
        private FuelType coffeeMachineType;
        private Slot slot;

        private void Start()
        {
            slot = GetComponentInChildren<Slot>();
        }

        public void FillCup()
        {
            // Attempt to fill the cup if one is present
            Debug.Log("Trying to fill");
            var cupObj = slot.GetSlottedObj();
            if (cupObj != null)
            {
                cupObj.GetComponent<Cup>().Fill(coffeeMachineType, slot.gameObject.transform.position);
            }
            else
            {
                Debug.Log("No cup in the machine.");
            }
        }
    }
}
