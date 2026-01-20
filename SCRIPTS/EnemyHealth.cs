using UnityEngine;

/// Handles enemy health and damage reception.
/// When health reaches zero, the enemy is destroyed.
public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float health = 50f;    // Current enemy health

    /// Applies damage to the enemy.
    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
        {
            Die();
        }
    }

    /// Destroys the enemy GameObject.
    /// This automatically triggers EnemyLifecycleRelay.OnDestroy.
    void Die()
    {
        Destroy(transform.root.gameObject);
    }
}
