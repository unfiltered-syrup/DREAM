using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    
    [Header("Config")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float acceleration;
    
    private CharacterController characterController;
    
    // movement
    private Vector2 currentMovementInput;
    private bool isMoving;
    private bool isSprinting;
    private float currentMoveSpeed;
    private Vector3 moveDir = Vector3.zero;
    
    // input
    [HideInInspector] public InputSystem_Actions input;
    
    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void SubscribeInputEvents()
    {
        input = new InputSystem_Actions();
        input.Player.Move.performed += OnMovementInput;
        input.Player.Move.canceled += OnMovementInput;
        input.Player.Sprint.performed += context => isSprinting = true;
        input.Player.Sprint.canceled += context => isSprinting = false;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        characterController = GetComponent<CharacterController>();
        SubscribeInputEvents();
    }

    private void FixedUpdate()
    {
        Move();
    }

    // ACTION METHODS
    // movement
    private void OnMovementInput(InputAction.CallbackContext context)                                    
    {                                                                                            
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovementInput = new Vector2(currentMovementInput.x, currentMovementInput.y);
        isMoving = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }
    // movement (called per frame)
    private void Move()
    {
        // get target speed based on sprint or walk
        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
        if (!isMoving) targetSpeed = 0;
        // get input magnitude, cap at 1
        float inputMagnitude = currentMovementInput.magnitude;
        if (inputMagnitude > 1f) inputMagnitude = 1f;

        // get current horizontal speed
        float currentHorizontalSpeed = new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z).magnitude;
        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - .1f || currentHorizontalSpeed > targetSpeed + .1f)
        {
            currentMoveSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, acceleration * Time.deltaTime);
            // round speed to 3 decimal places
            currentMoveSpeed = Mathf.Round(currentMoveSpeed * 1000f) / 1000f;
        }
        else
        {
            currentMoveSpeed = targetSpeed;
        }
        
        // create move vector
        if (isMoving)
        {
            Vector3 direct = new Vector3(currentMovementInput.x, 0, currentMovementInput.y).normalized;
            float targetAngle = Mathf.Atan2(direct.x, direct.z) * Mathf.Rad2Deg + playerCamera.transform.eulerAngles.y;
            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }

        // Debug.DrawRay(transform.position, moveDir, Color.red);

        // gravity
        var gravityVector = new Vector3(0, characterController.isGrounded ? 0 : -9.81f, 0);
        
        // move player
        characterController.Move(moveDir * (Time.deltaTime * currentMoveSpeed) + gravityVector * Time.deltaTime);
    }
}
