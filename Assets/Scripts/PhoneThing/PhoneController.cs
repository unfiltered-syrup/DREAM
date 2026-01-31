using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneController : MonoBehaviour
{
    private Vector3 startingMousePosition;
    private Vector3 mousePosition;
    [SerializeField] private CinemachineInputAxisController cameraController;
    [SerializeField] private Vector2 constraints;
    [SerializeField] private Transform forearmTransform;
    [SerializeField] private Transform pivotTransform;
    
    [SerializeField] private float phoneSpeedWhileActive = 40f;
    [SerializeField] private float phoneSpeedWhileInactive = 20f;

    private void Start()
    {
        startingMousePosition = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        forearmTransform.position = pivotTransform.position;
        
        if (PlayerController.Instance.input.Player.Interact.WasPressedThisFrame())
        {
            Cursor.lockState = CursorLockMode.None;
            // cameraController.enabled = false;
            // Cursor.visible = true;
            // cameraObject.SetActive(false);
        }
        
        mousePosition = Mouse.current.position.ReadValue();
        var joystickInput = PlayerController.Instance.input.Player.Look.ReadValue<Vector2>();
        mousePosition += new Vector3(joystickInput.x, joystickInput.y, 0) * 10f;
        Vector3 difference = PlayerController.Instance.input.Player.Interact.IsPressed() ? (mousePosition - startingMousePosition) : Vector3.down;
        
        var targetPosition = transform.localPosition + new Vector3(difference.normalized.x, difference.normalized.y, 0) * Time.deltaTime;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, (PlayerController.Instance.input.Player.Interact.IsPressed() ? phoneSpeedWhileActive : phoneSpeedWhileInactive) * Time.deltaTime);
        transform.localPosition = new Vector3(
            Mathf.Clamp(transform.localPosition.x, -constraints.x, constraints.x),
            Mathf.Clamp(transform.localPosition.y, -constraints.y, constraints.y),
            transform.localPosition.z
        );
        
        if (PlayerController.Instance.input.Player.Interact.WasReleasedThisFrame())
        {
            Cursor.lockState = CursorLockMode.Locked;
            // cameraController.enabled = true;
            // Cursor.visible = false;
            // cameraObject.SetActive(true);
        }
    }
}
