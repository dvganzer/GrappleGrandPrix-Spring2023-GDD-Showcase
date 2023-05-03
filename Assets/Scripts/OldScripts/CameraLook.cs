using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour {

    public Transform lookAt;
    public Transform camTransform;
    public float sensitivityX = 4;
    public float sensitivityY = 4;
    public float Y_ANGLE_MIN = -70.0f;
    public float Y_ANGLE_MAX = 70.0f;
    public Camera cam;

    private float distance = 10;
    float currentX = 0;
    float currentY = 10;



    Vector2 canmeraInput = Vector2.zero;

    private void Start()
    {
        camTransform = transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        
        currentX += canmeraInput.x * sensitivityX;
        currentY -= canmeraInput.y * sensitivityY;

        currentY = Mathf.Clamp(currentY, Y_ANGLE_MIN, Y_ANGLE_MAX);

        Vector3 dir = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        camTransform.position = lookAt.position + rotation * dir;
        camTransform.LookAt(lookAt.position);
    }

    public void OnCamera(InputAction.CallbackContext context)
    {
        canmeraInput = context.ReadValue<Vector2>();
    }
}
