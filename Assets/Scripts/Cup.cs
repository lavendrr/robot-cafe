using UnityEngine;
using UnityEngine.Assertions;

namespace Orders
{
    public class Cup : MonoBehaviour
    {
        private FuelType fuelType = FuelType.None;

        public FuelType GetFuelType()
        {
            return fuelType;
        }

        public void Fill(FuelType fillType)
        {
            if (fuelType == FuelType.None)
            {
                gameObject.GetComponentsInChildren<MeshRenderer>()[1].enabled = true;
                fuelType = fillType;
                Debug.Log("Filled cup with " + fuelType.ToString());
            }
            else
            {
                Debug.Log("Cup already full.");
            }
        }

        public void Empty()
        {
            fuelType = FuelType.None;
            gameObject.GetComponentsInChildren<MeshRenderer>()[1].enabled = false;
        }
    }
}
