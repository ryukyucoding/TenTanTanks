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
            // Use default values if no original turret found
            originalTurretPosition = Vector3.zero;
            originalTurretRotation = Quaternion.identity;
            originalTurretScale = Vector3.one;
            originalTurretParent = transform; // PlayerTank level

            DebugLog("No original turret found - using default position for new turret");
        }

        // FIXED: Force hide old turret components that might show up as white
        if (tankBase != null)
        {
            DebugLog("=== FORCE HIDING OLD TURRET COMPONENTS ===");
            string[] oldComponentNames = { "Barrel.001", "Barrel", "Turret", "FirePoint" };

            foreach (string compName in oldComponentNames)
            {
                Transform oldComp = tankBase.Find(compName);
                if (oldComp != null)
                {
                    DebugLog($"Force hiding old component: {oldComp.name}");
                    oldComp.gameObject.SetActive(false);

                    // Disable all renderers
                    Renderer[] renderers = oldComp.GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in renderers)
                    {
                        if (r != null)
                        {
                            r.enabled = false;
                            DebugLog($"Disabled renderer: {r.name}");
                        }
                    }
                }
            }
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

        // FIXED: Look for Turret at PlayerTank level, NOT ArmTank level
        if (originalTurret == null)
        {
            DebugLog("=== SEARCHING FOR TURRET AT PLAYERTANK LEVEL ===");
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                DebugLog($"PlayerTank Child {i}: {child.name} (active: {child.gameObject.activeSelf})");
                if (child.name == "Turret")
                {
                    originalTurret = child;
                    DebugLog($"Found original Turret at PlayerTank level: {originalTurret.name}");
                    break;
                }
            }

            // Fallback: look in ArmTank for individual barrel components
            if (originalTurret == null && tankBase != null)
            {
                DebugLog("No Turret found at PlayerTank level, looking for barrels in ArmTank...");
                originalTurret = tankBase.Find("Barrel");
                if (originalTurret == null)
                    originalTurret = tankBase.Find("Barrel.001");

                if (originalTurret != null)
                {
                    DebugLog("Found barrel component as fallback: " + originalTurret.name);
                }
                else
                {
                    DebugLog("WARNING: No original turret found anywhere!");
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
    /// FIXED: 正確處理PlayerTank層級的Turret替換
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

        // FIXED: Look for Turret at PlayerTank level, not ArmTank level
        DebugLog("=== BEFORE CLEANING - PLAYERTANK LEVEL ===");
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            DebugLog($"PlayerTank Child {i}: {child.name}");
        }

        // Find and destroy ALL Turret objects at PlayerTank level
        List<Transform> turretsToDestroy = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name == "Turret")
            {
                turretsToDestroy.Add(child);
                DebugLog($"Marked PlayerTank Turret for destruction: {child.name}");
            }
        }

        // Destroy all found turrets at PlayerTank level
        foreach (Transform turret in turretsToDestroy)
        {
            DebugLog($"DESTROYING PlayerTank Turret: {turret.name}");
            DestroyImmediate(turret.gameObject);
        }

        DebugLog($"Destroyed {turretsToDestroy.Count} turret(s) from PlayerTank level");

        // Now create the new turret (only if not Basic)
        if (upgradeName.ToLower() != "basic")
        {
            // Create new turret prefab
            currentTurretPrefab = Instantiate(prefabToUse);
            currentTurretPrefab.name = "Turret";  // Name it "Turret"

            // FIXED: Place it at PlayerTank level, same as original
            currentTurretPrefab.transform.SetParent(transform, false); // PlayerTank as parent
            currentTurretPrefab.transform.localPosition = originalTurretPosition;
            currentTurretPrefab.transform.localRotation = originalTurretRotation;
            currentTurretPrefab.transform.localScale = Vector3.one;

            DebugLog($"New turret '{currentTurretPrefab.name}' created at PlayerTank level at position: {originalTurretPosition}");

            // Update reference
            originalTurret = currentTurretPrefab.transform;

            DebugLog("=== AFTER CREATING NEW TURRET - PLAYERTANK LEVEL ===");
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                DebugLog($"PlayerTank Child {i}: {child.name}");
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

        DebugLog("=== APPLYING COLOR CHANGES ===");
        DebugLog($"Target color: {newColor}");
        DebugLog($"Total renderers found: {tankRenderers.Length}");

        // FIXED: Also disable old turret components in ArmTank to prevent white coloring
        if (tankBase != null)
        {
            // Hide/disable old turret components that might still be getting colored
            Transform[] oldComponents = {
                tankBase.Find("Barrel.001"),
                tankBase.Find("Barrel"),
                tankBase.Find("Turret")
            };

            foreach (Transform comp in oldComponents)
            {
                if (comp != null)
                {
                    DebugLog($"Disabling old component: {comp.name}");
                    comp.gameObject.SetActive(false);

                    // Also disable all renderers on this component
                    Renderer[] oldRenderers = comp.GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in oldRenderers)
                    {
                        if (r != null)
                        {
                            r.enabled = false;
                            DebugLog($"Disabled renderer on: {r.name}");
                        }
                    }
                }
            }
        }

        foreach (Renderer renderer in tankRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                // Skip renderers that are part of the new turret (they keep their own colors)
                if (currentTurretPrefab != null &&
                    renderer.transform.IsChildOf(currentTurretPrefab.transform))
                {
                    DebugLog($"Skipping new turret renderer: {renderer.name}");
                    continue;
                }

                // Skip disabled renderers (old turret components)
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    DebugLog($"Skipping disabled/inactive renderer: {renderer.name}");
                    continue;
                }

                DebugLog($"Applying color to: {renderer.name} (was: {renderer.material.color})");
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