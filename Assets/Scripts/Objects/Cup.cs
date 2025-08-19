using UnityEngine;
using System.Collections;

public class Cup : MonoBehaviour
{
    private FuelType fuelType = FuelType.None;

    public FuelType GetFuelType()
    {
        return fuelType;
    }

    public void Fill(FuelType fillType, Vector3 position, Material fuelMaterial)
    {
        // Fill the cup if it's empty, or do nothing if it's already full
        if (fuelType == FuelType.None)
        {
            // Make the cup un-grabbable (until it is set to be grabbable again by the animation function when finished)
            gameObject.tag = "Untagged";

            // Animate the lever
            var animator = transform.parent.parent.parent.gameObject.GetComponentInChildren<Animator>();
            animator.SetTrigger("LeverPull");

            AudioManager.Instance.PlaySFX(AudioManager.Instance.pourCoffee, position);

            // Activate the drink mesh and animate it
            MeshRenderer drinkMeshRenderer = gameObject.GetComponentsInChildren<MeshRenderer>()[1];
            if (drinkMeshRenderer != null)
            {
                drinkMeshRenderer.enabled = true;
                drinkMeshRenderer.material = fuelMaterial;
                StartCoroutine(ScaleUpCoffeeMesh());
                fuelType = fillType;
            }
            else
            {
                Debug.LogError("Drink mesh renderer not found.");
            }
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
        gameObject.tag = "Grabbable";
    }

    public bool Empty()
    {
        // Returns true if the cup was full then emptied, returns false if the cup was already empty
        if (fuelType != FuelType.None)
        {
            fuelType = FuelType.None;
            gameObject.GetComponentsInChildren<MeshRenderer>()[1].enabled = false;
            return true;
        }
        else
        {
            return false;
        }

    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "ItemKillbox")
        {
            Destroy(gameObject);
        }
    }

    void OnSlotInsert()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.setDown, transform.position);
    }

    public void Destroy()
    {
        Slot slot = GetComponentInParent<Slot>();
        if (slot != null)
        {
            slot.RemoveObj();
        }
        Destroy(gameObject);
    }
}
