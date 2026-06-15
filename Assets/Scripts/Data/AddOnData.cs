using UnityEngine;
// [Flags] allows combining values with bitwise OR (e.g. MixIn | Topping).
// Each entry must be the next power of 2 (1 << N). To add a category, append a new entry with the next N.
[System.Flags]
public enum AddOnCategory
{
    MixIn  = 1 << 0,
    Topping = 1 << 1,
}


[CreateAssetMenu(fileName = "AOD_New", menuName = "Add-On Data")]
public class AddOnData : ScriptableObject
{
    public string addOnName;
    public AddOnCategory validCategories;
}
