using UnityEngine;


/// Manages the player's health:
/// - Tracks current and maximum health
/// - Updates the HUD
/// - Notifies the RoundManager when the player dies

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;     // Maximum player health

    float currentHealth;               // Current player health

    // Initialization
    void Start()
    {
        // Initialize health at the start of the game
        currentHealth = maxHealth;

        // Update HUD to show full health
        HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    // Damage Handling
    public void TakeDamage(float damage)
    {
        // Reduce current health
        currentHealth -= damage;

        // Clamp health to 0 and update the HUD
        HUDManager.Instance.UpdateHealth(Mathf.Max(0f, currentHealth), maxHealth);

        // Check for death
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // Death Logic
    void Die()
    {
        // Inform the RoundManager that the player has lost
        RoundManager.Instance.GameOver();
    }
}
