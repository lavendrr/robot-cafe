using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Cup : MonoBehaviour
{
    private FuelType fuelType = FuelType.None;
    bool filling = false;
    private Dictionary<FuelType, float> drinkComp = new();

    public FuelType GetFuelType()
    {
        return fuelType;
    }

    public Dictionary<FuelType, float> GetDrinkComp()
    {
        return drinkComp;
    }

    // Returns true if the drink overflowed from this fill
    public bool FillPartial(FuelType fuel, float fillAmount)
    {
        if (!drinkComp.ContainsKey(fuel))
        {
            drinkComp[fuel] = 0f;
        }

        drinkComp[fuel] = drinkComp[fuel] + fillAmount;
        UpdateCoffeeMesh();

        Debug.Log($"Partial fill of {fuel} w/ amount {fillAmount}");
        foreach (KeyValuePair<FuelType, float> pair in drinkComp)
        {
            Debug.Log($"Drink has {pair.Key} at {pair.Value}");
        }

        if (drinkComp.Sum(x => x.Value) >= 100f)
        {
            Debug.Log("Overflowed");
            return true;
        }
        return false;
    }

    public void Fill(FuelType fillType, Vector3 position, Material fuelMaterial)
    {
        // Fill the cup if it's empty, or do nothing if it's already full
        // if (fuelType == FuelType.None)
        var animator = transform.parent.parent.parent.gameObject.GetComponentInChildren<Animator>();

        if (!filling)
        {
            filling = true;
            // Make the cup un-grabbable (until it is set to be grabbable again by the animation function when finished)
            gameObject.tag = "Untagged";

            // Animate the lever
            // var animator = transform.parent.parent.parent.gameObject.GetComponentInChildren<Animator>();
            animator.SetTrigger("LeverPullStart");

            AudioManager.Instance.PlaySFX(AudioManager.Instance.pourCoffee, position);

            // Activate the drink mesh and animate it
            MeshRenderer drinkMeshRenderer = gameObject.GetComponentsInChildren<MeshRenderer>()[1];
            if (drinkMeshRenderer != null)
            {
                drinkMeshRenderer.enabled = true;
                drinkMeshRenderer.material = fuelMaterial;
                StartCoroutine(ScaleUpCoffeeMesh(fillType));
                fuelType = fillType;
            }
            else
            {
                Debug.LogError("Drink mesh renderer not found.");
            }
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

        Transform drinkMeshTransform = transform.Find("SM_drink");
        if (drinkMeshTransform == null)
        {
            Debug.LogError("Drink mesh transform not found.");
            return;
        }
        drinkMeshTransform.localScale = Vector3.Lerp(startScale, targetScale, Math.Clamp(drinkComp.Sum(x => x.Value)/100, 0, 1));
    }

    // Animation function for the drink mesh
    private IEnumerator ScaleUpCoffeeMesh(FuelType fuel)
    {
        float elapsedTime = 0f;
        // Vector3 startScale = new Vector3(0.81f, 0.094f, 0.81f);
        // Vector3 targetScale = Vector3.one;
        float duration = 1.5f;

        // Transform drinkMeshTransform = transform.Find("SM_drink");
        // if (drinkMeshTransform == null)
        // {
        //     Debug.LogError("Drink mesh transform not found.");
        //     yield break;
        // }

        int a = 0;


        while (elapsedTime < duration)
        {
            // Use linear interpolation to scale the mesh up
            float t = elapsedTime / duration;
            //drinkMeshTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsedTime += Time.deltaTime;

            FillPartial(fuel, 1f);
            if (!filling)
            {
                Debug.Log("Stopped filling");
                gameObject.tag = "Grabbable";
                yield break;
            }
            a++;
            yield return new WaitForSeconds(0.015f);
        }

        Debug.Log(a);

        //drinkMeshTransform.localScale = targetScale;
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
