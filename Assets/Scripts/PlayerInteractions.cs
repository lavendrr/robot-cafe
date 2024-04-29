using UnityEngine;

namespace StarterAssets
{
    public class PlayerInteractions : MonoBehaviour
    {
        private GameObject mainCamera;
        private GameObject grabbedObject;

        // Start is called before the first frame update
        void Start()
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        void OnGrab()
        {
            GrabCheck();
        }

        void GrabCheck()
        {
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward), out RaycastHit hit, 3f))
            {
                if (hit.collider.gameObject.CompareTag("Grabbable"))
                {
                    if (grabbedObject == null)
                    {
                        // Stores the hit object, reparents it to the camera, and turns off its physics
                        grabbedObject = hit.collider.gameObject;
                        grabbedObject.transform.SetParent(mainCamera.transform);
                        grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                    }
                    else
                    {
                        // Does the opposite
                        grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                        grabbedObject.transform.parent = null;
                        grabbedObject = null;
                    }
                }
                Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward) * hit.distance, Color.green, 3f);
                Debug.Log("Did Hit");
            }
            else
            {
                Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward) * 1000, Color.red, 3f);
                Debug.Log("Did not Hit");
            }
        }
    }
}
