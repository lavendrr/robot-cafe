using UnityEngine;

public class Player : MonoBehaviour
{
    private Vector3 SpawnPosition;
    private Quaternion SpawnRotation;

    void Awake()
    {
        StateManager.Instance.RegisterPlayer(this);
        // Subscribe to the state change event
        //StateManager.Instance.OnStateChanged += HandleStateChange;
    }

    void Start()
    {
        SpawnPosition = transform.position;
        SpawnRotation = transform.rotation;
        Debug.Log(SpawnPosition);
    }

    // TODO: This function is superfluous since we reinstantiate the Shift scene on shift start
    // This could instead be repurposed to spawn the player at the correct position according
    // to their cafe layout.
    // private void HandleStateChange(State newState)
    // {
    //     if (newState.GetType() == typeof(ShiftState))
    //     {
    //         Respawn();
    //     }
    // }

    public void Respawn()
    {
        if (SpawnPosition != null)
        {
            TeleportTo(SpawnPosition, SpawnRotation.eulerAngles);
        }
    }

    public void TeleportTo(Vector3 newPosition, Vector3 newRotation)
    {
        transform.SetPositionAndRotation(newPosition, Quaternion.Euler(newRotation));
    }
}