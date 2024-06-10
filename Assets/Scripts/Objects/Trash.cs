using UnityEngine;

public class Trash : MonoBehaviour
{
    public void TrashCup(GameObject cup)
    {
        if (cup != null)
        {
            cup.GetComponent<Cup>().Empty();
            Debug.Log("Trashed cup contents");
        }
    }
}
