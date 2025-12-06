using UnityEngine;
using System.Collections.Generic;
using WheelUpgradeSystem;

/// <summary>
/// FIXED TANK TRANSFORMATION SYSTEM
/// 修復：新砲管現在會完全取代舊砲管，而不是作為兄弟物件
/// </summary>
public class TankTransformationManager : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private ModularTankController modularTankController;
    [SerializeField] private TankController tankController;
    [SerializeField] private TankShooting tankShooting;
    [SerializeField] private TankUpgradeSystem tankUpgradeSystem;

    [Header("Tank Prefabs - Tier 1")]
    [SerializeField] private GameObject hugeTier1Prefab;
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

    // FIXED: Store original turret data for proper replacement
    private Vector3 originalTurretPosition;
    private Quaternion originalTurretRotation;
    private Vector3 originalTurretScale;
    private Transform originalTurretParent;

    void Start()
    {
        InitializeTransformationSystem();
        LoadSavedTransformation();
    }

    private void InitializeTransformationSystem()
    {
        DebugLog("=== TANK TRANSFORMATION SYSTEM INITIALIZE ===");

        AutoFindComponents();
        AutoLoadPrefabs();
        SubscribeToUpgradeEvents();

        // FIXED: Store original turret information
        StoreOriginalTurretInfo();

        DebugLog("Tank Transformation System Ready!");
    }

    /// <summary>
    /// FIXED: Store original turret position/rotation/scale for proper replacement
    /// </summary>
    private void StoreOriginalTurretInfo()
    {
        if (originalTurret != null)
        {
            originalTurretPosition = originalTurret.localPosition;
            originalTurretRotation = originalTurret.localRotation;
            originalTurretScale = originalTurret.localScale;
            originalTurretParent = originalTurret.parent;

            DebugLog($"Stored original turret info: {originalTurret.name} at {originalTurretPosition}");
        }
        else
        {
            DebugLog("WARNING: No original turret found to store info!");
        }
    }

    private void LoadSavedTransformation()
    {
        if (PlayerDataManager.Instance == null)
        {
            DebugLog("No PlayerDataManager found - keeping Basic tank");
            return;
        }

        string savedTransformation = PlayerDataManager.Instance.GetCurrentTankTransformation();

        if (string.IsNullOrEmpty(savedTransformation) || savedTransformation == "Basic")
        {
            DebugLog($"No saved transformation or Basic tank - keeping original");
            return;
        }

        DebugLog($"Loading saved tank transformation: {savedTransformation}");
        OnUpgradeSelected(savedTransformation);
        DebugLog($"Successfully applied saved transformation: {savedTransformation}");
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

        // FIXED: Find the parent Turret object, not individual barrels
        if (originalTurret == null && tankBase != null)
        {
            // Priority 1: Look for parent "Turret" object
            originalTurret = tankBase.Find("Turret");
            if (originalTurret != null)
            {
                DebugLog("Found original Turret parent: " + originalTurret.name);
            }
            else
            {
                // Priority 2: Look for "Barrel" objects
                originalTurret = tankBase.Find("Barrel");
                if (originalTurret == null)
                    originalTurret = tankBase.Find("Barrel.001");

                if (originalTurret != null)
                {
                    DebugLog("Found original turret barrel: " + originalTurret.name);
                }
                else
                {
                    DebugLog("WARNING: No original turret found!");
                }
            }
        }

        // Find renderers
        if (tankRenderers.Length == 0)
        {
            tankRenderers = GetComponentsInChildren<Renderer>();
        }

        DebugLog($"Components found: TankController={tankController != null}, TankShooting={tankShooting != null}, UpgradeSystem={tankUpgradeSystem != null}");
        DebugLog($"Original turret found: {(originalTurret != null ? originalTurret.name : "None")}");
    }

    private void AutoLoadPrefabs()
    {
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
        DebugLog("Subscribed to upgrade events");
    }

    public void OnUpgradeSelected(string upgradeName)
    {
        DebugLog($"UPGRADE SELECTED: {upgradeName}");

        ApplyVisualTransformation(upgradeName);
        ApplyStatChanges(upgradeName);
        UpdateShootingSystem();

        currentUpgrade = upgradeName;
        DebugLog($"TRANSFORMATION COMPLETE: {upgradeName}");

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SaveTankTransformation(upgradeName);
        }
    }

    /// <summary>
    /// FIXED: 完全取代砲管而不是添加兄弟物件
    /// </summary>
    public void ApplyVisualTransformation(string upgradeName)
    {
        Debug.Log($"Applying visual transformation: {upgradeName}");

        // Remove previous turret prefab
        if (currentTurretPrefab != null)
        {
            DebugLog("Destroying previous turret prefab");
            DestroyImmediate(currentTurretPrefab);
            currentTurretPrefab = null;
        }

        // Clear fire points
        currentFirePoints.Clear();

        // Get configuration and prefab
        currentConfig = GetTankConfiguration(upgradeName);
        if (currentConfig == null)
        {
            Debug.LogWarning($"No configuration found for {upgradeName}");
            return;
        }

        GameObject prefabToUse = GetPrefabForUpgrade(upgradeName);
        if (prefabToUse == null)
        {
            Debug.LogWarning($"No prefab found for {upgradeName}");
            return;
        }

        // FIXED: Complete turret replacement logic
        if (upgradeName.ToLower() == "basic")
        {
            // Restore original turret
            if (originalTurret != null)
            {
                originalTurret.gameObject.SetActive(true);
                DebugLog("Restored original turret");
            }
        }
        else
        {
            // FIXED: Completely destroy and replace original turret
            if (originalTurret != null && originalTurretParent != null)
            {
                DebugLog($"Completely destroying original turret: {originalTurret.name}");

                // Store position/rotation before destroying
                Vector3 replacePosition = originalTurret.localPosition;
                Quaternion replaceRotation = originalTurret.localRotation;
                Transform parent = originalTurret.parent;

                // Destroy the original turret completely
                DestroyImmediate(originalTurret.gameObject);
                originalTurret = null;  // Clear reference

                // Create new turret prefab
                currentTurretPrefab = Instantiate(prefabToUse);
                currentTurretPrefab.name = "Turret";  // FIXED: Name it "Turret" to replace original

                // Position it in the same place as original
                currentTurretPrefab.transform.SetParent(parent, false);
                currentTurretPrefab.transform.localPosition = replacePosition;
                currentTurretPrefab.transform.localRotation = replaceRotation;
                currentTurretPrefab.transform.localScale = Vector3.one;

                DebugLog($"New turret '{currentTurretPrefab.name}' created at position: {replacePosition}");

                // Update references to point to the new turret
                originalTurret = currentTurretPrefab.transform;
            }
        }

        // Find fire points in the new turret
        if (currentTurretPrefab != null)
        {
            Transform[] allTransforms = currentTurretPrefab.GetComponentsInChildren<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.CompareTag("FirePoint") || t.name.Contains("FirePoint"))
                {
                    currentFirePoints.Add(t);
                    DebugLog($"Found fire point: {t.name}");
                }
            }
        }

        // Update tank controller turret reference
        if (tankController != null && currentTurretPrefab != null)
        {
            tankController.SetTurret(currentTurretPrefab.transform);
            DebugLog("Updated TankController turret reference");
        }

        // Update shooting system
        if (tankShooting != null)
        {
            tankShooting.SetFirePoints(currentFirePoints);
            DebugLog("Updated TankShooting fire points");
        }

        // Apply color changes
        ApplyColorChanges(upgradeName);

        Debug.Log($"Visual transformation complete for: {upgradeName}");
    }

    private GameObject GetPrefabForUpgrade(string upgradeName)
    {
        switch (upgradeName.ToLower())
        {
            case "heavy":
                return hugeTier1Prefab;
            case "rapid":
                return rapidSinglePrefab;
            case "balanced":
                return balancedDoublePrefab;
            default:
                return null;
        }
    }

    private void ApplyColorChanges(string upgradeName)
    {
        if (currentConfig == null || tankRenderers == null) return;

        Color newColor = currentConfig.tankColor;

        foreach (Renderer renderer in tankRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                // Skip renderers that are part of the new turret (they keep their own colors)
                if (currentTurretPrefab != null &&
                    renderer.transform.IsChildOf(currentTurretPrefab.transform))
                {
                    continue;
                }

                renderer.material.color = newColor;
            }
        }

        DebugLog($"Applied color: {newColor} to tank body");
    }

    private void ApplyStatChanges(string upgradeName)
    {
        if (currentConfig == null) return;

        DebugLog($"Applying stat changes for: {upgradeName}");

        if (tankController != null)
        {
            float newMoveSpeed = 5f * currentConfig.moveSpeedMultiplier;
            tankController.SetMoveSpeed(newMoveSpeed);
            DebugLog($"Move speed updated: {newMoveSpeed}");
        }

        if (tankShooting != null)
        {
            float newFireRate = currentConfig.fireRateMultiplier;
            tankShooting.SetFireRate(newFireRate);
            DebugLog($"Fire rate updated: {newFireRate}");

            float newBulletSpeed = 5f * currentConfig.bulletSpeedMultiplier;
            tankShooting.SetBulletSpeed(newBulletSpeed);
            DebugLog($"Bullet speed updated: {newBulletSpeed}");
        }

        MultiTurretShooting multiTurret = GetComponent<MultiTurretShooting>();
        if (multiTurret != null)
        {
            multiTurret.SetFireRate(currentConfig.fireRateMultiplier);
            multiTurret.SetBulletSpeed(5f * currentConfig.bulletSpeedMultiplier);
            DebugLog($"Multi-turret stats updated");
        }

        DebugLog($"Tank configuration applied - Damage will be x{currentConfig.damageMultiplier}");
    }

    private void UpdateShootingSystem()
    {
        DebugLog($"Updating shooting system with {currentFirePoints.Count} fire points");

        if (tankShooting != null && currentFirePoints.Count > 0)
        {
            tankShooting.SetFirePoints(currentFirePoints);
            DebugLog("Fire points updated in TankShooting");
        }

        MultiTurretShooting multiTurret = GetComponent<MultiTurretShooting>();
        if (multiTurret != null)
        {
            DebugLog("MultiTurretShooting component found - it will get fire points automatically");
        }
    }

    private TankConfiguration GetTankConfiguration(string upgradeName)
    {
        switch (upgradeName.ToLower())
        {
            case "heavy":
                return new TankConfiguration
                {
                    upgradeName = "Heavy",
                    tankColor = new Color(0.8f, 0.2f, 0.2f),
                    moveSpeedMultiplier = 0.7f,
                    fireRateMultiplier = 0.8f,
                    bulletSpeedMultiplier = 0.8f,
                    damageMultiplier = 1.5f
                };

            case "rapid":
                return new TankConfiguration
                {
                    upgradeName = "Rapid",
                    tankColor = new Color(1f, 0.8f, 0.2f),
                    moveSpeedMultiplier = 1.2f,
                    fireRateMultiplier = 1.8f,
                    bulletSpeedMultiplier = 1.4f,
                    damageMultiplier = 0.6f
                };

            case "balanced":
                return new TankConfiguration
                {
                    upgradeName = "Balanced",
                    tankColor = new Color(0.3f, 0.7f, 1f),
                    moveSpeedMultiplier = 1f,
                    fireRateMultiplier = 1.3f,
                    bulletSpeedMultiplier = 1f,
                    damageMultiplier = 1f
                };

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

            default:
                DebugLog($"No configuration for: {upgradeName}");
                return null;
        }
    }

    // PUBLIC METHODS FOR UPGRADE WHEEL
    public void SelectHeavyUpgrade()
    {
        OnUpgradeSelected("Heavy");
    }

    public void SelectRapidUpgrade()
    {
        OnUpgradeSelected("Rapid");
    }

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

        // Debug hierarchy
        DebugLog("=== CURRENT HIERARCHY ===");
        if (tankBase != null)
        {
            foreach (Transform child in tankBase)
            {
                DebugLog($"Child: {child.name} (active: {child.gameObject.activeSelf})");
            }
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