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
            if (fuelType == FuelType.None)
            {
                gameObject.layer = LayerMask.NameToLayer("Grabbed");
                var animator = transform.parent.parent.parent.gameObject.GetComponentInChildren<Animator>();
                animator.SetTrigger("LeverPull");
                AudioManager.instance.PlaySFX(AudioManager.instance.pourCoffee, position);
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
                float t = elapsedTime / duration;
                drinkMeshTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            drinkMeshTransform.localScale = targetScale;
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
