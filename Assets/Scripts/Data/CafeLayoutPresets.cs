using UnityEngine;

public static class CafeLayoutPresets
{
    // Presets are authored as CafeLayoutPreset assets under Resources/LayoutPresets.
    public static LevelLayout GetPreset(string name)
    {
        CafeLayoutPreset asset = Resources.Load<CafeLayoutPreset>("LayoutPresets/" + name);
        if (asset != null)
        {
            return asset.ToLevelLayout();
        }

        Debug.LogWarning("Preset " + name + " not found.");
        return null;
    }
}
