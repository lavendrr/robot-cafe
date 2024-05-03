using UnityEngine;

namespace StarterAssets
{
    public class PlayerInteractions : MonoBehaviour
    {
        private GameObject mainCamera;
        private GameObject grabbedObject;
        private GameObject grabUI;
        private GameObject slotUI;

        enum InteractableType
        {
            None,
            Grabbable,
            Slottable
        }

        // Start is called before the first frame update
        void Start()
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            grabUI = GameObject.Find("GrabUI");
            grabUI.SetActive(false);
            slotUI = GameObject.Find("SlotUI");
            slotUI.SetActive(false);
        }

        void Update()
        {
            var (type, obj) = InteractionCheck();
            if (type == InteractableType.Grabbable)
            {
                grabUI.SetActive(true);
                slotUI.SetActive(false);
            }
            else if (type == InteractableType.Slottable && grabbedObject != null)
            {
                grabUI.SetActive(false);
                slotUI.SetActive(true);
            }
            else
            {
                grabUI.SetActive(false);
                slotUI.SetActive(false);
            }
        }

        (InteractableType type, GameObject obj) InteractionCheck()
        {
            // Sends a ray 3 meters out from the camera & returns the first interactable object's type and reference.
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward), out RaycastHit hit, 3f, LayerMask.GetMask("Interactable")))
            {
                // Checks if the player isn't already holding something as well
                if (hit.collider.gameObject.CompareTag("Grabbable"))
                {
                    return (InteractableType.Grabbable, hit.collider.gameObject);
                }
                else if (hit.collider.gameObject.CompareTag("Slottable"))
                {
                    return (InteractableType.Slottable, hit.collider.gameObject);
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
                    // Grabs the held object
                    grabbedObject = obj;
                    if (grabbedObject.transform.parent != null && grabbedObject.transform.parent.gameObject.name == "SlotRoot")
                    {
                        grabbedObject.transform.parent.gameObject.GetComponent<Slot>().RemoveObj();
                    }
                    grabbedObject.transform.SetParent(mainCamera.transform);
                    grabbedObject.transform.SetLocalPositionAndRotation(Vector3.zero + new Vector3(0.5f, -0.5f, 1.1f), Quaternion.identity);
                    grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                    grabbedObject.GetComponent<BoxCollider>().enabled = false;
                    grabbedObject.layer = LayerMask.NameToLayer("Grabbed");
                }
            }
            else
            {
                if (type == InteractableType.None)
                {
                    // Drops the held object
                    grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                    grabbedObject.GetComponent<BoxCollider>().enabled = true;
                    grabbedObject.transform.parent = null;
                    grabbedObject.layer = LayerMask.NameToLayer("Interactable");
                    grabbedObject = null;
                }
                else if (type == InteractableType.Slottable)
                {
                    // Reparents the grabbed object to the slottable object, resets its transform, and removes it from the Grabbed layer
                    obj.transform.Find("SlotRoot").GetComponent<Slot>().InsertObj(grabbedObject);
                    grabbedObject = null;
                }
            }
        }

        void OnUse()
        {
            UseAttempt();
        }

        void UseAttempt()
        {
            var (type, obj) = InteractionCheck();
            if (obj.name == "CoffeeMachine")
            {
                obj.GetComponent<CoffeeMachine>().FillCup();
            }
        }
    }
}
