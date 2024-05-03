using UnityEngine;

public class Slot : MonoBehaviour
{
    private GameObject slottedObj;

    public void InsertObj(GameObject obj)
    {
        slottedObj = obj;
        slottedObj.GetComponent<BoxCollider>().enabled = true;
        slottedObj.transform.SetParent(gameObject.transform);
        slottedObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        slottedObj.layer = LayerMask.NameToLayer("Interactable");
    }

    public void RemoveObj()
    {
        slottedObj = null;
    }

    public GameObject GetSlottedObj()
    {
        return slottedObj;
    }
}
