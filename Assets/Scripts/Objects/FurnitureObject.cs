using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GridCoord
{
    public int x;
    public int y;
}

[CreateAssetMenu(fileName = "FO_New", menuName = "Furniture Object")]
public class FurnitureObject : ScriptableObject
{
    public string furnitureName;
    public GameObject prefab;
    public int cost;
    public List<GridCoord> gridOffsets;
}
