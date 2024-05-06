using Orders;
using UnityEngine;
using System.Collections;

public class Delivery : MonoBehaviour
{
    public void Deliver()
    {
        Debug.Log("Trying to deliver");
        var cupObj = gameObject.transform.parent.gameObject.GetComponentInChildren<Slot>().GetSlottedObj();
        if (cupObj != null)
        {
            if(OrderManager.instance.FillOrder(cupObj, transform.position))
            {
                StartCoroutine(RingBell());
            }
        }
    }

    private IEnumerator RingBell()
    {
        Transform ringerTransform = transform.Find("SM_bell").Find("ringer");
        if (ringerTransform == null)
        {
            Debug.LogError("Ringer transform not found.");
            yield break;
        }

        Vector3 initialPosition = ringerTransform.localPosition;
        Vector3 targetPosition = initialPosition - Vector3.up * 0.016413f;
        float duration = 0.1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            ringerTransform.localPosition = Vector3.Lerp(initialPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ringerTransform.localPosition = targetPosition;

        // Move back up
        duration = 0.05f;
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            ringerTransform.localPosition = Vector3.Lerp(targetPosition, initialPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ringerTransform.localPosition = initialPosition;
    }
}
