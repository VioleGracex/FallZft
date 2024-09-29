using UnityEngine;
using System.Collections;

public class WizardController : MonoBehaviour
{
    public Animator animator;
    public ParticleSystem smokeParticleSystem;

    // Duration of the attack animation
    public float attackAnimationDuration = 5f; // Example duration
    public float attackCooldown = 8f; // Time between attacks

    // Wind force settings
    public float windForce = 10f; // Force applied to the player
    public Vector3 windZoneSize = new Vector3(5f, 1f, 10f); // Size of the wind effect zone

    // Offset for the wind effect zone from the wizard's position
    public Vector3 windZoneOffset; // Adjust this in the Inspector

    // Rotation speed for manual control
    public float rotationSpeed = 100f; // Speed of manual rotation

    // Interval for random Y-axis rotation
    public float rotationInterval = 2f; // Time between random rotations
    public float randomRotationRange = 360f; // Full 360-degree range

    private bool isAttacking; // Flag to check if the wizard is currently attacking

    private void Start()
    {
        // Start the attacking loop
        StartCoroutine(AttackLoop());

        // Start the random rotation loop
        StartCoroutine(RandomRotate());
    }

    private void Update()
    {
        // Rotate the wizard based on mouse input
        RotateWizard();
    }

    private void RotateWizard()
    {
        float horizontalInput = Input.GetAxis("Mouse X"); // Get horizontal mouse input
        Vector3 rotation = new Vector3(0, horizontalInput, 0) * rotationSpeed * Time.deltaTime; // Calculate rotation
        transform.Rotate(rotation); // Rotate the wizard
    }

    private IEnumerator AttackLoop()
    {
        while (true) // Infinite loop for continuous attacking
        {
            isAttacking = true; // Set attacking flag
            PlayAnimation("Attack1"); // Ensure this matches your animation name
            smokeParticleSystem.Play();

            // Start applying wind effect while attacking
            yield return StartCoroutine(ApplyWindEffect());

            StartCoroutine(ReturnToIdle());
            yield return new WaitForSeconds(attackCooldown); // Wait before the next attack
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            return;

        animator.Play(animationName);
    }

    private IEnumerator ReturnToIdle()
    {
        // Wait for the duration of the attack animation
        yield return new WaitForSeconds(attackAnimationDuration);

        // Play the idle animation
        PlayAnimation("Idle");
        smokeParticleSystem.Stop();
        isAttacking = false; // Reset attacking flag
    }

    private IEnumerator ApplyWindEffect()
    {
        // Define the center of the box for the wind zone relative to the wizard's orientation
        Vector3 boxCenter = transform.position + transform.TransformDirection(windZoneOffset);

        // Keep applying wind effect while attacking
        float windEffectDuration = attackAnimationDuration; // Apply for the duration of the attack
        float elapsedTime = 0f;

        while (elapsedTime < windEffectDuration)
        {
            // Find all colliders in the wind zone box, taking into account the wizard's rotation
            Collider[] colliders = Physics.OverlapBox(boxCenter, windZoneSize / 2, transform.rotation);

            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Player")) // Ensure your player has the "Player" tag
                {
                    Rigidbody rb = col.GetComponent<Rigidbody>();
                    if (rb != null && isAttacking) // Only apply force if attacking
                    {
                        // Use the wizard's forward direction as the wind direction
                        Vector3 windDirection = transform.forward; // Wind pushes in the forward direction of the wizard

                        // Apply force to the player in the forward direction of the wizard
                        rb.AddForce(windDirection * windForce, ForceMode.Impulse);
                    }
                }
            }

            elapsedTime += 0.1f; // Increment elapsed time by the delay duration
            yield return new WaitForSeconds(0.1f); // Wait before applying the wind effect again
        }
    }

    private IEnumerator RandomRotate()
    {
        while (true)
        {
            // Wait for the defined interval
            yield return new WaitForSeconds(rotationInterval);

            // Randomly rotate the wizard along the Y-axis
            float randomYRotation = Random.Range(0f, randomRotationRange);
            transform.rotation = Quaternion.Euler(0f, randomYRotation, 0f);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the wire box for the wind zone
        Gizmos.color = Color.blue; // Set the color for the wind zone
        Vector3 boxCenter = transform.position + transform.TransformDirection(windZoneOffset);
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one); // Align gizmo with the wizard's rotation
        Gizmos.DrawWireCube(Vector3.zero, windZoneSize); // Draw a wire cube at local zero, since we already applied the transform
    }
}
