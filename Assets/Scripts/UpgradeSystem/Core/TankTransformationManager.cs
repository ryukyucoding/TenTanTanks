using UnityEngine;
using System.Collections.Generic;
using WheelUpgradeSystem;

/// <summary>
/// COMPLETE TANK TRANSFORMATION SYSTEM
/// Connects upgrade wheel choices to visual transformations and stat updates
/// UPDATED: Now uses Huge_T1 prefab for heavy tanks (properly scaled)
/// </summary>
public class TankTransformationManager : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private ModularTankController modularTankController;
    [SerializeField] private TankController tankController;
    [SerializeField] private TankShooting tankShooting;
    [SerializeField] private TankUpgradeSystem tankUpgradeSystem;

    [Header("Tank Prefabs - Tier 1")]
    [SerializeField] private GameObject hugeTier1Prefab; // NEW - Your Huge_T1 prefab
    [SerializeField] private GameObject rapidSinglePrefab;
    [SerializeField] private GameObject balancedDoublePrefab;

    [Header("Tank Prefabs - Tier 2 Heavy")]
    [SerializeField] private GameObject heavyTripleFrontPrefab;
    [SerializeField] private GameObject heavyTripleCirclePrefab;

    [Header("Tank Prefabs - Tier 2 Rapid")]
    [SerializeField] private GameObject rapidTripleFrontPrefab;
    [SerializeField] private GameObject rapidTripleCirclePrefab;

    [Header("Tank Prefabs - Tier 2 Balanced")]
    [SerializeField] private GameObject balancedQuadPrefab;

    [Header("Tank Base References")]
    [SerializeField] private Transform tankBase;
    [SerializeField] private Transform originalTurret;
    [SerializeField] private Renderer[] tankRenderers;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // Runtime variables
    private GameObject currentTurretPrefab;
    private List<Transform> currentFirePoints = new List<Transform>();
    private string currentUpgrade = "Basic";
    private TankConfiguration currentConfig;

    void Start()
    {
        InitializeTransformationSystem();

        // Auto-load saved transformation
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.LoadTankTransformation();
        }
    }

    private void InitializeTransformationSystem()
    {
        DebugLog("=== TANK TRANSFORMATION SYSTEM INITIALIZE ===");

        // Auto-find components if not assigned
        AutoFindComponents();

        // Auto-load prefabs
        AutoLoadPrefabs();

        // Subscribe to upgrade events
        SubscribeToUpgradeEvents();

        DebugLog("Tank Transformation System Ready!");
    }

    private void AutoFindComponents()
    {
        if (modularTankController == null)
            modularTankController = GetComponent<ModularTankController>();
        if (tankController == null)
            tankController = GetComponent<TankController>();
        if (tankShooting == null)
            tankShooting = GetComponent<TankShooting>();
        if (tankUpgradeSystem == null)
            tankUpgradeSystem = FindFirstObjectByType<TankUpgradeSystem>();

        // Find tank base
        if (tankBase == null)
        {
            tankBase = transform.Find("ArmTank");
            if (tankBase != null)
                DebugLog("Auto-found tank base: " + tankBase.name);
        }

        // Find original turret to hide
        if (originalTurret == null && tankBase != null)
        {
            originalTurret = tankBase.Find("Barrel");
            if (originalTurret == null)
                originalTurret = tankBase.Find("Barrel.001");
        }

        // Find renderers
        if (tankRenderers.Length == 0)
        {
            tankRenderers = GetComponentsInChildren<Renderer>();
        }

        DebugLog($"Components found: TankController={tankController != null}, TankShooting={tankShooting != null}, UpgradeSystem={tankUpgradeSystem != null}");
    }

    private void AutoLoadPrefabs()
    {
        // Load from Resources if not assigned
        // UPDATED: Load Huge_T1 instead of Heavy_Single
        if (hugeTier1Prefab == null)
            hugeTier1Prefab = Resources.Load<GameObject>("TankPrefabs/Huge_T1");
        if (rapidSinglePrefab == null)
            rapidSinglePrefab = Resources.Load<GameObject>("TankPrefabs/Rapid_Single");
        if (balancedDoublePrefab == null)
            balancedDoublePrefab = Resources.Load<GameObject>("TankPrefabs/Balanced_Double");

        DebugLog($"Prefabs loaded: Huge_T1={hugeTier1Prefab != null}, Rapid={rapidSinglePrefab != null}, Balanced={balancedDoublePrefab != null}");
    }

    private void SubscribeToUpgradeEvents()
    {
        // This is where we connect to the upgrade wheel system
        // We'll subscribe to upgrade selection events
        DebugLog("Subscribed to upgrade events");
    }

    /// <summary>
    /// MAIN METHOD: Called when user selects an upgrade from the wheel
    /// This is the bridge between wheel selection and tank transformation
    /// </summary>
    public void OnUpgradeSelected(string upgradeName)
    {
        DebugLog($"UPGRADE SELECTED: {upgradeName}");

        // Apply visual transformation
        ApplyVisualTransformation(upgradeName);

        // Apply stat changes
        ApplyStatChanges(upgradeName);

        // Update shooting system
        UpdateShootingSystem();

        currentUpgrade = upgradeName;

        DebugLog($"TRANSFORMATION COMPLETE: {upgradeName}");

        // Save to PlayerDataManager for persistence
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SaveTankTransformation(upgradeName);
        }
    }

    public void ApplyVisualTransformation(string upgradeName)
    {
        Debug.Log($"Applying visual transformation: {upgradeName}");

        // remove previous turret prefab
        if (currentTurretPrefab != null)
        {
            DestroyImmediate(currentTurretPrefab);
            currentTurretPrefab = null;
        }

        // --- HIDE / DESTROY all original turret layers ---
        if (tankBase != null)
        {
            Transform originalTurretObj = tankBase.Find("Turret");
            if (originalTurretObj != null)
            {
                Debug.Log($"[Debug] Found original turret: {originalTurretObj.name}");
                Destroy(originalTurretObj.gameObject);
                Debug.Log($"[Debug] Destroyed original turret object");
            }
            else
            {
                Debug.LogWarning("[Debug] No original Turret object found under tankBase!");
            }
        }
        else
        {
            Debug.LogWarning("[Debug] tankBase is null!");
        }


        // clear fire points
        currentFirePoints.Clear();

        // get tank configuration
        currentConfig = GetTankConfiguration(upgradeName);
        if (currentConfig == null)
        {
            Debug.LogWarning($"No configuration found for {upgradeName}");
            return;
        }

        GameObject prefabToUse = GetPrefabForUpgrade(upgradeName);
        if (prefabToUse == null)
        {
            Debug.LogWarning($"No prefab found for {upgradeName}. Using original turret.");
            return;
        }

        // --- Instantiate new turret ---
        Vector3 spawnPos = tankBase != null ? tankBase.position : Vector3.zero;
        Quaternion spawnRot = tankBase != null ? tankBase.rotation : Quaternion.identity;

        currentTurretPrefab = Instantiate(prefabToUse, spawnPos, spawnRot);
        currentTurretPrefab.name = $"Turret_{upgradeName}";

        if (tankBase != null)
        {
            currentTurretPrefab.transform.SetParent(tankBase, true);
            currentTurretPrefab.transform.localPosition = Vector3.zero;
            currentTurretPrefab.transform.localRotation = Quaternion.identity;
            currentTurretPrefab.transform.localScale = Vector3.one;
        }

        // collect fire points
        currentFirePoints.AddRange(currentTurretPrefab.GetComponentsInChildren<Transform>());
        List<Transform> firePointTransforms = new List<Transform>();
        foreach (var t in currentFirePoints)
        {
            if (t.CompareTag("FirePoint") || t.name.Contains("FirePoint"))
            {
                firePointTransforms.Add(t);
            }
        }
        currentFirePoints = firePointTransforms;

        // update tank shooting and controller
        if (tankController != null)
            tankController.SetTurret(currentTurretPrefab.transform);

        if (tankShooting != null)
            tankShooting.SetFirePoints(currentFirePoints);

        // apply tank color
        ApplyColorChanges(upgradeName);

        Debug.Log($"Visual transformation complete. Fire points: {currentFirePoints.Count}");
    }



    private GameObject GetPrefabForUpgrade(string upgradeName)
    {
        switch (upgradeName.ToLower())
        {
            case "basic":
                return null; // Use original

            // TIER 1 - UPDATED: Heavy now uses Huge_T1
            case "heavy":
                return hugeTier1Prefab; // CHANGED: Uses new properly scaled prefab
            case "rapid":
                return rapidSinglePrefab;
            case "balanced":
                return balancedDoublePrefab;

            // TIER 2 - HEAVY
            case "superheavy_front":
            case "superheavy-front":
                return heavyTripleFrontPrefab;
            case "superheavy_circle":
            case "superheavy-circle":
                return heavyTripleCirclePrefab;

            // TIER 2 - RAPID
            case "rapidfire_front":
            case "rapidfire-front":
                return rapidTripleFrontPrefab;
            case "rapidfire_circle":
            case "rapidfire-circle":
                return rapidTripleCirclePrefab;

            // TIER 2 - BALANCED
            case "versatile":
                return balancedQuadPrefab;

            default:
                DebugLog($"Unknown upgrade name: {upgradeName}");
                return null;
        }
    }

    private void FindFirePointsInConfiguration()
    {
        if (currentTurretPrefab == null) return;

        // Find all FirePoint objects in the prefab
        Transform[] allChildren = currentTurretPrefab.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child.name == "FirePoint" || child.name.Contains("FirePoint"))
            {
                currentFirePoints.Add(child);
                DebugLog($"Found fire point: {child.name}");
            }
        }

        DebugLog($"Total fire points found: {currentFirePoints.Count}");
    }

    private void ApplyStatChanges(string upgradeName)
    {
        if (currentConfig == null) return;

        DebugLog($"Applying stat changes for: {upgradeName}");

        // Apply movement speed
        if (tankController != null)
        {
            float newMoveSpeed = 5f * currentConfig.moveSpeedMultiplier;
            tankController.SetMoveSpeed(newMoveSpeed);
            DebugLog($"Move speed updated: {newMoveSpeed}");
        }

        // Apply fire rate changes
        if (tankShooting != null)
        {
            float newFireRate = currentConfig.fireRateMultiplier;
            tankShooting.SetFireRate(newFireRate);
            DebugLog($"Fire rate updated: {newFireRate}");
        }

        // Apply bullet speed changes
        if (tankShooting != null)
        {
            float newBulletSpeed = 5f * currentConfig.bulletSpeedMultiplier;
            tankShooting.SetBulletSpeed(newBulletSpeed);
            DebugLog($"Bullet speed updated: {newBulletSpeed}");
        }

        // Apply multi-turret shooting changes if available
        MultiTurretShooting multiTurret = GetComponent<MultiTurretShooting>();
        if (multiTurret != null)
        {
            multiTurret.SetFireRate(currentConfig.fireRateMultiplier);
            multiTurret.SetBulletSpeed(5f * currentConfig.bulletSpeedMultiplier);
            DebugLog($"Multi-turret stats updated - Fire Rate: x{currentConfig.fireRateMultiplier}, Bullet Speed: x{currentConfig.bulletSpeedMultiplier}");
        }

        // Note: ModularTankController has GET methods for damage/bullet multipliers, not SET methods
        // The damage multiplier will be applied when bullets are fired
        DebugLog($"Tank configuration applied - Damage will be x{currentConfig.damageMultiplier} when bullets are created");
    }

    private void UpdateShootingSystem()
    {
        DebugLog($"Updating shooting system with {currentFirePoints.Count} fire points");

        if (tankShooting != null && currentFirePoints.Count > 0)
        {
            tankShooting.SetFirePoints(currentFirePoints);
            DebugLog("Fire points updated in TankShooting");
        }

        // Update MultiTurretShooting component if available
        // Note: MultiTurretShooting doesn't have SetFirePoints method, 
        // it gets fire points from modularTank.GetAllFirePoints()
        MultiTurretShooting multiTurret = GetComponent<MultiTurretShooting>();
        if (multiTurret != null)
        {
            DebugLog("MultiTurretShooting found - it will auto-update fire points from ModularTankController");
        }

        // Update modular tank controller with current turret if available
        if (modularTankController != null && currentTurretPrefab != null)
        {
            DebugLog("ModularTankController will handle fire point detection automatically");
        }
    }

    private void ApplyColorChanges(string upgradeName)
    {
        if (currentConfig == null) return;

        foreach (Renderer renderer in tankRenderers)
        {
            if (renderer == null) continue;

            // skip original turret¡¦s renderer
            if (originalTurret != null && renderer.transform.IsChildOf(originalTurret))
                continue;

            renderer.material.color = currentConfig.tankColor;
        }

        DebugLog($"Tank color updated to: {currentConfig.tankColor}");
    }

    private TankConfiguration GetTankConfiguration(string upgradeName)
    {
        switch (upgradeName.ToLower())
        {
            case "basic":
                return new TankConfiguration
                {
                    upgradeName = "Basic",
                    tankColor = Color.white,
                    moveSpeedMultiplier = 1f,
                    fireRateMultiplier = 1f,
                    bulletSpeedMultiplier = 1f,
                    damageMultiplier = 1f
                };

            case "heavy":
                return new TankConfiguration
                {
                    upgradeName = "Heavy",
                    tankColor = new Color(0.8f, 0.2f, 0.2f), // Red
                    moveSpeedMultiplier = 0.7f,    // 30% slower
                    fireRateMultiplier = 0.8f,     // 20% slower firing
                    bulletSpeedMultiplier = 0.9f,  // 10% slower bullets
                    damageMultiplier = 1.5f        // 50% more damage
                };

            case "rapid":
                return new TankConfiguration
                {
                    upgradeName = "Rapid",
                    tankColor = new Color(1f, 0.8f, 0.2f), // Orange
                    moveSpeedMultiplier = 1.3f,    // 30% faster
                    fireRateMultiplier = 1.8f,     // 80% faster firing
                    bulletSpeedMultiplier = 1.4f, // 40% faster bullets
                    damageMultiplier = 0.6f       // 40% less damage
                };

            case "balanced":
                return new TankConfiguration
                {
                    upgradeName = "Balanced",
                    tankColor = new Color(0.3f, 0.7f, 1f), // Light blue
                    moveSpeedMultiplier = 1f,     // Same speed
                    fireRateMultiplier = 1.3f,    // 30% faster firing
                    bulletSpeedMultiplier = 1f,   // Same speed
                    damageMultiplier = 1f         // Same damage
                };

            // Add more configurations as needed...

            default:
                DebugLog($"No configuration for: {upgradeName}");
                return null;
        }
    }

    // PUBLIC METHODS FOR UPGRADE WHEEL INTEGRATION

    /// <summary>
    /// Called by upgrade wheel when user selects Heavy
    /// </summary>
    public void SelectHeavyUpgrade()
    {
        OnUpgradeSelected("Heavy");
    }

    /// <summary>
    /// Called by upgrade wheel when user selects Rapid
    /// </summary>
    public void SelectRapidUpgrade()
    {
        OnUpgradeSelected("Rapid");
    }

    /// <summary>
    /// Called by upgrade wheel when user selects Balanced
    /// </summary>
    public void SelectBalancedUpgrade()
    {
        OnUpgradeSelected("Balanced");
    }

    // CONTEXT MENU TESTING
    [ContextMenu("Test Heavy Transformation")]
    public void TestHeavy()
    {
        OnUpgradeSelected("Heavy");
    }

    [ContextMenu("Test Rapid Transformation")]
    public void TestRapid()
    {
        OnUpgradeSelected("Rapid");
    }

    [ContextMenu("Test Balanced Transformation")]
    public void TestBalanced()
    {
        OnUpgradeSelected("Balanced");
    }

    [ContextMenu("Reset to Basic")]
    public void ResetToBasic()
    {
        OnUpgradeSelected("Basic");
    }

    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        DebugLog("=== CURRENT TRANSFORMATION STATE ===");
        DebugLog($"Current Upgrade: {currentUpgrade}");
        DebugLog($"Current Prefab: {(currentTurretPrefab != null ? currentTurretPrefab.name : "None")}");
        DebugLog($"Fire Points: {currentFirePoints.Count}");
        if (currentConfig != null)
        {
            DebugLog($"Move Speed: x{currentConfig.moveSpeedMultiplier}");
            DebugLog($"Fire Rate: x{currentConfig.fireRateMultiplier}");
            DebugLog($"Damage: x{currentConfig.damageMultiplier}");
            DebugLog($"Color: {currentConfig.tankColor}");
        }
    }

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[TankTransformation] {message}");
        }
    }
}

/// <summary>
/// Tank configuration data
/// </summary>
[System.Serializable]
public class TankConfiguration
{
    public string upgradeName;
    public Color tankColor = Color.white;
    public float moveSpeedMultiplier = 1f;
    public float fireRateMultiplier = 1f;
    public float bulletSpeedMultiplier = 1f;
    public float damageMultiplier = 1f;
}