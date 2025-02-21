using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerChecker : MonoBehaviour
{
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    void Start()
    {
        Debug.Log("Connected Devices list START");
        foreach (var device in InputSystem.devices)
        {
            Debug.Log("Connected Device: " + device.displayName);
        }
        Debug.Log("Connected Devices list END");
    }

    void Update()
    {

        if (Gamepad.current != null)
        {
            Vector2 tilt = controls.Gameplay.Tilt.ReadValue<Vector2>();
            Debug.Log($"Tilt vector: {tilt}");

            if (controls.Gameplay.Dive.triggered)
            {
                Debug.Log("Dive button pressed");
            }
        }
        else
        {
            Debug.Log("No gamepad connected");
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }


}


