using UnityEngine;

public class Trash : MonoBehaviour
{
    public void OnUse(GameObject cup)
    {
        if (cup != null)
        {
            if (cup.GetComponent<Cup>().Empty())
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.trash, transform.position);
            }
        }
    }
}
