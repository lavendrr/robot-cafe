using UnityEngine;
using UnityEditor;
using System.IO;

namespace Orders
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager instance { get; private set; }

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

    }
}
