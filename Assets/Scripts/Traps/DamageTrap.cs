using System.Collections;
using UnityEngine;

public class DamageTrap : MonoBehaviour
{
    public float damage = 10f; // Amount of damage to deal
    public float activationDelay = 1f; // Delay before damage is applied
    public float rechargeTime = 5f; // Time before the trap can activate again
    private bool isActivated = false; // To check if trap is active
    private bool isInCooldown = false; // To track if the trap is in cooldown
    private Renderer trapRenderer;
    private Coroutine damageCoroutine; // Coroutine for continuous damage

    private void Start()
    {
        trapRenderer = GetComponent<Renderer>();
        trapRenderer.material.color = Color.white; // Default color
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated && !isInCooldown)
        {
            ActivateTrap();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isActivated)
        {
            // Stop the continuous damage coroutine if the player exits
            StopDamageCoroutine();
            ResetTrap(); // Reset the trap
        }
    }

    private void ActivateTrap()
    {
        isActivated = true;
        trapRenderer.material.color = new Color(1f, 0.5f, 0f); // Change color to orange
        Invoke(nameof(DealDamage), activationDelay); // Schedule initial damage after a delay
        damageCoroutine = StartCoroutine(ContinuousDamage()); // Start the coroutine for continuous damage
    }

    private void StopDamageCoroutine()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine); // Stop the continuous damage
            damageCoroutine = null; // Reset the coroutine reference
        }
    }

    private IEnumerator ContinuousDamage()
    {
        while (isActivated && !isInCooldown)
        {
            yield return new WaitForSeconds(rechargeTime); // Wait for the recharge time
            trapRenderer.material.color = new Color(1f, 0.5f, 0f); // Change color to orange
            yield return new WaitForSeconds(1f);
            DealDamage(); // Deal damage
            yield return new WaitForSeconds(1f);
            trapRenderer.material.color = Color.white; // Default color
            StartCooldown(); // Start cooldown after dealing damage
        }
    }

    private void DealDamage()
    {
        // Logic to deal damage to the player
        // Assuming the player has a health component
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        // Flash the trap to red to indicate damage
        trapRenderer.material.color = Color.red;
    }

    private void StartCooldown()
    {
        StopDamageCoroutine(); // Stop the damage while in cooldown
        isInCooldown = true;
        StartCoroutine(CooldownRoutine()); // Start cooldown coroutine
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(rechargeTime); // Wait for the cooldown time
        isInCooldown = false; // Cooldown finished, trap can activate again if player steps on it
        trapRenderer.material.color = new Color(1f, 0.5f, 0f); // Reset to orange after cooldown
        if (isActivated) // If player is still on the trap, resume damage
        {
            damageCoroutine = StartCoroutine(ContinuousDamage());
        }
    }

    private void ResetTrap()
    {
        trapRenderer.material.color = Color.white; // Reset color
        isActivated = false; // Trap can activate again
    }
}
