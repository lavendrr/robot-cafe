using UnityEngine;


    public class CoffeeMachine : MonoBehaviour
    {
        [SerializeField]
        private FuelType coffeeMachineType;
        [SerializeField]
        private Material fuelMaterial;
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
                cupObj.GetComponent<Cup>().Fill(coffeeMachineType, slot.gameObject.transform.position, fuelMaterial);
            }
            else
            {
                Debug.Log("No cup in the machine.");
            }
        }
    }
