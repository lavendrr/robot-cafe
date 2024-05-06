using UnityEngine;
using System.Collections;

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
            // Fill the cup if it's empty, or do nothing if it's already full
            if (fuelType == FuelType.None)
            {
                // Make the cup un-interactable (until it is set to be interactive again by the animation function when finished)
                gameObject.layer = LayerMask.NameToLayer("Grabbed");

                // Animate the lever
                var animator = transform.parent.parent.parent.gameObject.GetComponentInChildren<Animator>();
                animator.SetTrigger("LeverPull");

                AudioManager.instance.PlaySFX(AudioManager.instance.pourCoffee, position);

                // Activate the drink mesh and animate it
                gameObject.GetComponentsInChildren<MeshRenderer>()[1].enabled = true;
                StartCoroutine(ScaleUpCoffeeMesh());
                fuelType = fillType;
                Debug.Log("Filled cup with " + fuelType.ToString());
            }
            else
            {
                Debug.Log("Cup already full.");
            }
        }

        // Animation function for the drink mesh
        private IEnumerator ScaleUpCoffeeMesh()
        {
            float elapsedTime = 0f;
            Vector3 startScale = new Vector3(0.81f, 0.094f, 0.81f);
            Vector3 targetScale = Vector3.one;
            float duration = 1.5f;

            Transform drinkMeshTransform = transform.Find("SM_drink");
            if (drinkMeshTransform == null)
            {
                Debug.LogError("Drink mesh transform not found.");
                yield break;
            }


            while (elapsedTime < duration)
            {
                // Use linear interpolation to scale the mesh up
                float t = elapsedTime / duration;
                drinkMeshTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            drinkMeshTransform.localScale = targetScale;
            // Make the cup interactable again
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }

        public void Empty()
        {
            fuelType = FuelType.None;
            gameObject.GetComponentsInChildren<MeshRenderer>()[1].enabled = false;
        }

        void OnCollisionEnter(Collision other)
        {
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
