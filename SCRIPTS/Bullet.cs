using UnityEngine;

/// Simple physical bullet behavior:
/// - Uses Rigidbody + Collider
/// - Applies damage on collision
/// - Spawns impact VFX (default or flesh)
/// - Ignores collisions with the shooter
/// - Destroys itself on impact or after a lifetime
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    // Damage Settings
    [Header("Damage")]
    public float damage = 10f; // Damage dealt on hit

    // Lifetime Settings
    [Header("Lifetime")]
    public float maxLifetime = 3f; // Auto-destroy time if no collision happens

    // Impact Visual Effects
    [Header("Impact VFX")]
    public GameObject impactDefaultPrefab; // VFX for non-organic surfaces
    public GameObject impactFleshPrefab;   // VFX for enemies / flesh

    [Header("Impact SFX")]
    public AudioClip impactDefaultSFX;
    public AudioClip impactFleshSFX;   // enemy sound
    [Range(0f, 1f)] public float impactVolume = 0.8f;


    // Optional layer-based flesh detection
    [Header("Optional: Flesh Detection")]
    public LayerMask fleshMask; // Layers considered as "flesh"

    // Shooter reference (used to avoid self-collision)
    Transform owner;

    // Initialization
    void Start()
    {
        // Destroy the bullet after its maximum lifetime
        Destroy(gameObject, maxLifetime);
    }

    /// Initializes the bullet with damage and owner reference.
    /// This also ignores collisions with the shooter's colliders.
    /// <param name="newDamage">Damage applied on impact</param>
    /// <param name="newOwner">Transform of the shooter (player or weapon root)</param>
    public void Init(float newDamage, Transform newOwner)
    {
        damage = newDamage;
        owner = newOwner;

        if (owner == null) return;

        Collider myCol = GetComponent<Collider>();
        if (myCol == null) return;

        // Ignore collisions with all colliders on the owner
        foreach (var c in owner.GetComponentsInChildren<Collider>())
        {
            if (c != null)
                Physics.IgnoreCollision(myCol, c, true);
        }
    }

    // Collision Handling
    void OnCollisionEnter(Collision collision)
    {
        // Get contact point and surface normal
        ContactPoint cp = collision.GetContact(0);

        // Check for EnemyHealth on the hit object or its parents
        EnemyHealth enemy = collision.transform.GetComponentInParent<EnemyHealth>();

        // Determine if the surface is considered flesh
        bool isFlesh =
            enemy != null ||
            ((fleshMask.value & (1 << collision.gameObject.layer)) != 0);

        // Apply damage if an enemy was hit
        if (enemy != null)
            enemy.TakeDamage(damage);

        // Spawn impact VFX
        GameObject prefab = isFlesh ? impactFleshPrefab : impactDefaultPrefab;
        if (prefab != null)
        {
            GameObject fx = Instantiate(
                prefab,
                cp.point,
                Quaternion.LookRotation(cp.normal)
            );

            Destroy(fx, 3f);
        }

        // Play impact SFX
        AudioClip sfx = isFlesh ? impactFleshSFX : impactDefaultSFX;
        if (sfx != null)
        {
            AudioSource.PlayClipAtPoint(sfx, cp.point, impactVolume);
        }

        // Destroy the bullet after impact
        Destroy(gameObject);
    }
}