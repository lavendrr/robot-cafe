using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField]
    public EventReference bgm, pickUp, setDown, pourCoffee, bellDing, playerMove, customerDialogue, customerHover, UIClick;
    private EventInstance bgmInstance, moveInstance, customerHoverInstance;
    private List<EventInstance> instances = new();

    private bool playerActive = false;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Subscribe to the state change event
        StateManager.Instance.OnStateChanged += HandleStateChange;

        // Subscribe to game paused events
        StateManager.Instance.OnGamePausedChanged += OnGamePausedChanged;

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
        else
        {
            // Completely stop the move instance & other active instances in other states
            ClearInstances();
            playerActive = false;
            moveInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            RuntimeManager.StudioSystem.setParameterByName("MoveInput", 0f, true);
        }
    }

    private void OnGamePausedChanged(bool gamePaused)
    {
        if (gamePaused)
        {
            // Pause the move instance & any other active instances in the pause state
            playerActive = false;
            foreach (EventInstance instance in instances)
            {
                instance.setPaused(true);
            }
            moveInstance.setPaused(true);
        }
        else
        {
            // Unpause the move instance & any other active instances when not in the pause state
            playerActive = true;
            foreach (EventInstance instance in instances)
            {
                instance.setPaused(false);
            }
            moveInstance.setPaused(false);
        }
    }

    void Update()
    {
        if (!PlayerInteractions.Instance)
        {
            return;
        }

        if (playerActive)
        {
            if (PlayerInteractions.Instance.GetMoveInputState())
            {
                RuntimeManager.StudioSystem.setParameterByName("MoveInput", 1f);
            }
            else
            {
                RuntimeManager.StudioSystem.setParameterByName("MoveInput", 0f);
            }
        }
    }

    public EventInstance? PlaySFX(EventReference eventRef, Vector3? position = null, GameObject parent = null, bool isInstance = false)
    {
        // Sets position equal to the non-nullable Vector3 type if it isn't null, or zero if it is null
        Vector3 pos = position != null ? (Vector3)position : Vector3.zero;
        if (isInstance == false)
        {
            RuntimeManager.PlayOneShot(eventRef, pos);
            return null;
        }
        else
        {
            EventInstance instance = RuntimeManager.CreateInstance(eventRef);
            RuntimeManager.AttachInstanceToGameObject(instance, parent.transform);
            instance.start();
            AddInstance(instance);
            return instance;
        }

    }

    public void AddInstance(EventInstance inst)
    {
        // Saves the event instance in the instance list and returns its index
        instances.Add(inst);
    }

    public void RemoveInstance(EventInstance instance)
    {
        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instances.Remove(instance);
    }

    private void ClearInstances()
    {
        foreach (EventInstance instance in instances)
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instances.Remove(instance);
        }
    }
}
