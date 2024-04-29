using UnityEngine;

namespace StarterAssets
{
    public class PlayerInteractions : MonoBehaviour
    {
        private GameObject mainCamera;
        private GameObject grabbedObject;
        private GameObject grabUI;

        // Start is called before the first frame update
        void Start()
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            grabUI = GameObject.Find("GrabUI");
            grabUI.SetActive(false);
        }

        void Update()
        {
            if (GrabCheck() != null)
            {
                grabUI.SetActive(true);
            }
            else
            {
                grabUI.SetActive(false);
            }
        }

        GameObject GrabCheck()
        {
            // Sends a ray 3 meters out from the camera & returns a valid grabbable object if possible.
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward), out RaycastHit hit, 3f, LayerMask.GetMask("Interactable")))
            {
                if (hit.collider.gameObject.CompareTag("Grabbable") && hit.collider.gameObject != grabbedObject)
                {
                    return hit.collider.gameObject;
                }
            }
            return null;
        }

        void OnGrab()
        {
            GrabAttempt();
        }

        void GrabAttempt()
        {
            var checkedObj = GrabCheck();
            if (checkedObj != null)
            {
                // Stores the hit object, reparents it to the camera, and turns off its physics
                grabbedObject = checkedObj;
                grabbedObject.transform.SetParent(mainCamera.transform);
                grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                Debug.Log(LayerMask.GetMask("Grabbed"));
                grabbedObject.layer = LayerMask.NameToLayer("Grabbed");
            }
            else if (grabbedObject != null)
            {
                grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                grabbedObject.transform.parent = null;
                grabbedObject.layer = LayerMask.NameToLayer("Interactable");
                grabbedObject = null;
            }
        }
    }
}
