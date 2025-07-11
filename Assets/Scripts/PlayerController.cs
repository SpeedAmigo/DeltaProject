using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Connection;
using FishNet.Object;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkingSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    
    [Header("Camera Settings")]
    [SerializeField] private float sensitivity = 15f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;
    
    private CinemachineCamera playerCamera;
    private CinemachineOrbitalFollow orbitalFollow;
    
    private InputSystem_Actions inputSystem;
    private InputAction move;
    private InputAction sprint;
    private InputAction jump;
    private InputAction scroll;
    
    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isSprinting;
    
    

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            inputSystem.Disable();
            enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            return;
        }
        
        GameObject camera = GameManagerScript.Instance.CreatePlayerCamera(transform);
        playerCamera = camera.GetComponent<CinemachineCamera>();
        orbitalFollow = playerCamera.GetComponent<CinemachineOrbitalFollow>();
    }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //orbitalFollow = playerCamera.GetComponent<CinemachineOrbitalFollow>();
        
        inputSystem = new InputSystem_Actions();
        
        move = inputSystem.Player.Move;
        move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        move.canceled += ctx => moveInput = Vector2.zero;
        
        sprint = inputSystem.Player.Sprint;
        sprint.performed += ctx => isSprinting = ctx.ReadValueAsButton();

        jump = inputSystem.Player.Jump;
        jump.performed += ctx => JumpHandler();
        
        scroll = inputSystem.Player.Scroll;
        scroll.performed += ctx => CameraDistance(ctx.ReadValue<Vector2>().y);
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
        
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized * moveSpeed;
        
        Vector3 velocity = transform.TransformDirection(movement);
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;
    }

    private void JumpHandler()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, 1.5f)) return;

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

    private void CameraDistance(float scrollValue)
    {
        if (Mathf.Abs(scrollValue) < 0.1f) return;
        
        orbitalFollow.Radius -= scrollValue;
        orbitalFollow.Radius = Mathf.Clamp(orbitalFollow.Radius, minDistance, maxDistance);
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, Vector3.down * 1.5f, Color.red);
        
        RotationHandler();
    }
    
    private void FixedUpdate()
    {
        MoveHandler();
    }
}
