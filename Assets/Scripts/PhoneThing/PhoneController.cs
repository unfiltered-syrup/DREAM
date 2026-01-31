using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneController : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController cameraController;
    [SerializeField] private Vector2 constraints;
    [SerializeField] private Transform forearmTransform;
    [SerializeField] private Transform pivotTransform;
    
    [SerializeField] private float phoneSpeedWhileActive = 4f;
    [SerializeField] private float phoneSpeedWhileInactive = 2f;

    private Vector3 lastMousePosition;
    private Vector3 smoothVelocity;

    [SerializeField] private float inputSensitivity = 0.0025f;
    [SerializeField] private float idleFallSpeed = 0.25f;
    [SerializeField] private float smoothTime = 0.1f;

    void Start()
    {
        lastMousePosition = Mouse.current.position.ReadValue();
    }

    void Update()
    {
        forearmTransform.position = pivotTransform.position;
        float t = Mathf.InverseLerp(0f, constraints.x, Mathf.Abs(transform.localPosition.x));
        float targetY = Mathf.Sign(transform.localPosition.x) * 30f;
        
        float u = Mathf.InverseLerp(0f, constraints.y, Mathf.Abs(transform.localPosition.y));
        float targetX = Mathf.Sign(transform.localPosition.y) * 10f;
        
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            Quaternion.Euler(targetX * u, targetY * t, 0f),
            Time.deltaTime * 8f
        );

        bool isInteracting = PlayerController.Instance.input.Player.Interact.IsPressed();

        if (PlayerController.Instance.input.Player.Interact.WasPressedThisFrame())
        {
            // Cursor.lockState = CursorLockMode.None;
            lastMousePosition = Mouse.current.position.ReadValue();
        }

        Vector3 inputVector = Vector3.zero;

        if (isInteracting)
        {
            // Mouse delta → direction
            Vector3 currentMouse = Mouse.current.position.ReadValue();
            Vector3 mouseDelta = currentMouse - lastMousePosition;
            lastMousePosition = currentMouse;

            // Joystick
            Vector2 joystick = PlayerController.Instance.input.Player.Look.ReadValue<Vector2>();

            inputVector = new Vector3(
                mouseDelta.x * inputSensitivity + joystick.x,
                mouseDelta.y * inputSensitivity + joystick.y,
                0f
            );

            inputVector = Vector3.ClampMagnitude(inputVector, 1f);
        }
        else
        {
            // Idle fall
            inputVector = Vector3.down * idleFallSpeed;
        }

        float speed = isInteracting ? phoneSpeedWhileActive : phoneSpeedWhileInactive;

        Vector3 targetPosition = transform.localPosition + inputVector * speed * Time.deltaTime;

        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            targetPosition,
            ref smoothVelocity,
            smoothTime
        );

        // Clamp bounds
        transform.localPosition = new Vector3(
            Mathf.Clamp(transform.localPosition.x, -constraints.x, constraints.x),
            Mathf.Clamp(transform.localPosition.y, -constraints.y, constraints.y - 0.125f),
            transform.localPosition.z
        );

        if (PlayerController.Instance.input.Player.Interact.WasReleasedThisFrame())
        {
            // Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
