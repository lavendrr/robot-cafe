using System.Collections.Generic;
using UnityEngine;

public class MultiSliderController : MonoBehaviour
{
    public RectTransform track;
    public GameObject handlePrefab;

    public List<MultiSliderHandle> handles = new();
    public float padding = 0.1f; // minimum value between two handles or handle and the edge
    public float snapIncrement = 0.1f;
    //public List<float> sections = new();

    private void Start()
    {
        Build(3);
    }

    public void Build(int ingredientCount)
    {
        // Clear old
        foreach (var h in handles)
            Destroy(h.gameObject);
        handles.Clear();

        // Create N-1 handles
        for (int i = 0; i < ingredientCount - 1; i++)
        {
            var handle = Instantiate(handlePrefab, track);
            var handleScript = handle.GetComponent<MultiSliderHandle>();
            handleScript.track = track;
            handleScript.sliderController = this;
            handleScript.snapIncrement = snapIncrement;
            handles.Add(handleScript);
        }

        // Evenly space handles within [padding, 1 - padding], snapping to increment
        float pad = Mathf.Clamp(padding, 0f, 0.49f);
        float available = 1f;// - 2f * pad;
        float inc = Mathf.Clamp(snapIncrement, 0f, 1f);
        for (int i = 0; i < handles.Count; i++)
        {
            float fraction = (i + 1f) / ingredientCount;
            float value = pad + fraction * available;

            // Snap to nearest increment if increment > 0
            if (inc > 0f)
            {
                value = Mathf.Round(value / inc) * inc;
            }

            // Ensure snapped value stays within padded bounds
            value = Mathf.Clamp(value, pad, 1f - pad);

            handles[i].SetNormalizedValue(value);
        }

        UpdateConstraints();
    }

    public void UpdateConstraints()
    {
        float pad = Mathf.Clamp(padding, 0f, 0.49f);

        for (int i = 0; i < handles.Count; i++)
        {
            float min = (i == 0) ? pad : handles[i - 1].GetNormalizedValue() + pad;
            float max = (i == handles.Count - 1) ? 1f - pad : handles[i + 1].GetNormalizedValue() - pad;

            // If padding is too large and min > max, collapse constraint to midpoint to avoid invalid ranges
            if (min > max)
            {
                float mid = (min + max) * 0.5f;
                min = max = Mathf.Clamp(mid, pad, 1f - pad);
            }

            handles[i].minValue = min;
            handles[i].maxValue = max;
        }
    }

    public List<float> GetPercentages()
    {
        List<float> values = new();

        float prev = 0f;
        foreach (var h in handles)
        {
            float v = h.GetNormalizedValue();
            values.Add(v - prev);
            prev = v;
        }

        values.Add(1f - prev);
        return values;
    }

    // public void UpdateVisuals()
    // {
    //     var percentages = GetPercentages();

    //     for (int i = 0; i < sections.Count; i++)
    //     {
    //         sections[i].percentage = percentages[i];
    //         sections[i].fillImage.fillAmount = percentages[i];
    //         sections[i].percentageLabel.text = Mathf.RoundToInt(percentages[i] * 100) + "%";
    //     }
    // }

}
