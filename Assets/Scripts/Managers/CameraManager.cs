using UnityEngine;

public enum SceneCamera
{
    MainCamera,
    OverheadCamera
}

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public Camera mainCamera, overheadCamera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        // Ensure both cameras are initially enabled correctly
        mainCamera.enabled = true;
        overheadCamera.enabled = false;
    }

    public void SetActiveCamera(SceneCamera newCamera)
    {
        if (newCamera == SceneCamera.MainCamera)
        {
            mainCamera.enabled = true;
            overheadCamera.enabled = false;
        }
        else if (newCamera == SceneCamera.OverheadCamera)
        {
            mainCamera.enabled = false;
            overheadCamera.enabled = true;
        } else {
            Debug.LogWarning("CameraManager;; " + newCamera.ToString() + " is not a valid camera.");
        }
    }
}