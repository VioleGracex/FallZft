using UnityEngine;
using TMPro;
public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f; // Player's health
    [SerializeField] Transform checkpoint; // Last checkpoint position

    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject resumeBtn;

    

    // Method to respawn the player
    public void Respawn()
    {
     
        this.transform.position = checkpoint.position;

        // Take damage upon respawning
        TakeDamage(20f);
        
    }

    // Method to take damage
    private float currentHealth; // Current health of the player
    public TextMeshProUGUI healthText; // Reference to the TextMeshPro text for health display

    private void Start()
    {
        currentHealth = maxHealth; // Initialize current health
        UpdateHealthUI(); // Update health UI at the start
    }

    // Method to take damage
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount; // Subtract damage from current health
        UpdateHealthUI(); // Update health display

        if (currentHealth <= 0)
        {
            Die(); // Call die method if health is zero or less
        }
    }

    // Method to handle player death
    private void Die()
    {
        currentHealth = 0; // Ensure health does not go below zero
        UpdateHealthUI(); // Update health display
        // Add additional logic for player death, such as restarting the game or showing a game over screen
        gameManager.TogglePauseMenu();

        Debug.Log("Player has died!");
    }

    // Method to update the health UI
    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth.ToString("F0"); // Display current health
        }
    }

    // Optional method to reset health
    public void ResetHealth()
    {
        currentHealth = maxHealth; // Reset health to max
        UpdateHealthUI(); // Update health display
    }
}
