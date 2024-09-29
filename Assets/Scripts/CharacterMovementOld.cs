using UnityEngine;

public class CharacterMovementOld : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float accelerationTime = 2f; // Time taken to reach full running speed
    private float currentSpeed;
    private float accelerationRate; // Rate of speed increase per second
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Animation Settings")]
    public Animator animator; // Reference to the Animator
    public CharacterController controller; // Reference to the CharacterController

    [SerializeField] private string walkAnim = "Walk";
    [SerializeField] private string runAnim = "Run";
    [SerializeField] private string idleAnim = "Idle";
    [SerializeField] private string inclineForwardAnim = "InclineForward"; // Incline forward animation
    [SerializeField] private string declineForwardAnim = "DeclineForward"; // Decline forward animation

    [SerializeField] private string jumpStartAnim = "JumpStart";  // Jump start animation
    [SerializeField] private string jumpMidAirAnim = "JumpMidAir";  // Jump mid-air animation
    [SerializeField] private string jumpLandAnim = "FG_Landing_A";  // Jump landing animation

    [Header("Ground Detection")]
    public LayerMask groundLayer; // LayerMask for ground detection
    public float groundCheckDistance = 0.1f; // Distance for ground check

    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping = false;  // Track if the character is in a jump state
    private bool inAir = false;      // Track if the character is in mid-air
    private float landingAnimationTime = 0f; // Timer for landing animation

    [Header("Cinemachine Settings")]
    public Transform cameraTransform; // Reference to the camera (used for directional movement)

    private Quaternion initialRotation; // Store the initial rotation of the player

    private Vector3 currentMoveDir; // Store the current movement direction
    private bool isMoving = false; // Track if the character is currently moving

    void Start()
    {
        initialRotation = transform.rotation; // Save the initial rotation of the player
        currentSpeed = walkSpeed; // Start at walking speed
        accelerationRate = (runSpeed - walkSpeed) / accelerationTime; // Calculate acceleration rate
    }

    void Update()
    {
        Move();
        ApplyGravity();
    }

    private void Move()
    {
        // Ground check using Raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // Ground check and landing logic
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            if (isJumping || inAir) // If the character was jumping and is now grounded
            {
                PlayAnimation(jumpLandAnim); // Play landing animation
                landingAnimationTime = animator.GetCurrentAnimatorStateInfo(0).length; // Store length of landing animation
                isJumping = false;  // Reset jump state
                inAir = false;      // Reset mid-air state
            }
        }

        // If landing animation is playing, keep the character in landing state
        if (landingAnimationTime > 0f)
        {
            landingAnimationTime -= Time.deltaTime; // Decrease the timer
            return; // Skip movement and return to prevent idle transition
        }

        // Get input values
        float horizontal = Input.GetAxisRaw("Horizontal"); // Controller or keyboard input
        float vertical = Input.GetAxisRaw("Vertical"); // Controller or keyboard input
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Handle movement
        if (direction.magnitude >= 0.1f)
        {
            isMoving = true; // Character is moving

            // Calculate movement direction based on camera orientation
            Vector3 moveDir = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
            moveDir.y = 0; // We don't want vertical movement
            currentMoveDir = moveDir.normalized; // Update current movement direction

            // Rotate the character towards the movement direction
            float targetAngle = Mathf.Atan2(currentMoveDir.x, currentMoveDir.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Gradually increase speed from walkSpeed to runSpeed
            currentSpeed += accelerationRate * Time.deltaTime; // Gradually increase speed
            currentSpeed = Mathf.Clamp(currentSpeed, walkSpeed, runSpeed); // Clamp speed between walk and run

            // Apply movement to the character
            controller.Move(currentMoveDir * currentSpeed * Time.deltaTime);

            // Play appropriate animation based on current speed
            if (currentSpeed < runSpeed)
            {
                PlayAnimation(walkAnim); // Play walking animation
            }
            else
            {
                PlayAnimation(runAnim); // Play running animation
            }
        }
        else
        {
            // If no input is detected, return to idle
            if (isGrounded && !isJumping && !inAir)
            {
                PlayAnimation(idleAnim);
            }
            isMoving = false; // Reset movement state

            // Reset speed to walking speed when stopping
            currentSpeed = walkSpeed;
        }

        // Handle jumping
        if ((Input.GetButtonDown("Jump") && isGrounded) || (Input.GetKeyDown(KeyCode.Space) && isGrounded))
        {
            Jump();
        }
    }

    private void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // Calculate jump velocity
        PlayAnimation(jumpStartAnim); // Play jump start animation
        isJumping = true;
    }

    private void ApplyGravity()
    {
        // When the character is in mid-air and falling, trigger mid-air animation
        if (!isGrounded && velocity.y < 0 && !inAir)
        {
            PlayAnimation(jumpMidAirAnim); // Play mid-air animation
            inAir = true;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Play the correct animation based on its name
    private void PlayAnimation(string animationName)
    {
        // Allow jump animations to interrupt current animations
        if (animationName == jumpStartAnim || animationName == jumpMidAirAnim)
        {
            animator.Play(animationName);
            return;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            return; // Skip if already playing the animation

        animator.Play(animationName);
    }
}
