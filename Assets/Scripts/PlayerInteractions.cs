using Cinemachine.Utility;
using Unity.VisualScripting;
using UnityEngine;

namespace StarterAssets
{
    public class PlayerInteractions : MonoBehaviour
    {
        private GameObject mainCamera;
        private GameObject grabbedObject;
        private GameObject grabUI;

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
        }

        void Update()
        {
            var (type, obj) = InteractionCheck();
            if (type == InteractableType.Grabbable)
            {
                grabUI.SetActive(true);
            }
            else
            {
                grabUI.SetActive(false);
            }
        }

        (InteractableType type, GameObject obj) InteractionCheck()
        {
            // Sends a ray 3 meters out from the camera & returns the first interactable object's type and reference.
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward), out RaycastHit hit, 3f, LayerMask.GetMask("Interactable")))
            {
                // Checks if the player isn't already holding something as well
                if (hit.collider.gameObject.CompareTag("Grabbable") && hit.collider.gameObject != grabbedObject)
                {
                    return (InteractableType.Grabbable, hit.collider.gameObject);
                }
                else if (hit.collider.gameObject.CompareTag("Slottable") && grabbedObject != null)
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
            if (type == InteractableType.Grabbable)
            {
                // Grabs the held object
                grabbedObject = obj;
                grabbedObject.transform.SetParent(mainCamera.transform);
                grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                grabbedObject.layer = LayerMask.NameToLayer("Grabbed");
            }
            else if (grabbedObject != null)
            {
                if (type == InteractableType.None)
                {
                    // Drops the held object
                    grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                    grabbedObject.transform.parent = null;
                    grabbedObject.layer = LayerMask.NameToLayer("Interactable");
                    grabbedObject = null;
                }
                else if (type == InteractableType.Slottable)
                {
                    // Reparents the grabbed object to the slottable object, resets its transform, and removes it from the Grabbed layer
                    grabbedObject.transform.SetParent(obj.transform.Find("SlotRoot").transform);
                    grabbedObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    grabbedObject.layer = LayerMask.NameToLayer("Interactable");
                    grabbedObject = null;
                }
            }
        }
    }
}
