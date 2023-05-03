using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCam : MonoBehaviour
{
    [Header("UI")]
    [SerializedField] public Text SenseText;

    [Header("Camera")]
    //Sensitivty Variables 
    public float sensX;
    public float sensY;

    public Transform orientation;
    [SerializedField] public Vector2 cameraInput = Vector2.zero;
    //Camera Roation Variables
    float xRotation;
    float yRotation;

    public Slider slider;
    public enum ControllerType
    {
        KeyboardMouse,
        Gamepad
    }
    public ControllerType currentControllerType;
    private float lastInputTime = 0f;


    private void Start()
    {
        //Locks Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        lastInputTime = Time.time;
        CheckCurrentControllerType();
    }
    public void ValueChangeCheck()
    {
        // Update the value variable with the slider's current value
        sensX = slider.value;
        sensY = slider.value;
    }

    public void CheckCurrentControllerType()
    {
        float currentTime = Time.time;
        bool gamepadConnected = Gamepad.current != null && (currentTime - Gamepad.current.lastUpdateTime) < 0.1f;
        bool keyboardMouseConnected = Keyboard.current != null && (currentTime - Keyboard.current.lastUpdateTime) < 0.1f;
        if (gamepadConnected)
        {
            currentControllerType = ControllerType.Gamepad;
            sensX = 100f;
            sensY = 100f;
            lastInputTime = (float)Gamepad.current.lastUpdateTime;
        }
        else if (keyboardMouseConnected)
        {
            currentControllerType = ControllerType.KeyboardMouse;
            lastInputTime = (float)Keyboard.current.lastUpdateTime;
            sensX = 10f;
            sensY = 10f;
        }
        else if (currentTime - lastInputTime > 0.5f)
        { // if no input has been received in the last 0.5 seconds, assume keyboard/mouse
            currentControllerType = ControllerType.KeyboardMouse;
            sensX = 10f;
            sensY = 10f;
        }
    }




    private void Update()
    {
        //Gets InputSystem for Controller and Mouse
        float mouseX = cameraInput.x * Time.deltaTime * sensX;
        float mouseY = cameraInput.y * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;       
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        //Rotates Camera and Orientation of Player

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        CheckCurrentControllerType();

        slider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        sensX = slider.value;
        sensY = slider.value;


    }

    public void OnCamera(InputAction.CallbackContext context)
    {
        cameraInput = context.ReadValue<Vector2>();
    }


}

