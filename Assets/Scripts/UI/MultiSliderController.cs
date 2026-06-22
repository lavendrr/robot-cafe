using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiSliderController : MonoBehaviour
{
    public Action OnMultiSliderChanged;
    public RectTransform track;
    public GameObject handlePrefab;

    public List<MultiSliderHandle> handles = new();
    public float snapIncrement = 0.1f;
    public List<float> segmentPercentages;

    // Minimum value between two handles (or handle and edge), as the 0..1 equivalent of
    // DrinkEditorUI.minimumBasePortion. The 0..100 source of truth lives in DrinkEditorUI.
    public float padding => DrinkEditorUI.minimumBasePortion / 100f;

    public void Build()
    {
        // Clear old
        foreach (var h in handles)
            Destroy(h.gameObject);
        handles.Clear();
        segmentPercentages = new List<float>();

        int ingredientCount = DrinkEditorUI.Instance?.CurrentItem?.drink.bases.Count ?? 0;

        // Handle 0 ingredient case
        if (ingredientCount < 1)
        {
            return;
        }

        // Create N-1 handles
        for (int i = 0; i < ingredientCount - 1; i++)
        {
            var handle = Instantiate(handlePrefab, track);
            var handleScript = handle.GetComponent<MultiSliderHandle>();
            handleScript.track = track;
            handleScript.sliderController = this;
            handleScript.snapIncrement = snapIncrement;
            handles.Add(handleScript);
            segmentPercentages.Add(1.0f / ingredientCount);
        }

        // Add one more segment representation for the one between
        // the uppermost handle and the top of the slider
        segmentPercentages.Add(1.0f / ingredientCount);

        // Evenly space handles, snapping to increment
        float inc = Mathf.Clamp(snapIncrement, 0f, 1f);
        for (int i = 0; i < handles.Count; i++)
        {
            float value = (i + 1f) / ingredientCount;
            if (inc > 0f)
                value = Mathf.Round(value / inc) * inc;
            handles[i].SetNormalizedValue(value);
        }

        UpdateSegments();
        OnMultiSliderChanged?.Invoke();
    }

    // The single mover used by both dragging and the +/- buttons. Places handle `index` at
    // `target`, then pushes neighbours outward so every segment keeps at least `padding`. Pushing
    // is what produces the cascade: a move that overruns the adjacent handle carries it (and the
    // ones beyond) along until a segment with slack absorbs the change.
    public void MoveHandle(int index, float target)
    {
        int count = handles.Count;
        if (index < 0 || index >= count)
            return;

        float pad = Mathf.Clamp(padding, 0f, 0.49f);

        if (snapIncrement > 0f)
            target = Mathf.Round(target / snapIncrement) * snapIncrement;

        // Clamp so the handles below and above can all still fit at >= pad apart.
        float minPos = (index + 1) * pad;
        float maxPos = 1f - (count - index) * pad;
        target = Mathf.Clamp(target, minPos, maxPos);

        float[] pos = new float[count];
        for (int i = 0; i < count; i++)
            pos[i] = handles[i].currentValue;

        pos[index] = target;
        for (int j = index + 1; j < count; j++)        // push up
            if (pos[j] < pos[j - 1] + pad) pos[j] = pos[j - 1] + pad;
        for (int j = index - 1; j >= 0; j--)           // push down
            if (pos[j] > pos[j + 1] - pad) pos[j] = pos[j + 1] - pad;

        for (int i = 0; i < count; i++)
            handles[i].SetNormalizedValue(pos[i]);

        UpdateSegments();
        OnMultiSliderChanged?.Invoke();
    }

    // Resize one segment by delta (normalized 0..1): positive grows it, negative shrinks it.
    // Growing steals from the nearest side with room (preferring above, then below), and MoveHandle's
    // push cascade carries the change past any neighbours already pinned at the minimum.
    public void AdjustSegment(int segmentIndex, float delta)
    {
        int count = handles.Count;
        if (count == 0)
            return; // a lone ingredient is always 100%; nothing to redistribute

        float pad = Mathf.Clamp(padding, 0f, 0.49f);
        float step = Mathf.Abs(delta);
        if (step <= 0f)
            return;

        bool hasUpper = segmentIndex < count;       // upper boundary is handle[segmentIndex]
        bool hasLower = segmentIndex >= 1;          // lower boundary is handle[segmentIndex - 1]

        if (delta > 0f)
        {
            // Grow: pull from whichever side still has room, nearest (above) first.
            if (hasUpper && RoomOnSide(segmentIndex, +1, pad) >= step - pad * 0.5f)
                MoveHandle(segmentIndex, handles[segmentIndex].currentValue + step);
            else if (hasLower && RoomOnSide(segmentIndex, -1, pad) >= step - pad * 0.5f)
                MoveHandle(segmentIndex - 1, handles[segmentIndex - 1].currentValue - step);
        }
        else
        {
            // Shrink: the segment gives a step to an adjacent one (which can always absorb it),
            // provided this segment itself stays at or above the minimum.
            if (segmentPercentages[segmentIndex] - step < pad - pad * 0.5f)
                return;
            if (hasUpper)
                MoveHandle(segmentIndex, handles[segmentIndex].currentValue - step);
            else
                MoveHandle(segmentIndex - 1, handles[segmentIndex - 1].currentValue + step);
        }
    }

    // Whether segmentIndex can grow / shrink by a full step. Grow needs room anywhere else on the
    // track (not just the immediate neighbour, matching AdjustSegment's cascade); shrink needs this
    // segment to sit above the minimum. Half-step threshold avoids float noise on the snap grid.
    public void GetSegmentAdjustable(int segmentIndex, out bool canShrink, out bool canGrow)
    {
        canShrink = false;
        canGrow = false;
        if (handles.Count == 0)
            return; // a lone ingredient is always 100%; neither button does anything

        float pad = Mathf.Clamp(padding, 0f, 0.49f);
        float step = pad; // the +/- step equals the minimum portion
        float half = pad * 0.5f;

        canShrink = segmentPercentages[segmentIndex] - step >= pad - half;

        float roomElsewhere = 0f;
        for (int j = 0; j < segmentPercentages.Count; j++)
            if (j != segmentIndex)
                roomElsewhere += segmentPercentages[j] - pad;
        canGrow = roomElsewhere >= step - half;
    }

    // Total spare room (above the minimum) on one side of a segment. dir +1 = segments above, -1 = below.
    private float RoomOnSide(int segmentIndex, int dir, float pad)
    {
        float room = 0f;
        if (dir > 0)
            for (int j = segmentIndex + 1; j < segmentPercentages.Count; j++)
                room += segmentPercentages[j] - pad;
        else
            for (int j = segmentIndex - 1; j >= 0; j--)
                room += segmentPercentages[j] - pad;
        return room;
    }

    public void UpdateSegments()
    {
        for (int i = 0; i < segmentPercentages.Count; i++)
        {
            float lowerBound = (i == 0) ? 0 : handles[i - 1].currentValue;
            float upperBound = (i == segmentPercentages.Count - 1) ? 1 : handles[i].currentValue;
            segmentPercentages[i] = (float)Math.Round(upperBound - lowerBound, 1);
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
}
