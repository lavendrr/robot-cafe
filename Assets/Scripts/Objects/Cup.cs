using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Cup : MonoBehaviour
{
    bool filling = false;
    [SerializeField]
    MeshRenderer drinkMeshRenderer;
    public Drink drink = new();

    public Dictionary<FuelType, float> GetDrinkComp()
    {
        return drink.comp;
    }

    public void ToggleFill(FuelType fillType, Vector3 position, Material fuelMaterial)
    {
        // Fill the cup if it's empty, or do nothing if it's already full
        var animator = transform.parent.parent.parent.gameObject.GetComponentInChildren<Animator>();

        if (!filling)
        {
            filling = true;
            // Make the cup un-grabbable (until it is set to be grabbable again by the animation function when finished)
            gameObject.tag = "Untagged";

            // Animate the lever
            animator.SetTrigger("LeverPullStart");

            AudioManager.Instance.PlaySFX(AudioManager.Instance.pourCoffee, position);

            drinkMeshRenderer.material = fuelMaterial;
            StartCoroutine(Fill(fillType, 1f));
        }
        else
        {
            filling = false;
            animator.SetTrigger("LeverPullStop");
        }
    }

    // Static update function for the drink mesh
    private void UpdateCoffeeMesh()
    {
        Vector3 startScale = new Vector3(0.81f, 0.094f, 0.81f);
        Vector3 targetScale = Vector3.one;

        Transform drinkMeshTransform = drinkMeshRenderer.transform;
        drinkMeshTransform.localScale = Vector3.Lerp(startScale, targetScale, Math.Clamp(drink.comp.Sum(x => x.Value)/100, 0, 1));
    }

    // Animation function for the drink mesh
    private IEnumerator Fill(FuelType fuel, float fillAmount)
    {
        if (!drink.comp.ContainsKey(fuel))
        {
            drink.comp[fuel] = 0f;
        }

        while (filling)
        {
            drink.comp[fuel] = drink.comp[fuel] + fillAmount;
            UpdateCoffeeMesh();

            foreach (KeyValuePair<FuelType, float> pair in drink.comp)
            {
                Debug.Log($"Drink has {pair.Key} at {pair.Value}");
            }

            if (drink.comp.Sum(x => x.Value) >= 100f)
            {
                Debug.Log("Overflowed");
            }
            yield return new WaitForSeconds(0.015f);
        }

        Debug.Log("Stopped filling");
        // Make the cup interactable again
        gameObject.tag = "Grabbable";
        yield break;
    }

    public bool Empty()
    {
        // Returns true if the cup was full then emptied, returns false if the cup was already empty
        if (drink.comp.Count != 0)
        {
            drink.comp.Clear();
            UpdateCoffeeMesh();
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
