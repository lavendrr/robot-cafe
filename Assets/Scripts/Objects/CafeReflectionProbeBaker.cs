using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// Renders a Realtime reflection probe exactly once, after the runtime-spawned cafe
/// geometry exists, then leaves it static
[RequireComponent(typeof(ReflectionProbe))]
public class CafeReflectionProbeBaker : MonoBehaviour
{
    [Tooltip("Safety cap on how many frames to wait for cafe geometry before rendering anyway.")]
    [SerializeField] private int _maxWaitFrames = 300;

    private ReflectionProbe _probe;

    private void Awake()
    {
        EnsureProbe();
        WarnIfMisconfigured();
    }

    // Awake only runs in play mode, so edit-mode callers (e.g. the CafeLayoutManager inspector
    // button) can't rely on _probe being set. Resolve it lazily.
    private ReflectionProbe EnsureProbe()
    {
        if (_probe == null)
        {
            _probe = GetComponent<ReflectionProbe>();
        }
        return _probe;
    }

    private void WarnIfMisconfigured()
    {
        if (_probe.mode != ReflectionProbeMode.Realtime)
        {
            Debug.LogWarning(
                $"[{nameof(CafeReflectionProbeBaker)}] Probe '{name}' mode is {_probe.mode}; " +
                $"expected {ReflectionProbeMode.Realtime}. The one-shot script bake only drives Realtime probes.",
                this);
        }

        if (_probe.refreshMode != ReflectionProbeRefreshMode.ViaScripting)
        {
            Debug.LogWarning(
                $"[{nameof(CafeReflectionProbeBaker)}] Probe '{name}' refresh mode is {_probe.refreshMode}; " +
                $"expected {ReflectionProbeRefreshMode.ViaScripting}, or the probe may also refresh outside this baker.",
                this);
        }

        if (_probe.timeSlicingMode != ReflectionProbeTimeSlicingMode.NoTimeSlicing)
        {
            Debug.LogWarning(
                $"[{nameof(CafeReflectionProbeBaker)}] Probe '{name}' time slicing is {_probe.timeSlicingMode}; " +
                $"expected {ReflectionProbeTimeSlicingMode.NoTimeSlicing} so the capture completes in one pass.",
                this);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(BakeWhenReady());
    }

    private IEnumerator BakeWhenReady()
    {
        // Wait until the cafe has been populated (CafeRoot has children). Bail out of waiting
        // after a frame cap so a layout that never populates still produces some capture.
        int frames = 0;
        while (!IsCafePopulated() && frames < _maxWaitFrames)
        {
            frames++;
            yield return null;
        }

        // One more full frame so every spawned renderer/material is enabled and submitted,
        // otherwise the capture can miss freshly-instantiated neon emissives.
        yield return new WaitForEndOfFrame();

        int renderId = EnsureProbe().RenderProbe();

        // Hold until the GPU finishes the one-shot render before we consider the probe baked.
        while (!_probe.IsFinishedRendering(renderId))
        {
            yield return null;
        }
    }

    /// Renders the probe once, immediately, without waiting for geometry.
    /// Returns the render id, or -1 if no probe.
    [ContextMenu("Bake Now")]
    public int BakeImmediate()
    {
        ReflectionProbe probe = EnsureProbe();
        if (probe == null)
        {
            return -1;
        }

        WarnIfMisconfigured();
        return probe.RenderProbe();
    }

    private static bool IsCafePopulated()
    {
        CafeLayoutManager layout = CafeLayoutManager.Instance;
        return layout != null
            && layout.CafeRoot != null
            && layout.CafeRoot.transform.childCount > 0;
    }

    /// Re-render the probe on demand (e.g. after a mid-shift layout change). Public so other
    /// systems can trigger a fresh capture without reconfiguring the probe.
    public void Rebake()
    {
        StopAllCoroutines();
        StartCoroutine(BakeWhenReady());
    }
}
