using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GridCoord
{
    public int col;
    public int row;
}

[CreateAssetMenu(fileName = "FD_New", menuName = "Furniture Data")]
public class FurnitureData : ScriptableObject
{
    public string furnitureName;
    public GameObject prefab;
    public int cost;
    public List<GridCoord> gridOffsets;
    public Sprite catalogSprite;
    public List<Sprite> gridSprites;
    public string tooltipText;
    public Boolean isSellable;
    public List<FurnitureArea> validAreas;
}
