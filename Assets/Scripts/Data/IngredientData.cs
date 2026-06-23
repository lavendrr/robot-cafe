using System;
using UnityEngine;

// [Flags] allows combining values with bitwise OR (e.g. MixIn | Topping).
// Each entry must be the next power of 2 (1 << N). To add a category, append a new entry with the next N.
// NOTE: existing values are serialized in scenes/prefabs/assets, so MixIn/Topping keep their original
// bits and Base is appended — do not reorder.
[System.Flags]
public enum IngredientCategory
{
    MixIn   = 1 << 0,
    Topping = 1 << 1,
    Base    = 1 << 2,
}


[CreateAssetMenu(fileName = "ING_New", menuName = "Ingredient Data")]
public class IngredientData : ScriptableObject
{
    public string ingredientName;
    public IngredientCategory validCategories;
    // Fill visual used when this ingredient is poured into a cup. Used by Base ingredients;
    // may be left unset for pure add-ons.
    public Material material;
    // Color shown for this ingredient's segment on the cup slider.
    public Color sliderColor = Color.white;
    public String unitSingular;
    public String unitPlural;
}
