using UnityEngine;
using UnityEngine.InputSystem; // Required for the new system

[RequireComponent(typeof(CharacterController))]
public class PlayerTestControl : MonoBehaviour
{
    [Header("Input Action References")]
    public InputActionReference moveAction; // Drag "Move" action here
    public InputActionReference lookAction; // Drag "Look" action here

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 0.05f; // Sensitivity is different in New Input System (pixels)
    public float lookXLimit = 90.0f;

    private CharacterController characterController;
    private Vector3 velocity;
    private float xRotation = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        // Enable the input actions when the script is active
        moveAction.action.Enable();
        lookAction.action.Enable();
    }

    void OnDisable()
    {
        // Disable them to prevent errors/leaks
        moveAction.action.Disable();
        lookAction.action.Disable();
    }

    void Update()
    {
        // --- 1. Rotation (Mouse Look) ---
        // Read value returns a Vector2 (x, y)
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
        
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -lookXLimit, lookXLimit);
        
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);


        // --- 2. Movement (WASD) ---
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        
        // Convert 2D input (x, y) to 3D direction (x, 0, z)
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        characterController.Move(move * moveSpeed * Time.deltaTime);


        // --- 3. Gravity (Same as before) ---
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}