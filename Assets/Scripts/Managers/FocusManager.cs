using Unity.Cinemachine;
using UnityEngine;

public class FocusManager : MonoBehaviour
{
    public static FocusManager Instance { get; private set; }

    [Tooltip("The dedicated focus virtual camera. Driven manually; the brain is Cut so MainCamera mirrors it each frame.")]
    [SerializeField] private CinemachineCamera focusVcam;

    [Tooltip("Priority applied to the focus vcam while focused (must exceed the player follow camera's priority).")]
    [SerializeField] private int activePriority = 100;
    [Tooltip("Priority applied to the focus vcam while not focused (must be below the player follow camera's priority).")]
    [SerializeField] private int inactivePriority = -100;

    [Tooltip("Seconds to tween in/out of focus.")]
    [SerializeField] private float blendDuration = 0.3f;

    // True whenever focus is active, including the in/out tweens, so the player stays frozen the whole time.
    public bool IsFocused => _state != FocusState.Idle;
    public FocusInteractable CurrentTarget { get; private set; }

    private enum FocusState { Idle, BlendingIn, Focused, BlendingOut }
    private FocusState _state = FocusState.Idle;

    // Tween endpoints (world space).
    private float _blendT;
    private Vector3 _fromPos, _toPos;
    private Quaternion _fromRot, _toRot;
    private float _fromFov, _toFov;
    // Pose to return to on exit (the player camera pose captured at entry; the player is frozen while focused).
    private Vector3 _returnPos;
    private Quaternion _returnRot;
    private float _returnFov;
    // The focus vcam's authored zoom FOV, cached before any tween overwrites the lens.
    private float _focusFov;

    // Mouse-look state, only active once the in-tween completes.
    private float _focusPitch, _focusYaw, _baseYaw;
    private const float FocusClamp = 30f;
    private const float RotationSpeed = 2.0f;

    private CinemachineBrain _brain;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (focusVcam != null)
        {
            focusVcam.Priority = inactivePriority;
            // Remember the authored zoom FOV; the lens gets overwritten each tween.
            _focusFov = focusVcam.Lens.FieldOfView;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void EnterFocus(FocusInteractable target)
    {
        if (_state != FocusState.Idle || target == null || target.focusPoint == null || focusVcam == null) return;

        if (!ResolveBrain()) return;

        CurrentTarget = target;

        // Capture the live camera pose so the takeover is seamless, and remember it for the return tween.
        _fromPos = _returnPos = _brain.transform.position;
        _fromRot = _returnRot = _brain.transform.rotation;
        _toPos = target.focusPoint.position;
        _toRot = target.focusPoint.rotation;

        // Capture the current rendered FOV so the zoom tweens from gameplay -> focus instead of snapping.
        _fromFov = _returnFov = CurrentRenderedFov();
        _toFov = _focusFov;

        // Start the focus vcam exactly where the camera is now (pose + FOV), then take over (brain Cut -> no visible jump).
        focusVcam.transform.SetPositionAndRotation(_fromPos, _fromRot);
        SetFocusVcamFov(_fromFov);
        focusVcam.Priority = activePriority;

        _blendT = 0f;
        _state = FocusState.BlendingIn;
        target.OnFocusEnter();
    }

    public void ExitFocus()
    {
        if (_state == FocusState.Idle || _state == FocusState.BlendingOut) return;

        // Tween from the current focus pose back to the stored player pose.
        _fromPos = focusVcam.transform.position;
        _fromRot = focusVcam.transform.rotation;
        _toPos = _returnPos;
        _toRot = _returnRot;

        _fromFov = focusVcam.Lens.FieldOfView;
        _toFov = _returnFov;

        _blendT = 0f;
        _state = FocusState.BlendingOut;
        CurrentTarget?.OnFocusExit();
        CurrentTarget = null;
    }

    private void LateUpdate()
    {
        switch (_state)
        {
            case FocusState.BlendingIn:
                if (StepTween())
                {
                    _state = FocusState.Focused;
                    Vector3 e = _toRot.eulerAngles;
                    _focusPitch = NormalizeAngle(e.x);
                    _baseYaw = NormalizeAngle(e.y);
                    _focusYaw = _baseYaw;
                }
                break;

            case FocusState.Focused:
                ApplyMouseLook();
                break;

            case FocusState.BlendingOut:
                if (StepTween())
                {
                    focusVcam.Priority = inactivePriority;
                    _state = FocusState.Idle;
                }
                break;
        }
    }

    // Advances the active tween one frame; returns true when complete.
    private bool StepTween()
    {
        _blendT += Time.deltaTime / Mathf.Max(0.0001f, blendDuration);
        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_blendT));
        focusVcam.transform.SetPositionAndRotation(
            Vector3.Lerp(_fromPos, _toPos, t),
            Quaternion.Slerp(_fromRot, _toRot, t));
        SetFocusVcamFov(Mathf.Lerp(_fromFov, _toFov, t));
        return _blendT >= 1f;
    }

    private float CurrentRenderedFov()
    {
        Camera cam = _brain != null ? _brain.OutputCamera : null;
        return cam != null ? cam.fieldOfView : _focusFov;
    }

    private void SetFocusVcamFov(float fov)
    {
        // LensSettings is a struct, so read-modify-write the whole property.
        LensSettings lens = focusVcam.Lens;
        lens.FieldOfView = fov;
        focusVcam.Lens = lens;
    }

    private void ApplyMouseLook()
    {
        Vector2 look = PlayerInteractions.Instance.look;
        if (look.sqrMagnitude < 0.0001f) return;

        _focusPitch -= look.y * RotationSpeed;
        _focusYaw += look.x * RotationSpeed;

        _focusPitch = Mathf.Clamp(_focusPitch, -FocusClamp, FocusClamp);
        _focusYaw = Mathf.Clamp(_focusYaw, _baseYaw - FocusClamp, _baseYaw + FocusClamp);

        focusVcam.transform.rotation = Quaternion.Euler(_focusPitch, _focusYaw, 0f);
    }

    private bool ResolveBrain()
    {
        if (_brain == null) _brain = Camera.main != null ? Camera.main.GetComponent<CinemachineBrain>() : null;
        if (_brain == null) _brain = FindObjectOfType<CinemachineBrain>();
        return _brain != null;
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}
