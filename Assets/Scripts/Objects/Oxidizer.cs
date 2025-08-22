using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oxidizer : MonoBehaviour
{
    private Slot slot;
    private BoxCollider slotCollider;

    private void Start()
    {
        slot = transform.parent.GetComponentInChildren<Slot>();
        slotCollider = transform.parent.Find("Slot").GetComponent<BoxCollider>();
    }

    private void OnSlotInsert()
    {
        slotCollider.enabled = false;
    }

    private void OnSlotRemove()
    {
        slotCollider.enabled = true;
    }

    public void OnUse()
    {
        // Attempt to oxidize the drink if there is fuel in the cup
        var cupObj = slot.GetSlottedObj();
        if (cupObj != null)
        {
            cupObj.GetComponent<Cup>().Oxidize();
        }
    }
}
