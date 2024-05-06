using UnityEngine;

namespace Orders
{
    public class Cup : MonoBehaviour
    {
        private FuelType fuelType = FuelType.None;

        public FuelType GetFuelType()
        {
            return fuelType;
        }

        public void Fill(FuelType fillType, Vector3 position)
        {
            if (fuelType == FuelType.None)
            {
                // AudioManager.instance.PlaySFX(AudioManager.instance.pourCoffee, position);
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

        void OnCollisionEnter(Collision other)
        {
            Debug.Log("Colliding");
            if (other.gameObject.name == "ItemKillbox")
            {
                Debug.Log("Hit box");
                OrderManager.instance.SpawnCup();
                Debug.Log("Cup fell out of bounds and was replaced");
                Destroy(gameObject);
            }
        }
    }
}
