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
    [SerializeField]
    Material liquidMaterial;
    [SerializeField]
    Liquid liquid;
    public Drink drink = new();

    // Order bases were first poured in; drives bottom -> top layer stacking.
    readonly List<IngredientData> pourOrder = new();
    // Reused each refresh so filling (every ~15ms) doesn't allocate.
    readonly List<Color> layerColorScratch = new();
    readonly List<float> layerFractionScratch = new();

    public Dictionary<IngredientData, int> GetDrinkComp()
    {
        return drink.bases;
    }

    public void ToggleFill(IngredientData fillType, Vector3 position)
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

            // All bases share one liquid material; per-cup colour comes from the layer bands,
            // so assign the shared material (no per-fuel material swap, no cloning).
            if (liquidMaterial != null && drinkMeshRenderer.sharedMaterial != liquidMaterial)
                drinkMeshRenderer.sharedMaterial = liquidMaterial;
            StartCoroutine(Fill(fillType, 1f));
        }
        else
        {
            filling = false;
            animator.SetTrigger("LeverPullStop");
        }
    }

    // Drives the shader's liquid level from the drink's total fuel (0..100 -> 0..1). Replaces the
    // old drink-mesh scaling: the mesh stays full-size and the shader's fill cut shows the level.
    private void UpdateFillLevel()
    {
        if (liquid == null && drinkMeshRenderer != null)
            liquid = drinkMeshRenderer.GetComponent<Liquid>();
        if (liquid == null)
            return;

        float total = drink.bases.Sum(x => x.Value);
        liquid.SetFillLevel(Mathf.Clamp01(total / 100f));
    }

    // Pushes the current base composition to the liquid shader as colour layers (bottom -> top
    // in pour order). Liquid owns the material property block, so we only hand it the bands.
    private void RefreshLayers()
    {
        if (liquid == null && drinkMeshRenderer != null)
            liquid = drinkMeshRenderer.GetComponent<Liquid>();
        if (liquid == null)
            return;

        layerColorScratch.Clear();
        layerFractionScratch.Clear();

        int total = drink.bases.Values.Sum();
        foreach (IngredientData ing in pourOrder)
        {
            if (!drink.bases.TryGetValue(ing, out int pct) || pct <= 0)
                continue;
            layerColorScratch.Add(ing.sliderColor);
            layerFractionScratch.Add(total > 0 ? (float)pct / total : 0f);
        }

        liquid.SetLayers(layerColorScratch, layerFractionScratch);
    }

#if UNITY_EDITOR
    [System.Serializable]
    struct TestLayer
    {
        public IngredientData ingredient;
        [Range(0, 100)] public int percent;
    }

    [Header("Editor Composition Preview")]
    [Tooltip("When on, edits to the test composition below are previewed live on the liquid in the editor.")]
    [SerializeField]
    bool previewComposition;
    [Tooltip("Bands bottom -> top. Percents are relative (normalized), so they need not sum to 100.")]
    [SerializeField]
    List<TestLayer> testComposition = new();

    void OnValidate()
    {
        if (!previewComposition)
            return;

        // Defer out of OnValidate before touching renderer state / property blocks.
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null && previewComposition)
                ApplyTestComposition();
        };
    }

    [ContextMenu("Apply Test Composition")]
    void ApplyTestComposition()
    {
        if (liquid == null && drinkMeshRenderer != null)
            liquid = drinkMeshRenderer.GetComponent<Liquid>();
        if (liquid == null)
            return;

        if (liquidMaterial != null && drinkMeshRenderer != null &&
            drinkMeshRenderer.sharedMaterial != liquidMaterial)
            drinkMeshRenderer.sharedMaterial = liquidMaterial;

        layerColorScratch.Clear();
        layerFractionScratch.Clear();
        foreach (TestLayer layer in testComposition)
        {
            if (layer.ingredient == null || layer.percent <= 0)
                continue;
            layerColorScratch.Add(layer.ingredient.sliderColor);
            layerFractionScratch.Add(layer.percent);   // SetLayers normalizes
        }

        liquid.SetLayers(layerColorScratch, layerFractionScratch);
    }
#endif

    // Animation function for the drink mesh
    private IEnumerator Fill(IngredientData fuel, float fillAmount)
    {
        if (!drink.bases.ContainsKey(fuel))
        {
            drink.bases[fuel] = 0;
            pourOrder.Add(fuel);
        }

        while (filling)
        {
            drink.bases[fuel] += 1;
            UpdateFillLevel();
            RefreshLayers();

            foreach (KeyValuePair<IngredientData, int> pair in drink.bases)
            {
                Debug.Log($"Drink has {pair.Key} at {pair.Value}");
            }

            if (drink.bases.Sum(x => x.Value) >= 100)
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
        if (drink.bases.Count != 0)
        {
            drink.bases.Clear();
            pourOrder.Clear();
            RefreshLayers();
            UpdateFillLevel();
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
