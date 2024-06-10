using UnityEngine;
using FMODUnity;
using FMOD.Studio;


    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance { get; private set; }

        [SerializeField]
        public EventReference bgm, pickUp, pourCoffee, bellDing, playerMove;
        private EventInstance bgmInstance;

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
            bgmInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.STOPPED)
            {
                bgmInstance = RuntimeManager.CreateInstance(bgm);
                bgmInstance.start();
            }
            RuntimeManager.PlayOneShot(playerMove);
        }

        void Update()
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

        public void PlaySFX(EventReference eventRef, Vector3 position)
        {
            RuntimeManager.PlayOneShot(eventRef, position);
        }
    }
