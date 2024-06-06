using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    void Start()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Audio");

        if (objs.Length > 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}
