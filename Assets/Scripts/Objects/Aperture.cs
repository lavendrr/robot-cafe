using UnityEngine;

[ExecuteAlways]
public class Aperture : MonoBehaviour
{
    [Range(0f, 1f)] public float aperture = 0f;
    public float maxAngle = 45f;
    public Transform[] bladePivots;

    [SerializeField, HideInInspector] Quaternion[] restRotations;

    [ContextMenu("Find blade pivots")]
    void FindPivots()
    {
        var list = new System.Collections.Generic.List<Transform>();
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name.StartsWith("blade_pivot_")) list.Add(t);
        bladePivots = list.ToArray();
        CaptureRest();
    }

    [ContextMenu("Capture rest pose (set aperture to 0 first)")]
    void CaptureRest()
    {
        aperture = 0f;
        restRotations = new Quaternion[bladePivots.Length];
        for (int i = 0; i < bladePivots.Length; i++)
            restRotations[i] = bladePivots[i].localRotation;
    }

    void Update()
    {
        if (bladePivots == null || restRotations == null ||
            restRotations.Length != bladePivots.Length) return;

        Quaternion turn = Quaternion.AngleAxis(aperture * maxAngle, Vector3.up);
        for (int i = 0; i < bladePivots.Length; i++)
            if (bladePivots[i]) bladePivots[i].localRotation = restRotations[i] * turn;
    }
}