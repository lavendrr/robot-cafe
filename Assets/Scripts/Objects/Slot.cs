using UnityEngine;

public class Slot : MonoBehaviour
{
    private GameObject slottedObj;

    public void InsertObj(GameObject obj)
    {
        // Slots the object inside by reparenting it, re-enables its collider, makes it interactable again, and sends a message upwards in case the slottable object has special behavior
        slottedObj = obj;
        slottedObj.GetComponent<BoxCollider>().enabled = true;
        slottedObj.transform.SetParent(gameObject.transform);
        slottedObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        SendMessageUpwards("OnSlotInsert", options: SendMessageOptions.DontRequireReceiver);
        slottedObj.layer = LayerMask.NameToLayer("Interactable");
    }

    public void RemoveObj()
    {
        slottedObj = null;
        SendMessageUpwards("OnSlotRemove", options: SendMessageOptions.DontRequireReceiver);
    }

    public GameObject GetSlottedObj()
    {
        return slottedObj;
    }
}
