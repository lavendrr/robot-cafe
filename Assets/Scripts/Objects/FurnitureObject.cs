using System;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[System.Serializable]
public struct GridCoord
{
    public int col;
    public int row;
}

[CreateAssetMenu(fileName = "FO_New", menuName = "Furniture Object")]
public class FurnitureObject : ScriptableObject
{
    public string furnitureName;
    public GameObject prefab;
    public int cost;
    public List<GridCoord> gridOffsets;
    public Sprite catalogSprite;
    public List<Sprite> gridSprites;
    public string tooltipText;
    public Boolean isSellable;
}
