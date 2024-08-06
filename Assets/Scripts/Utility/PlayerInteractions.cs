using UnityEngine;
using UnityEngine.InputSystem;

public enum InteractableType
{
    None,
    Grabbable,
    Slottable,
    Usable
}

public class PlayerInteractions : MonoBehaviour
{
    public static PlayerInteractions Instance { get; private set; }

    private PlayerInput playerInput;
    private InputAction moveAction;

    private GameObject mainCamera;
    private GameObject grabbedObject;
    private bool moveInput;

    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool grab;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;


    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["move"];
    }

    void Update()
    {
        moveAction.started += ctx => moveInput = true;
        moveAction.canceled += ctx => moveInput = false;
    }

    public bool GetMoveInputState()
    {
        return moveInput;
    }

    public (InteractableType type, GameObject obj) InteractionCheck()
    {
        // Debug ray that matches the raycast for distance testing
        Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward) * 3);
        // Sends a ray 3 meters out from the camera & returns the first interactable object's type and reference. Uses an inverse layermask to ignore anything on the Ignore Raycast layer
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward), out RaycastHit hit, 3f, ~(1 << LayerMask.NameToLayer("Ignore Raycast"))))
        {
            if (hit.collider.gameObject.CompareTag("Grabbable"))
            {
                return (InteractableType.Grabbable, hit.collider.gameObject);
            }
            else if (hit.collider.gameObject.CompareTag("Slottable"))
            {
                return (InteractableType.Slottable, hit.collider.gameObject);
            }
            else if (hit.collider.gameObject.CompareTag("Usable"))
            {
                return (InteractableType.Usable, hit.collider.gameObject);
            }
            else
            {
                return (InteractableType.None, hit.collider.gameObject);
            }
        }
        return (InteractableType.None, null);
    }

    void OnGrab()
    {
        GrabAttempt();
    }

    void GrabAttempt()
    {
        var (type, obj) = InteractionCheck();
        if (grabbedObject == null)
        {
            if (type == InteractableType.Grabbable)
            {
                // Grabs the held object, disables its physics, and removes it from the slot if it is currently in one
                grabbedObject = obj;
                AudioManager.Instance.PlaySFX(AudioManager.Instance.pickUp, grabbedObject.transform.position);
                if (grabbedObject.transform.parent != null && grabbedObject.transform.parent.gameObject.name == "SlotRoot")
                {
                    grabbedObject.transform.parent.gameObject.GetComponent<Slot>().RemoveObj();
                }
                grabbedObject.transform.SetParent(mainCamera.transform);
                grabbedObject.transform.SetLocalPositionAndRotation(Vector3.zero + new Vector3(0.5f, -0.5f, 0.75f), Quaternion.identity);
                grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                grabbedObject.GetComponent<BoxCollider>().enabled = false;
                grabbedObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }
        else
        {
            if (type == InteractableType.None)
            {
                // Drops the held object and re-enables physics
                grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                grabbedObject.GetComponent<BoxCollider>().enabled = true;
                grabbedObject.transform.parent = null;
                grabbedObject.layer = LayerMask.NameToLayer("Default");
                grabbedObject = null;
            }
            else if (type == InteractableType.Slottable)
            {
                // Slots the object in
                obj.transform.Find("SlotRoot").GetComponent<Slot>().InsertObj(grabbedObject);
                grabbedObject = null;
            }
        }
    }

    public bool GetGrabStatus()
    {
        return grabbedObject != null;
    }

    void OnPause()
    {
        StateManager.Instance.SetGamePaused(true);
    }

    void OnUnpause()
    {
        StateManager.Instance.SetGamePaused(false);
    }

    void OnUse()
    {
        UseAttempt();
    }

    void UseAttempt()
    {
        var (type, obj) = InteractionCheck();
        if (type == InteractableType.Usable)
        {
            // Requires a receiver on the target GameObject
            obj.SendMessage("OnUse", grabbedObject);
        }
    }

    public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnGrab(InputValue value)
		{
			GrabInput(value.isPressed);
		}

        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void GrabInput(bool newGrabState)
		{
			grab = newGrabState;
		}
		
		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
}
