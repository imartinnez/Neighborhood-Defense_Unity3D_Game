using System.Collections;
using UnityEngine;

/// Handles all weapon behavior:
/// - Shooting logic and fire rate
/// - Physical projectile spawning
/// - Ammo, reload, and HUD updates
/// - Recoil (kick + return)
/// - Idle weapon sway
/// - Audio and visual feedback
public class Weapon : MonoBehaviour
{
    // Crosshair UI
    [Header("UI")]
    public RectTransform crosshairRect; // On-screen crosshair reference

    // Core weapon stats
    [Header("Weapon Stats")]
    public float damage = 10f;      // Damage per bullet
    public float fireRate = 5f;     // Shots per second

    // Main references
    [Header("References")]
    public Camera playerCamera;             // Player camera
    public ParticleSystem muzzleFlashPS;    // Muzzle flash VFX

    // Projectile settings
    [Header("Projectile")]
    public Transform muzzlePoint;    // Bullet spawn point
    public Bullet bulletPrefab;      // Bullet prefab
    public float bulletSpeed = 80f;  // Bullet velocity
    public float bulletLifetime = 3f;

    // Impact VFX (forwarded to Bullet)
    [Header("Impact VFX (passed to Bullet)")]
    public GameObject impactDefaultPrefab; // Non-flesh impact VFX
    public GameObject impactFleshPrefab;   // Flesh impact VFX
    public LayerMask fleshMask;             // Flesh detection layers

    // Audio
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootSFX;

    // Reload
    [Header("Reload")]
    public AudioClip reloadSFX;
    public float reloadTime = 1.4f;

    // Ammo system
    [Header("Ammo")]
    public bool useAmmo = true;     // Enable / disable ammo usage
    public int magazineSize = 10;   // Bullets per magazine

    // Recoil behavior
    [Header("Recoil")]
    public Transform recoilTransform;   // Weapon transform affected by recoil
    public float recoilKickback = 0.06f;
    public float recoilRotation = 4f;
    public float recoilSnapSpeed = 90f;
    public float recoilReturnSpeed = 18f;

    // Idle weapon sway
    [Header("Idle Sway")]
    public bool enableIdleSway = true;
    public float idleAmplitude = 0.01f;
    public float idleFrequency = 1.5f;

    [Header("Aim Tuning")]
    public Vector2 crosshairPixelOffset = Vector2.zero;


    // Internal state
    float nextTimeToFire;
    int ammoInMag;
    bool isReloading;

    // Recoil state
    Vector3 initialLocalPos;
    Quaternion initialLocalRot;
    Vector3 currentRecoilPos, targetRecoilPos;
    Vector3 currentRecoilRot, targetRecoilRot;

    // Initialization
    void Start()
    {
        // Fill magazine on start
        ammoInMag = Mathf.Clamp(magazineSize, 0, magazineSize);

        // Cache initial recoil transform pose
        if (recoilTransform != null)
        {
            initialLocalPos = recoilTransform.localPosition;
            initialLocalRot = recoilTransform.localRotation;
        }

        // Update ammo HUD
        UpdateAmmoHUD(false);
    }

    // Main update loop
    void Update()
    {
        // Handle recoil interpolation every frame
        UpdateRecoilMotion();

        if (isReloading) return;

        // Manual reload input
        if (useAmmo && Input.GetKeyDown(KeyCode.R) && ammoInMag < magazineSize)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        // Fire input
        if (!Input.GetButton("Fire1")) return;
        if (Time.time < nextTimeToFire) return;

        // Fire rate control
        nextTimeToFire = Time.time + 1f / fireRate;

        // No-ammo mode (infinite shooting)
        if (!useAmmo)
        {
            Shoot();
            return;
        }

        // Ammo-based shooting
        if (ammoInMag > 0)
        {
            Shoot();
            ammoInMag--;
            UpdateAmmoHUD(false);

            if (ammoInMag <= 0)
                StartCoroutine(ReloadRoutine());
        }
        else
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    /// Handles a single shot:
    /// - Muzzle flash
    /// - Audio
    /// - Bullet spawn and configuration
    /// - Recoil kick
    void Shoot()
    {
        if (muzzleFlashPS != null)
            muzzleFlashPS.Play();

        PlayOneShot(shootSFX);

        if (bulletPrefab == null || playerCamera == null)
            return;

        // Ray goes through the on-screen crosshair position.
        Vector2 screenPoint = (crosshairRect != null)
            ? RectTransformUtility.WorldToScreenPoint(null, crosshairRect.position)
            : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        // Small manual correction if the crosshair sprite isn't perfectly centered.
        // (+X = right, +Y = up)
        screenPoint += crosshairPixelOffset;

        Ray ray = playerCamera.ScreenPointToRay(screenPoint);
        Vector3 dir = ray.direction.normalized;

        Vector3 spawnPos = (muzzlePoint != null)
            ? muzzlePoint.position
            : (playerCamera.transform.position + dir * 0.25f);

        Bullet b = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir, Vector3.up));

        b.impactDefaultPrefab = impactDefaultPrefab;
        b.impactFleshPrefab = impactFleshPrefab;
        b.fleshMask = fleshMask;
        b.maxLifetime = bulletLifetime;

        b.Init(damage, transform.root);

        Rigidbody rb = b.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearDamping = 0f;
            rb.linearVelocity = dir * bulletSpeed;
        }

        ApplyRecoilKick();
    }

    /// Applies an instant recoil impulse.
    void ApplyRecoilKick()
    {
        if (recoilTransform == null) return;

        // Backward kick
        targetRecoilPos += new Vector3(0f, 0f, -recoilKickback);

        // Randomized rotation for natural feel
        float randomYaw = Random.Range(-recoilRotation * 0.35f, recoilRotation * 0.35f);
        float randomRoll = Random.Range(-recoilRotation * 0.2f, recoilRotation * 0.2f);

        targetRecoilRot += new Vector3(recoilRotation, randomYaw, randomRoll);
    }

    /// Smoothly interpolates recoil and idle sway.
    void UpdateRecoilMotion()
    {
        if (recoilTransform == null) return;

        // Idle sway motion
        Vector3 idleOffset = Vector3.zero;
        if (enableIdleSway)
        {
            float t = Time.time * idleFrequency;
            idleOffset = new Vector3(
                0f,
                Mathf.Sin(t) * idleAmplitude,
                Mathf.Cos(t * 0.5f) * idleAmplitude * 0.4f
            );
        }

        // Recoil return
        targetRecoilPos = Vector3.Lerp(targetRecoilPos, Vector3.zero, recoilReturnSpeed * Time.deltaTime);
        targetRecoilRot = Vector3.Lerp(targetRecoilRot, Vector3.zero, recoilReturnSpeed * Time.deltaTime);

        // Recoil snap
        currentRecoilPos = Vector3.Lerp(currentRecoilPos, targetRecoilPos, recoilSnapSpeed * Time.deltaTime);
        currentRecoilRot = Vector3.Lerp(currentRecoilRot, targetRecoilRot, recoilSnapSpeed * Time.deltaTime);

        // Apply final transform
        recoilTransform.localPosition = initialLocalPos + idleOffset + currentRecoilPos;
        recoilTransform.localRotation = initialLocalRot * Quaternion.Euler(currentRecoilRot);
    }

    /// Handles reload timing and ammo reset.
    IEnumerator ReloadRoutine()
    {
        if (isReloading) yield break;

        isReloading = true;
        UpdateAmmoHUD(true);

        PlayOneShot(reloadSFX);

        float wait = (reloadSFX != null) ? reloadSFX.length : reloadTime;
        yield return new WaitForSeconds(wait);

        ammoInMag = magazineSize;
        isReloading = false;

        UpdateAmmoHUD(false);
    }

    /// Plays a one-shot audio clip.
    void PlayOneShot(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

    /// Updates the ammo UI through the HUDManager.
    void UpdateAmmoHUD(bool reloading)
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateAmmo(ammoInMag, magazineSize, reloading);
    }
}