using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class PlayerInteractions : MonoBehaviour
    {
        private StarterAssetsInputs _input;
        private PlayerInput _playerInput; 
        private CharacterController _controller;
        private InputAction grabAction;

        // Start is called before the first frame update
        void Start()
        {
            _input = GetComponent<StarterAssetsInputs>();
            _playerInput = GetComponent<PlayerInput>();
            _controller = GetComponent<CharacterController>();
            grabAction = _playerInput.actions.FindAction("Grab");

            // Called when Grab is triggered
            grabAction.performed += context => {Debug.Log("Grab triggered.");};
        }

        void OnTriggerStay(Collider other)
        {
            if (other.gameObject.name == "Cube" && grabAction.ReadValue<float>() == 1f)
            {
                Debug.Log("Grabbed cube");
                other.enabled = false;
            }
        }

        void OnGrab()
        {
            // Called when grab is triggered
            Debug.Log(grabAction.ReadValue<float>());
        }

    }
}
