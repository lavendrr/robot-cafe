using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [SerializeField]
    public EventReference bgm, pickUp, pourCoffee, bellDing, playerMove;
    private EventInstance bgmInstance;

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
        // Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Level")
        {
            playerActive = true;
            RuntimeManager.PlayOneShot(playerMove);
        }
    }

    private void Start()
    {
        bgmInstance.getPlaybackState(out PLAYBACK_STATE state);
        if (state == PLAYBACK_STATE.STOPPED)
        {
            bgmInstance = RuntimeManager.CreateInstance(bgm);
            bgmInstance.start();
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
