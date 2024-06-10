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
        public static PlayerInteractions instance { get; private set; }

        private PlayerInput playerInput;
        private InputAction moveAction;
        private AudioManager Audio;

        private GameObject mainCamera;
        private GameObject grabbedObject;
        private bool moveInput;


        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself
            if (instance != null && instance != this)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Audio = AudioManager.instance;
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
            // Sends a ray 3 meters out from the camera & returns the first interactable object's type and reference.
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward), out RaycastHit hit, 3f, LayerMask.GetMask("Interactable")))
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
                    Audio.PlaySFX(Audio.pickUp, grabbedObject.transform.position);
                    if (grabbedObject.transform.parent != null && grabbedObject.transform.parent.gameObject.name == "SlotRoot")
                    {
                        grabbedObject.transform.parent.gameObject.GetComponent<Slot>().RemoveObj();
                    }
                    grabbedObject.transform.SetParent(mainCamera.transform);
                    grabbedObject.transform.SetLocalPositionAndRotation(Vector3.zero + new Vector3(0.5f, -0.5f, 0.75f), Quaternion.identity);
                    grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                    grabbedObject.GetComponent<BoxCollider>().enabled = false;
                    grabbedObject.layer = LayerMask.NameToLayer("Grabbed");
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
                    grabbedObject.layer = LayerMask.NameToLayer("Interactable");
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

        void OnUse()
        {
            UseAttempt();
        }

        void UseAttempt()
        {
            var (type, obj) = InteractionCheck();
            if (type == InteractableType.Usable)
            {
                if (obj.name == "CoffeeMachine")
                {
                    obj.GetComponent<CoffeeMachine>().FillCup();
                }
                else if (obj.name == "Bell")
                {
                    obj.GetComponent<Delivery>().Deliver();
                }
                else if (obj.name == "Trash")
                {
                    obj.GetComponent<Trash>().TrashCup(grabbedObject);
                }
            }
        }
    }
