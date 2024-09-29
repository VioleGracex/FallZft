using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Collections;

public class CharacterMovementNew : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float jumpForce = 5f; // Changed from jumpHeight to jumpForce for clarity
    public float accelerationTime = 1f;
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Animation Settings")]
    public Animator animator;

    [SerializeField] private string walkAnim = "Walk";
    [SerializeField] private string runAnim = "Run";
    [SerializeField] private string idleAnim = "Idle";
    [SerializeField] private string jumpStartAnim = "JumpStart";
    [SerializeField] private string jumpMidAirAnim = "JumpMidAir";
    [SerializeField] private string jumpLandAnim = "FG_Landing_A";

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;
    public Vector3 groundCheckOffset = Vector3.zero; // Adjustable offset for the raycast origin

    private Vector3 velocity;
    private bool isGrounded;
    [SerializeField] private bool isJumping = false;
    private float landingAnimationTime = 0f;

    // New variables for jump cooldown
    private bool jumpCooldownActive = false;
    private float jumpCooldownTime = 0.2f; // Delay time after jump

    [Header("Cinemachine Settings")]
    public CinemachineFreeLook freeLookCamera; // Reference to Cinemachine FreeLook

    private PlayerControls playerControls;
    private Vector2 moveInput;
    private bool jumpInput;

    private float currentSpeed;
    private float moveHoldTime = 0f;

    private Rigidbody rb; // Rigidbody reference

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody>(); // Get Rigidbody component
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // Freeze unwanted rotations
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();
        playerControls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerControls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        playerControls.Player.Hop.performed += ctx => jumpInput = ctx.ReadValueAsButton();
    }

    private void OnDisable()
    {
        playerControls.Player.Move.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        playerControls.Player.Move.canceled -= ctx => moveInput = Vector2.zero;
        playerControls.Player.Hop.performed -= ctx => jumpInput = ctx.ReadValueAsButton();

        playerControls.Player.Disable();
    }

    private void Start()
    {
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        if (Time.timeScale > 0) // Only update if the game is running
        {
            Move(); // Update player movement
        }
    }

    private void Move()
    {
        // Ground check using Raycast
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position + groundCheckOffset, Vector3.down, out hit, groundCheckDistance, groundLayer);

        // Check if the player is on a Moving Platform
        if (isGrounded && hit.collider.CompareTag("MovingPlatform"))
        {
            // Only set the player as a child of the moving platform if they are not providing input
            if (moveInput.magnitude < 0.1f)
            {
                transform.SetParent(hit.transform); // Set the player as a child of the moving platform
            }
            else
            {
                transform.SetParent(null); // Detach from the platform if moving
            }
        }
        else if (isGrounded)
        {
            // If not on a moving platform, reset the parent
            transform.SetParent(null);
        }

        // Ground check and landing logic
        if (isGrounded && velocity.y <= 0 && !jumpCooldownActive)
        {
            isJumping = false;
        }
        else if (velocity.y < 0)
        {
            PlayAnimation(jumpMidAirAnim);
        }

        // Handle movement
        if (moveInput.magnitude >= 0.1f)
        {
            moveHoldTime += Time.deltaTime;

            // Gradually increase speed from walk to run
            currentSpeed = Mathf.Lerp(walkSpeed, runSpeed, moveHoldTime / accelerationTime);

            // Calculate movement direction based on camera's horizontal axis (yaw)
            float yawRotation = freeLookCamera.m_XAxis.Value;
            Vector3 forward = Quaternion.Euler(0, yawRotation, 0) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0, yawRotation, 0) * Vector3.right;
            Vector3 moveDir = forward * moveInput.y + right * moveInput.x;

            // Smoothly rotate the player in the direction of movement
            if (moveDir.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
            }

            // Apply movement to Rigidbody
            Vector3 movement = moveDir.normalized * currentSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + movement); // Use MovePosition for Rigidbody movement

            // Handle animations
            if (currentSpeed >= runSpeed && isGrounded && !isJumping)
            {
                PlayAnimation(runAnim);
            }
            else if (isGrounded && !isJumping)
            {
                PlayAnimation(walkAnim);
            }
        }
        else
        {
            moveHoldTime = 0f;
            currentSpeed = walkSpeed;

            // Check if the character's velocity is zero to transition to idle
            if (isGrounded && !isJumping)
            {
                PlayAnimation(idleAnim); // Play idle animation
            }
        }

        // Handle jumping
        if (jumpInput && isGrounded && !isJumping)
        {
            Jump();
            jumpInput = false;
        }
    }

    private void Jump()
    {
        // Apply upward force for jumping using AddForce
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        PlayAnimation(jumpMidAirAnim);
        isJumping = true;

        // Start jump cooldown
        StartCoroutine(JumpCooldown());
    }

    private IEnumerator JumpCooldown()
    {
        jumpCooldownActive = true;
        yield return new WaitForSeconds(jumpCooldownTime);
        jumpCooldownActive = false;
    }

    private void PlayAnimation(string animationName)
    {
        if (animationName == jumpStartAnim || animationName == jumpMidAirAnim)
        {
            animator.Play(animationName);
            return;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            return;

        animator.Play(animationName);
    }

    private void OnTriggerEnter(Collider other)
    {
      
    }

    private void OnTriggerExit(Collider other)
    {
    
    }

    // Draw a visual representation of the raycast in the Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Draw the ray from the position (with offset) in the downward direction
        Gizmos.DrawLine(transform.position + groundCheckOffset, transform.position + groundCheckOffset + Vector3.down * groundCheckDistance);
    }
}
