using UnityEngine;
using FMODUnity;

namespace Orders
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance { get; private set; }

        [SerializeField]
        public EventReference pickUp, pourCoffee, bellDing;

        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.
            if (instance != null && instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                instance = this; 
            }
        }

        public void PlaySFX(EventReference eventRef, Vector3 position)
        {
            RuntimeManager.PlayOneShot(eventRef, position);
        }
    }
}
