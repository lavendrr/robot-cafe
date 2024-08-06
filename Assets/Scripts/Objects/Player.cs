using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]

public class Player : MonoBehaviour
{
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    private float MoveSpeed = 6.0f;
    private float RotationSpeed = 2.0f;
    private float SpeedChangeRate = 10.0f; // acceleration and decceleration
    private float TopCameraClamp = 60.0f;
    private float BottomCameraClamp = -60.0f;

    // cinemachine
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private PlayerInput _playerInput;
    private CharacterController _controller;
    private PlayerInteractions _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool IsCurrentDeviceMouse
    {
        get
        {
            #if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
            #else
            return false;
            #endif
        }
    }
    private Vector3 SpawnPosition;
    private Quaternion SpawnRotation;

    void Awake()
    {
        StateManager.Instance.RegisterPlayer(this);
        // Get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        // Subscribe to the state change event
        StateManager.Instance.OnStateChanged += HandleStateChange;
    }

    void Start()
    {
        SpawnPosition = transform.position;
        SpawnRotation = transform.rotation;
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInteractions>();
        _playerInput = GetComponent<PlayerInput>();
    }

    public void Respawn()
    {
        if (SpawnPosition != null)
        {
            TeleportTo(SpawnPosition, SpawnRotation.eulerAngles);
        }
    }

    public void TeleportTo(Vector3 newPosition, Vector3 newRotation)
    {
        transform.SetPositionAndRotation(newPosition, Quaternion.Euler(newRotation));
    }

    private void Update()
    {
        Move();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void HandleStateChange(State newState)
    {
        if (newState.GetType() == typeof(ShiftEndState))
        {
            // Disable player input
            _playerInput.DeactivateInput();
        }
    }

    private void CameraRotation()
    {
        // if there is an input
        if (_input.look.sqrMagnitude >= _threshold)
        {
            //Don't multiply mouse input by Time.deltaTime
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
            
            _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
            _rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

            // clamp our pitch rotation
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomCameraClamp, TopCameraClamp);

            // Update Cinemachine camera target pitch
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            // rotate the player left and right
            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    private void Move()
    {
        // set target speed to maximum move speed
        float targetSpeed = MoveSpeed;

        // if there is no input, set the target speed to 0
        if (_input.move == Vector2.zero)
        {
            targetSpeed = 0.0f;
        }
        
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f; // acceptable margin of error where the speed will snap to MoveSpeed
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f; // take magnitude into account for joysticks

        // smoothly accelerate or decelerate to target speed if we're not there yet
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        // normalize input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            // move
            inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
        }

        // move the player
        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}