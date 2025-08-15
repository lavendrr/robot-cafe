using UnityEngine;

public class CupDispenser : MonoBehaviour
{
    private Collider[] blockers = new Collider[2];
    [SerializeField]
    private GameObject cupPrefab;
    [SerializeField]
    private GameObject spawnPoint;
    [SerializeField]
    private BoxCollider spawnCollider;

    private void Start()
    {
        if (cupPrefab != null && spawnPoint != null)
        {
            SpawnCup();
        }
    }

    public void SpawnCup()
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

        if (blocked)
        {
            Debug.LogWarning("Cup dispenser blocked by: " + blockingCollider.name);
            return;
        }

        Instantiate(cupPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation, spawnPoint.transform);
    }

    void OnUse()
    {
        SpawnCup();
    }

}
