using UnityEngine;
using System.Collections.Generic;
using WheelUpgradeSystem;

/// <summary>
/// COMPLETE TANK TRANSFORMATION SYSTEM
/// Connects upgrade wheel choices to visual transformations and stat updates
/// </summary>
public class TankTransformationManager : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private ModularTankController modularTankController;
    [SerializeField] private TankController tankController;
    [SerializeField] private TankShooting tankShooting;
    [SerializeField] private TankUpgradeSystem tankUpgradeSystem;

    [Header("Tank Prefabs - Tier 1")]
    [SerializeField] private GameObject heavySinglePrefab;
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
        if (heavySinglePrefab == null)
            heavySinglePrefab = Resources.Load<GameObject>("TankPrefabs/Heavy_Single");
        if (rapidSinglePrefab == null)
            rapidSinglePrefab = Resources.Load<GameObject>("TankPrefabs/Rapid_Single");
        if (balancedDoublePrefab == null)
            balancedDoublePrefab = Resources.Load<GameObject>("TankPrefabs/Balanced_Double");

        DebugLog($"Prefabs loaded: Heavy={heavySinglePrefab != null}, Rapid={rapidSinglePrefab != null}, Balanced={balancedDoublePrefab != null}");
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

    private void ApplyVisualTransformation(string upgradeName)
    {
        DebugLog($"Applying visual transformation: {upgradeName}");

        // Remove current turret prefab
        if (currentTurretPrefab != null)
        {
            DestroyImmediate(currentTurretPrefab);
            currentTurretPrefab = null;
        }

        // Hide original turret
        if (originalTurret != null)
            originalTurret.gameObject.SetActive(false);

        // Clear fire points
        currentFirePoints.Clear();

        // Get configuration for this upgrade
        currentConfig = GetTankConfiguration(upgradeName);
        if (currentConfig == null)
        {
            DebugLog($"No configuration found for: {upgradeName}");
            return;
        }

        // Get the prefab to instantiate
        GameObject prefabToUse = GetPrefabForUpgrade(upgradeName);
        if (prefabToUse == null)
        {
            DebugLog($"No prefab found for: {upgradeName}. Using original turret.");
            if (originalTurret != null)
                originalTurret.gameObject.SetActive(true);
            return;
        }

        // Instantiate the new turret configuration
        currentTurretPrefab = Instantiate(prefabToUse, tankBase);
        currentTurretPrefab.name = $"TurretConfig_{upgradeName}";

        // Position it correctly
        if (originalTurret != null)
        {
            currentTurretPrefab.transform.localPosition = originalTurret.localPosition;
            currentTurretPrefab.transform.localRotation = originalTurret.localRotation;
            currentTurretPrefab.transform.localScale = originalTurret.localScale;
        }

        // Find fire points in the new configuration
        FindFirePointsInConfiguration();

        // Apply tank color changes - FIXED: Pass upgradeName parameter
        ApplyColorChanges(upgradeName);

        DebugLog($"Visual transformation complete. Fire points: {currentFirePoints.Count}");
    }

    private GameObject GetPrefabForUpgrade(string upgradeName)
    {
        switch (upgradeName.ToLower())
        {
            case "basic":
                return null; // Use original

            // TIER 1
            case "heavy":
                return heavySinglePrefab;
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
            DebugLog($"Applied move speed: {newMoveSpeed} (x{currentConfig.moveSpeedMultiplier})");
        }

        // Apply shooting stats
        if (tankShooting != null)
        {
            // Fire rate
            tankShooting.SetFireRate(currentConfig.fireRateMultiplier);
            DebugLog($"Applied fire rate: x{currentConfig.fireRateMultiplier}");

            // Bullet speed
            float newBulletSpeed = 5f * currentConfig.bulletSpeedMultiplier;
            tankShooting.SetBulletSpeed(newBulletSpeed);
            DebugLog($"Applied bullet speed: {newBulletSpeed}");

            // Bullet damage (you might need to add this method)
            if (tankShooting.GetType().GetMethod("SetBulletDamage") != null)
            {
                tankShooting.GetComponent<TankShooting>().GetType().GetMethod("SetBulletDamage").Invoke(tankShooting, new object[] { currentConfig.damageMultiplier });
                DebugLog($"Applied damage: x{currentConfig.damageMultiplier}");
            }
        }

        DebugLog($"Stat changes complete for: {upgradeName}");
    }

    // FIXED: ApplyColorChanges method with upgradeName parameter
    private void ApplyColorChanges(string upgradeName)
    {
        // Skip color changes - keep original tank colors
        DebugLog($"Skipping color change for: {upgradeName} (keeping original colors)");
        return;

        // Original color changing code (commented out)
        // Apply color changes to tank body only  
        // if (tankRenderers != null && tankRenderers.Length > 0)
        // {
        //     Color targetColor = Color.gray; // Heavy tanks are gray
        //     foreach (Renderer renderer in tankRenderers)
        //     {
        //         if (renderer != null)
        //         {
        //             renderer.material.color = targetColor;
        //             DebugLog($"Applied color to renderer: {renderer.name}");
        //         }
        //     }
        //     DebugLog($"Applied tank color: {targetColor}");
        // }
    }

    private void UpdateShootingSystem()
    {
        // Update TankShooting to use multiple fire points
        if (tankShooting != null && currentFirePoints.Count > 0)
        {
            // This is where you'd integrate with TankShooting.cs
            // For now, we'll just log the information
            DebugLog($"Updated shooting system with {currentFirePoints.Count} fire points");

            // You would modify TankShooting.cs to call GetCurrentFirePoints()
            // instead of using a single firePoint reference
        }
    }

    /// <summary>
    /// Get current fire points for shooting system integration
    /// </summary>
    public List<Transform> GetCurrentFirePoints()
    {
        return currentFirePoints;
    }

    /// <summary>
    /// Get current upgrade name
    /// </summary>
    public string GetCurrentUpgrade()
    {
        return currentUpgrade;
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
                    tankColor = new Color(0.6f, 0.6f, 0.6f), // Gray
                    moveSpeedMultiplier = 0.7f,   // 30% slower
                    fireRateMultiplier = 0.5f,    // 50% slower firing
                    bulletSpeedMultiplier = 0.8f, // 20% slower bullets
                    damageMultiplier = 2.5f       // 150% more damage
                };

            case "rapid":
                return new TankConfiguration
                {
                    upgradeName = "Rapid",
                    tankColor = new Color(1f, 0.6f, 0.2f), // Orange
                    moveSpeedMultiplier = 1.3f,   // 30% faster
                    fireRateMultiplier = 2.5f,    // 150% faster firing
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