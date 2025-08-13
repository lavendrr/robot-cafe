using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class CafeLayoutManager : MonoBehaviour
{
    public static CafeLayoutManager Instance { get; private set; }

    private bool _prevTestInEditor = false;

    [SerializeField]
    public GameObject CafeRoot;
    public float GridCellSize = 1.0f;
    public bool TestInEditor = false;

    public List<CafeElement> TestLayout = new List<CafeElement>();

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private Vector3 ConvertGridToWorldPosition(int x, int y)
    {
        if (CafeRoot == null)
            return Vector3.zero;

        Vector3 rootPos = CafeRoot.transform.position;
        float worldX = rootPos.x + x * GridCellSize;
        float worldY = rootPos.y;
        float worldZ = rootPos.z + y * GridCellSize;
        return new Vector3(worldX, worldY, worldZ);
    }

    public void PopulateCafeLevel()
    {
        #if UNITY_EDITOR
        List<CafeElement> cafeLayout = TestLayout;
        #else
        List<CafeElement> cafeLayout = SaveManager.Instance.GetCafeLayout();
        #endif
        // Iterate through the saved layout and instantiate the furniture objects
        foreach (var element in cafeLayout)
        {
            if (element.furnitureObject == null)
            {
                continue;
            }
            GameObject furniturePrefab = element.furnitureObject.prefab;
            if (furniturePrefab == null)
            {
                Debug.LogWarning($"Furniture prefab for {element.furnitureObject.furnitureName} not found.");
                continue;
            }

            Vector3 spawnPosition = ConvertGridToWorldPosition(element.rootGridCoord.x, element.rootGridCoord.y);
            GameObject furnitureInstance = Instantiate(furniturePrefab, spawnPosition, Quaternion.Euler(0, element.rotation, 0));
            furnitureInstance.name = element.furnitureObject.furnitureName;
            if (CafeRoot != null)
            {
                furnitureInstance.transform.SetParent(CafeRoot.transform);
            }
        }
    }

    private void DestroyCafeFurniture()
    {
        if (CafeRoot == null)
            return;

        for (int i = CafeRoot.transform.childCount - 1; i >= 0; i--)
        {
            #if UNITY_EDITOR
            DestroyImmediate(CafeRoot.transform.GetChild(i).gameObject);
            #else
            Destroy(CafeRoot.transform.GetChild(i).gameObject);
            #endif
        }
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        // If TestInEditor was just set to false, destroy all children
        if (_prevTestInEditor && !TestInEditor && CafeRoot != null)
        {
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying && this != null && CafeRoot != null)
                {
                    DestroyCafeFurniture();
                }
            };
        } else if (!_prevTestInEditor && TestInEditor && CafeRoot != null)
        {
            // If TestInEditor was just set to true, populate the cafe level
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying && this != null && CafeRoot != null)
                {
                    DestroyCafeFurniture(); // CafeRoot shouldn't have children, but run just in case
                    PopulateCafeLevel();
                }
            };
        }

        if (TestInEditor && TestLayout != null)
        {
            // Defer execution to avoid Unity's internal restrictions
            EditorApplication.delayCall += () =>
            {
                // Only run if not in play mode and this object still exists
                if (!Application.isPlaying && this != null && CafeRoot != null)
                {
                    DestroyCafeFurniture();
                    PopulateCafeLevel();
                }
            };
        }

        _prevTestInEditor = TestInEditor;
#endif
    }
}