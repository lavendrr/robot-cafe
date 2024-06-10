using UnityEngine;
using FMODUnity;
using FMOD.Studio;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [SerializeField]
    public EventReference bgm, pickUp, pourCoffee, bellDing, playerMove;
    private EventInstance bgmInstance, moveInstance;

    private bool playerActive = false;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        // Subscribe to the state change event
        StateManager.Instance.OnStateChanged += HandleStateChange;

        bgmInstance.getPlaybackState(out PLAYBACK_STATE state);
        if (state == PLAYBACK_STATE.STOPPED)
        {
            bgmInstance = RuntimeManager.CreateInstance(bgm);
            bgmInstance.start();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the state change event
        StateManager.Instance.OnStateChanged -= HandleStateChange;
    }

    private void HandleStateChange(State newState)
    {
        if (newState.GetType() == typeof(ShiftState))
        {
            // Unpause the moveInstance if it was paused
            moveInstance.setPaused(false);
            playerActive = true;

            // Spin up a new moveInstance if there currently isn't one
            moveInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.STOPPED)
            {
                moveInstance = RuntimeManager.CreateInstance(playerMove);
                moveInstance.start();
            }
        }
        else if (newState.GetType() == typeof(PauseState))
        {
            // Pause the move instance in the pause state
            playerActive = false;
            moveInstance.setPaused(true);
        }
        else
        {
            // Completely stop the move instance in other states
            playerActive = false;
            moveInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            RuntimeManager.StudioSystem.setParameterByName("MoveInput", 0f, true);
        }
    }

    void Update()
    {
        if (playerActive)
        {
            if (PlayerInteractions.instance.GetMoveInputState())
            {
                RuntimeManager.StudioSystem.setParameterByName("MoveInput", 1f);
            }
            else
            {
                RuntimeManager.StudioSystem.setParameterByName("MoveInput", 0f);
            }
        }
    }

    public void PlaySFX(EventReference eventRef, Vector3 position)
    {
        RuntimeManager.PlayOneShot(eventRef, position);
    }
}
