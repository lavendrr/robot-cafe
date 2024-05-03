using UnityEngine;

public class Cup : MonoBehaviour
{
    private bool filled = false;

    public bool GetFillState()
    {
        return filled;
    }

    public void Fill()
    {
        if (filled == false)
        {
            filled = true;
            gameObject.GetComponentsInChildren<MeshRenderer>()[1].enabled = true;
            Debug.Log("Cup filled.");
        }
        else
        {
            Debug.Log("Cup already full.");
        }
    }
}
