using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ENHANCED TankShooting - Multi-FirePoint Support
/// 
/// SAFELY EXTENDS the original TankShooting to handle multiple turrets
/// WITHOUT breaking transition scene logic or upgrade wheel functionality
/// 
/// KEEPS ALL ORIGINAL FUNCTIONALITY:
/// - SetBulletSpeed() and SetFireRate() methods for TankStats compatibility
/// - Auto-fire toggle with E key
/// - All original audio and effects
/// 
/// NEW FUNCTIONALITY:
/// - Automatically finds ALL objects tagged with "FirePoint"
/// - Fires from ALL FirePoints simultaneously
/// - Maintains backward compatibility with single FirePoint systems
/// </summary>
public class TankShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    private float bulletSpeed = 5f;      // Will be set by TankStats dynamically
    private float fireRate = 1.2f;       // Will be set by TankStats dynamically
    [SerializeField] private float bulletLifetime = 5f;
    private float bulletScale = 1.5f;      // Bullet size multiplier

    [Header("Auto Fire Settings")]
    // Using Input System instead of legacy input

    [Header("Multi-FirePoint Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool fireAllTurretsSimultaneously = true;

    [Header("Audio & Effects")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private ParticleSystem muzzleFlash;  // Keep for compatibility

    // Component references (UNCHANGED from original)
    private TankController tankController;
    private AudioSource audioSource;

    // Shooting state (UNCHANGED from original)
    private float nextFireTime = 0f;
    private bool isAutoFireEnabled = false;
    private bool wasAutoFireKeyPressed = false;

    // ★ NEW: Multi-turret support
    private List<Transform> allFirePoints = new List<Transform>();
    private List<ParticleSystem> muzzleFlashes = new List<ParticleSystem>();
    private bool firePointsInitialized = false;

    void Awake()
    {
        // UNCHANGED from original TankShooting
        tankController = GetComponent<TankController>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        DebugLog("TankShooting Awake() - Multi-turret support enabled");
    }

    void Start()
    {
        // ★ NEW: Initialize multiple fire points
        InitializeFirePoints();
    }

    /// <summary>
    /// ★ NEW: Find and setup all FirePoints tagged with "FirePoint"
    /// This method safely discovers multiple turrets without breaking existing logic
    /// </summary>
    private void InitializeFirePoints()
    {
        DebugLog("Initializing multi-FirePoint support...");

        allFirePoints.Clear();

        // Find ALL objects with the "FirePoint" tag
        GameObject[] firePointObjects = GameObject.FindGameObjectsWithTag("FirePoint");

        DebugLog($"Found {firePointObjects.Length} objects with 'FirePoint' tag");

        foreach (GameObject fpObj in firePointObjects)
        {
            // Only include FirePoints that are children of this tank
            if (IsChildOfThisTank(fpObj.transform))
            {
                allFirePoints.Add(fpObj.transform);
                DebugLog($"Added FirePoint: {fpObj.name} at position {fpObj.transform.position}");
            }
        }

        // Fallback: If no tagged FirePoints found, look for objects named "FirePoint"
        if (allFirePoints.Count == 0)
        {
            Transform[] allChildren = GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child.name.ToLower().Contains("firepoint"))
                {
                    allFirePoints.Add(child);
                    DebugLog($"Fallback: Added FirePoint by name: {child.name}");
                }
            }
        }

        // Setup muzzle flashes for each fire point
        SetupMuzzleFlashes();

        firePointsInitialized = true;
        DebugLog($"Multi-FirePoint initialization complete! Total FirePoints: {allFirePoints.Count}");
    }

    /// <summary>
    /// Check if a transform is a child of this tank
    /// </summary>
    private bool IsChildOfThisTank(Transform firePoint)
    {
        Transform current = firePoint.parent;
        while (current != null)
        {
            if (current == this.transform)
                return true;
            current = current.parent;
        }
        return false;
    }

    /// <summary>
    /// ★ NEW: Setup muzzle flash effects for all fire points
    /// </summary>
    private void SetupMuzzleFlashes()
    {
        muzzleFlashes.Clear();

        if (muzzleFlash == null)
        {
            DebugLog("No muzzle flash prefab set - skipping muzzle flash setup");
            return;
        }

        foreach (Transform firePoint in allFirePoints)
        {
            if (firePoint != null)
            {
                // Create a muzzle flash for this fire point
                ParticleSystem flash = Instantiate(muzzleFlash, firePoint);
                flash.transform.localPosition = Vector3.zero;
                flash.transform.localRotation = Quaternion.identity;
                flash.Stop(); // Start stopped
                muzzleFlashes.Add(flash);
                DebugLog($"Created muzzle flash for: {firePoint.name}");
            }
        }
    }

    /// <summary>
    /// UNCHANGED: Set bullet speed (called by TankStats)
    /// </summary>
    public void SetBulletSpeed(float speed)
    {
        bulletSpeed = speed;
        DebugLog($"TankShooting.SetBulletSpeed: {speed:F2} (GameObject: {gameObject.name})");
    }

    /// <summary>
    /// UNCHANGED: Set fire rate (called by TankStats)
    /// </summary>
    public void SetFireRate(float rate)
    {
        fireRate = rate;
        DebugLog($"TankShooting.SetFireRate: {rate:F2} (GameObject: {gameObject.name})");
    }

    /// <summary>
    /// NEW: Set bullet scale (called by TankTransformationManager)
    /// </summary>
    public void SetBulletScale(float scale)
    {
        bulletScale = scale;
        DebugLog($"TankShooting.SetBulletScale: {scale:F2} (GameObject: {gameObject.name})");
    }

    void Update()
    {
        // UNCHANGED from original
        HandleAutoFireToggle();
        HandleShooting();

        // ★ NEW: Reinitialize fire points if needed (handles tank transformations)
        if (!firePointsInitialized || allFirePoints.Count == 0)
        {
            InitializeFirePoints();
        }
    }

    /// <summary>
    /// UNCHANGED: Handle auto-fire toggle with E key
    /// </summary>
    private void HandleAutoFireToggle()
    {
        if (Keyboard.current != null)
        {
            bool isEKeyPressed = Keyboard.current.eKey.isPressed;

            if (isEKeyPressed && !wasAutoFireKeyPressed)
            {
                isAutoFireEnabled = !isAutoFireEnabled;
                DebugLog($"Auto-fire: {(isAutoFireEnabled ? "ENABLED" : "DISABLED")}");
            }

            wasAutoFireKeyPressed = isEKeyPressed;
        }
    }

    /// <summary>
    /// ENHANCED: Handle shooting with multi-turret support
    /// </summary>
    private void HandleShooting()
    {
        bool shouldShoot = false;

        // UNCHANGED: Manual shooting (mouse click)
        if (tankController != null && tankController.IsShootPressed())
        {
            shouldShoot = true;
        }
        // UNCHANGED: Auto-fire mode
        else if (isAutoFireEnabled)
        {
            shouldShoot = true;
        }

        if (shouldShoot && CanShoot())
        {
            // ★ ENHANCED: Fire from all turrets
            FireFromAllTurrets();
        }
    }

    /// <summary>
    /// UNCHANGED: Check if we can shoot
    /// </summary>
    private bool CanShoot()
    {
        return Time.time >= nextFireTime && bulletPrefab != null && allFirePoints.Count > 0;
    }

    /// <summary>
    /// ★ NEW: Fire bullets from all turrets simultaneously
    /// </summary>
    private void FireFromAllTurrets()
    {
        // Set next fire time
        nextFireTime = Time.time + (1f / fireRate);

        int bulletsFired = 0;

        // Fire from each turret
        for (int i = 0; i < allFirePoints.Count; i++)
        {
            if (allFirePoints[i] != null)
            {
                FireBulletFromPoint(allFirePoints[i], i);
                bulletsFired++;
            }
        }

        // Play sound and effects
        if (bulletsFired > 0)
        {
            PlayShootSound();
            DebugLog($"Fired {bulletsFired} bullets from {allFirePoints.Count} turrets");
        }
    }

    /// <summary>
    /// ★ NEW: Fire a bullet from a specific fire point
    /// </summary>
    private void FireBulletFromPoint(Transform firePoint, int turretIndex)
    {
        if (firePoint == null || bulletPrefab == null) return;

        // Create the bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // ★ ENHANCED: Apply bullet size scaling based on tank upgrade
        float bulletScale = GetCurrentBulletScale();
        bullet.transform.localScale = Vector3.one * bulletScale;

        // Set bullet velocity
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = firePoint.forward * bulletSpeed;
        }

        // ★ ENHANCED: Configure bullet script with damage scaling
        ConfigureBulletDamage(bullet, bulletScale);

        // Play muzzle flash
        if (turretIndex < muzzleFlashes.Count && muzzleFlashes[turretIndex] != null)
        {
            muzzleFlashes[turretIndex].Play();
        }

        // Destroy bullet after lifetime
        Destroy(bullet, bulletLifetime);

        DebugLog($"Bullet fired from {firePoint.name} with speed {bulletSpeed}, scale {bulletScale:F2}");
    }

    /// <summary>
    /// UNCHANGED: Play shoot sound
    /// </summary>
    private void PlayShootSound()
    {
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    /// <summary>
    /// ★ NEW: Public method to refresh fire points after tank transformations
    /// Can be called by TankTransformationManager or other systems
    /// </summary>
    public void RefreshFirePoints()
    {
        DebugLog("Refreshing FirePoints after tank transformation...");
        firePointsInitialized = false;
        InitializeFirePoints();
    }

    /// <summary>
    /// ★ NEW: Get current fire point count (for debugging)
    /// </summary>
    public int GetFirePointCount()
    {
        return allFirePoints.Count;
    }

    /// <summary>
    /// ★ COMPATIBILITY: SetFirePoints method for TankTransformationManager
    /// This method exists for backward compatibility with existing systems
    /// </summary>
    public void SetFirePoints(List<Transform> newFirePoints)
    {
        if (newFirePoints != null && newFirePoints.Count > 0)
        {
            DebugLog($"SetFirePoints called with {newFirePoints.Count} fire points");
            allFirePoints.Clear();
            allFirePoints.AddRange(newFirePoints);

            // Re-setup muzzle flashes for the new fire points
            SetupMuzzleFlashes();
            firePointsInitialized = true;

            DebugLog($"Updated fire points via SetFirePoints: {allFirePoints.Count} total");
        }
        else
        {
            DebugLog("SetFirePoints called with empty or null list - re-initializing from tags");
            InitializeFirePoints();
        }
    }

    /// <summary>
    /// ★ NEW: Get current bullet scale from upgrade system
    /// </summary>
    private float GetCurrentBulletScale()
    {
        DebugLog($"Checking bullet scale sources... Current bulletScale variable: {bulletScale}");

        // PRIORITY 1: Use the bulletScale set by TankTransformationManager
        if (bulletScale != 1f)
        {
            DebugLog($"✅ Using bulletScale from TankTransformationManager: {bulletScale}");
            return bulletScale;
        }

        // PRIORITY 2: Check PlayerDataManager (fallback for saved transformations)
        var playerDataManager = PlayerDataManager.Instance;
        if (playerDataManager != null)
        {
            string currentTransformation = playerDataManager.GetCurrentTankTransformation();
            DebugLog($"PlayerDataManager transformation: {currentTransformation}");

            // Apply hardcoded scaling based on transformation
            switch (currentTransformation.ToLower())
            {
                case "heavy":
                    DebugLog("Applying Heavy bullet scale: 1.5");
                    return 1.5f;
                case "rapid":
                    DebugLog("Applying Rapid bullet scale: 0.7");
                    return 0.7f;
                case "balanced":
                    DebugLog("Applying Balanced bullet scale: 1.0");
                    return 1.0f;
                case "armorpiercing":
                    DebugLog("Applying ArmorPiercing bullet scale: 1.8");
                    return 1.8f;
                case "superheavy":
                    DebugLog("Applying SuperHeavy bullet scale: 2.0");
                    return 2.0f;
                case "burst":
                    DebugLog("Applying Burst bullet scale: 0.6");
                    return 0.6f;
                case "machinegun":
                    DebugLog("Applying MachineGun bullet scale: 0.5");
                    return 0.5f;
                case "tactical":
                    DebugLog("Applying Tactical bullet scale: 1.2");
                    return 1.2f;
                case "versatile":
                    DebugLog("Applying Versatile bullet scale: 1.0");
                    return 1.0f;
                case "basic":
                    DebugLog("Applying Basic bullet scale: 1.0");
                    return 1.0f;
                default:
                    DebugLog($"Unknown transformation '{currentTransformation}', checking upgrade system...");
                    break;
            }
        }
        else
        {
            DebugLog("⚠️ PlayerDataManager not found, checking upgrade system...");
        }

        // PRIORITY 3: Try to get bullet size multiplier from TankUpgradeSystem
        var tankUpgradeSystem = FindFirstObjectByType<WheelUpgradeSystem.TankUpgradeSystem>();
        if (tankUpgradeSystem != null)
        {
            DebugLog("✅ Found TankUpgradeSystem");
            var currentOption = tankUpgradeSystem.GetCurrentUpgradeOption();
            if (currentOption != null)
            {
                DebugLog($"✅ Found upgrade option: {currentOption.upgradeName}");
                DebugLog($"✅ Applied bullet scale from upgrade system: {currentOption.bulletSizeMultiplier}");
                return currentOption.bulletSizeMultiplier;
            }
            else
            {
                DebugLog("⚠️ TankUpgradeSystem found but no current upgrade option");
            }
        }
        else
        {
            DebugLog("⚠️ TankUpgradeSystem not found");
        }

        DebugLog("🔧 Using default bullet scale: 1.0");
        return 1f; // Default scale
    }

    /// <summary>
    /// **NEW: Configure bullet damage based on upgrade system
    /// </summary>
    /// <summary>
    /// **ENHANCED: Configure bullet script with lifetime and shooter
    /// </summary>
    private void ConfigureBulletDamage(GameObject bullet, float bulletScale)
    {
        // Try to find bullet script and set basic properties
        var bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetLifetime(bulletLifetime);
            bulletScript.SetShooter(gameObject);
            DebugLog("Applied bullet lifetime and shooter");
        }
        else
        {
            // If no Bullet script, just set lifetime via Destroy
            DebugLog("No Bullet script found, using Destroy for lifetime");
        }
    }

    /// <summary>
    /// Debug logging with toggle
    /// </summary>
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[TankShooting] {message}");
        }
    }

    // NEW: Context menu methods for testing
    [ContextMenu("Test Fire All Turrets")]
    public void TestFireAllTurrets()
    {
        if (CanShoot())
        {
            FireFromAllTurrets();
        }
        else
        {
            DebugLog("Cannot shoot right now - check fire rate or bullet prefab");
        }
    }

    [ContextMenu("Reinitialize Fire Points")]
    public void TestReinitializeFirePoints()
    {
        RefreshFirePoints();
    }

    [ContextMenu("Show Fire Point Status")]
    public void ShowFirePointStatus()
    {
        DebugLog("=== FIRE POINT STATUS ===");
        DebugLog($"Total FirePoints: {allFirePoints.Count}");
        DebugLog($"Initialized: {firePointsInitialized}");
        DebugLog($"Bullet Prefab: {(bulletPrefab != null ? "✓" : "✗")}");
        DebugLog($"Bullet Speed: {bulletSpeed}");
        DebugLog($"Fire Rate: {fireRate}");

        for (int i = 0; i < allFirePoints.Count; i++)
        {
            if (allFirePoints[i] != null)
            {
                DebugLog($"  FirePoint {i}: {allFirePoints[i].name} at {allFirePoints[i].position}");
            }
        }
    }
}