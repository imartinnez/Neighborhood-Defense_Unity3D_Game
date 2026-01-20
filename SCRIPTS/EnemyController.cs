using UnityEngine;
using UnityEngine.AI;

/// Controls enemy movement and melee combat behavior.
/// The enemy uses a NavMeshAgent to chase the player
/// and attacks when within range.
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    // Movement
    [Header("Movement")]
    public float moveSpeed = 3.5f;        // Movement speed (scaled per round)
    public float stoppingDistance = 1.2f; // Distance at which the enemy stops near the player

    // Melee Combat
    [Header("Melee Combat")]
    public float baseDamage = 10f;        // Damage dealt per attack (scaled per round)
    public float attackRate = 1.2f;       // Attacks per second
    public float attackRange = 1.6f;      // Distance required to attack

    // Optional References
    [Header("Optional References")]
    public AudioSource audioSource;       // AudioSource used for attack sounds
    public AudioClip attackSFX;            // Attack sound effect

    // Internal State
    NavMeshAgent agent;                    // Navigation agent
    Transform player;                      // Player transform reference
    float nextAttackTime = 0f;             // Cooldown timer for attacks

    // Initialization
    void Awake()
    {
        // Cache NavMeshAgent reference
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        // Find the player using the "Player" tag
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;

        // Configure NavMeshAgent settings
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
    }

    // Update Loop
    void Update()
    {
        if (player == null) return;

        // Chase the player
        agent.speed = moveSpeed; // Allows runtime scaling
        agent.SetDestination(player.position);

        // Check attack range and cooldown
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            TryAttackPlayer();
            nextAttackTime = Time.time + (1f / attackRate);
        }
    }

    // Attack Logic
    void TryAttackPlayer()
    {
        if (player == null) return;

        // Play attack sound
        if (attackSFX != null && audioSource != null)
            audioSource.PlayOneShot(attackSFX);

        // Deal damage to the player
        PlayerHealth playerHealth = player.GetComponentInChildren<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(baseDamage);
        }
    }
}
