using TMPro;
using UnityEngine;
using System.Collections;
using System;

public class CupDispenser : MonoBehaviour
{
    private Collider[] blockers = new Collider[4];
    [SerializeField]
    private GameObject cupPrefab;
    [SerializeField]
    private Transform spawnPoint, apexPoint;
    [SerializeField]
    private BoxCollider spawnCollider;
    [SerializeField]
    private TextMeshPro statText;
    [SerializeField]
    private GameObject useButton;
    public Animator animator;
    public AnimationCurve riseCurve = new AnimationCurve(
    new Keyframe(0, 0, 0, 2), new Keyframe(1, 1, 0, 0));
    [Range(0f, 1f)] public float closeAtProgress = 0.5f;
    const float riseTime = 0.4f;
    const string UsableTag = "Usable";
    const string BusyTag = "Untagged"; //native tag, always exists

    private int cupsDispensed;
    const string CUPS_DISPENSED_TEXT = "Cups Dispensed:<br>";

    private bool referencesValid;

    private void Awake()
    {
        referencesValid = ValidateReferences();
    }

    // Logs a clear error for any serialized reference the dispenser needs but
    // that was left unassigned, so misconfiguration shows up at scene load
    // instead of as a bare NullReferenceException later. Returns false if any
    // required reference is missing.
    private bool ValidateReferences()
    {
        bool valid = true;

        if (cupPrefab == null)
        {
            Debug.LogError($"{nameof(CupDispenser)}: {nameof(cupPrefab)} is not assigned.", this);
            valid = false;
        }
        else if (cupPrefab.GetComponent<Rigidbody>() == null)
        {
            Debug.LogError($"{nameof(CupDispenser)}: {nameof(cupPrefab)} root has no Rigidbody; cups cannot be launched.", this);
            valid = false;
        }
        if (spawnPoint == null)
        {
            Debug.LogError($"{nameof(CupDispenser)}: {nameof(spawnPoint)} is not assigned.", this);
            valid = false;
        }
        if (apexPoint == null)
        {
            Debug.LogError($"{nameof(CupDispenser)}: {nameof(apexPoint)} is not assigned.", this);
            valid = false;
        }
        if (spawnCollider == null)
        {
            Debug.LogError($"{nameof(CupDispenser)}: {nameof(spawnCollider)} is not assigned.", this);
            valid = false;
        }
        if (statText == null)
        {
            Debug.LogError($"{nameof(CupDispenser)}: {nameof(statText)} is not assigned.", this);
            valid = false;
        }
        if (useButton == null)
        {
            Debug.LogError($"{nameof(CupDispenser)}: {nameof(useButton)} is not assigned.", this);
            valid = false;
        }
        if (animator == null)
        {
            Debug.LogError($"{nameof(CupDispenser)}: {nameof(animator)} is not assigned.", this);
            valid = false;
        }

        return valid;
    }

    private void Start()
    {
        if (!referencesValid) return;

        cupsDispensed = 0;
        statText.text = CUPS_DISPENSED_TEXT + cupsDispensed.ToString();
        Instantiate(cupPrefab, apexPoint.position, apexPoint.rotation);
    }

    private Boolean DispenserBlocked()
    {
        // Check if there is anything blocking our spawn collider
        int blockerCount = Physics.OverlapBoxNonAlloc(
            spawnCollider.bounds.center,
            spawnCollider.bounds.extents,
            blockers,
            spawnCollider.transform.rotation
        );

        bool blocked = false;
        Collider blockingCollider = null;
        for (int i = 0; i < blockerCount; i++)
        {
            if (blockers[i] != spawnCollider)
            {
                blocked = true;
                blockingCollider = blockers[i];
                break;
            }
        }

        return blocked;
    }

    public IEnumerator LaunchCup()
    {
        if (!referencesValid) yield break;

        Rigidbody cup = Instantiate(cupPrefab, spawnPoint.position, spawnPoint.rotation).GetComponent<Rigidbody>();
        cup.isKinematic = true;
        Collider[] cupColliders = cup.GetComponentsInChildren<Collider>();
        foreach (var c in cupColliders) c.enabled = false;
        cupsDispensed++;
        statText.text = CUPS_DISPENSED_TEXT + cupsDispensed.ToString();

        Vector3 start = spawnPoint.position;
        Vector3 end = apexPoint.position;
        bool closing = false;

        for (float t = 0f; t < 1f; )
        {
            yield return new WaitForFixedUpdate();
            t = Mathf.Min(1f, t + Time.fixedDeltaTime / riseTime);
            cup.MovePosition(Vector3.LerpUnclamped(start, end, riseCurve.Evaluate(t)));

            if (!closing && t >= closeAtProgress)
            {
                animator.SetTrigger("EndDispense");
                closing = true;
            }
        }

        foreach (var c in cupColliders) c.enabled = true;
        cup.isKinematic = false;
        cup.linearVelocity = Vector3.zero;
    }

    void OnUse()
    {
        if (DispenserBlocked())
        {
            NullPress();
            return;
        }
        useButton.tag = BusyTag;
        animator.SetTrigger("StartDispense");
    }

    private void NullPress()
    {
        useButton.tag = BusyTag;
        animator.SetTrigger("NullPress");
    }

    /* ANIMATION EVENTS */
    public void OnFullyOpen()
    {
        StartCoroutine(LaunchCup());
    }

    public void OnDispenseFinished()
    {
        useButton.tag = UsableTag;
    }

    public void OnNullPressComplete()
    {
        useButton.tag = UsableTag;
    }

}
