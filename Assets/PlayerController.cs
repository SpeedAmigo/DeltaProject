using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkingSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    
    [SerializeField] private CinemachineCamera playerCamera;
    
    private InputSystem_Actions inputSystem;
    private InputAction move;
    private InputAction sprint;
    private InputAction jump;
    
    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isSprinting;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputSystem = new InputSystem_Actions();
        
        move = inputSystem.Player.Move;
        move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        move.canceled += ctx => moveInput = Vector2.zero;
        
        sprint = inputSystem.Player.Sprint;
        sprint.performed += ctx => isSprinting = ctx.ReadValueAsButton();

        jump = inputSystem.Player.Jump;
        jump.performed += ctx => JumpHandler();
    }

    private void OnEnable()
    {
        inputSystem.Enable();
    }

    private void OnDisable()
    {
        inputSystem.Disable();        
    }

    private void MoveHandler()
    {
        float moveSpeed = isSprinting ? sprintSpeed : walkingSpeed;
        
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        
        Vector3 velocity = transform.TransformDirection(movement);
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;
    }

    private void JumpHandler()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    
    private void RotationHandler()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float cameraY = playerCamera.transform.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, cameraY, 0f);
            transform.rotation = targetRotation;
        }
    }

    private void Update()
    {
        RotationHandler();
    }
    
    private void FixedUpdate()
    {
        MoveHandler();
    }
}
