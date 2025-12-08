using UnityEngine;
using System.Collections.Generic;
using WheelUpgradeSystem;

/// <summary>
/// FIXED TANK TRANSFORMATION SYSTEM - NOW WITH COMPLETE TIER 2 SUPPORT
/// FIXED TURRETS
/// ENHANCED: Added complete Tier 2 upgrade configurations following Tier 1 pattern
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
    [SerializeField] private GameObject smallTier1Prefab;
    [SerializeField] private GameObject balancedTier1Prefab;

    [Header("Tank Prefabs - Tier 2 Heavy")]
    [SerializeField] private GameObject hugeTier2FrontPrefab;
    [SerializeField] private GameObject hugeTier2AroundPrefab;

    [Header("Tank Prefabs - Tier 2 Rapid")]
    [SerializeField] private GameObject smallTier2FrontPrefab;
    [SerializeField] private GameObject smallTier2AroundPrefab;

    [Header("Tank Prefabs - Tier 2 Balanced")]
    [SerializeField] private GameObject balancedTier2FrontPrefab;
    [SerializeField] private GameObject balancedTier2AroundPrefab;

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

        // ✅ 确保 PlayerDataManager 存在
        EnsurePlayerDataManagerExists();

        AutoFindComponents();
        AutoLoadPrefabs();
        SubscribeToUpgradeEvents();

        // FIXED: Store original turret information
        StoreOriginalTurretInfo();

        DebugLog("Tank Transformation System Ready!");
    }

    /// <summary>
    /// 确保 PlayerDataManager 存在，如果不存在则创建
    /// </summary>
    private void EnsurePlayerDataManagerExists()
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("[TankTransformationManager] PlayerDataManager 不存在，尝试查找或创建...");
            
            // 尝试在场景中查找
            var existing = FindFirstObjectByType<PlayerDataManager>();
            if (existing == null)
            {
                Debug.LogWarning("[TankTransformationManager] 场景中没有 PlayerDataManager，创建新实例");
                GameObject pdmObject = new GameObject("PlayerDataManager");
                pdmObject.AddComponent<PlayerDataManager>();
            }
            else
            {
                Debug.Log("[TankTransformationManager] 找到现有 PlayerDataManager");
            }
        }
        else
        {
            Debug.Log("[TankTransformationManager] PlayerDataManager 已存在");
        }
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

        DebugLog($"✅ Found saved transformation: {savedTransformation}");
        DebugLog("Applying saved transformation in 0.5 seconds...");
        
        // ✅ 延遲應用變形，確保所有組件都已初始化
        StartCoroutine(ApplyTransformationDelayed(savedTransformation, 0.5f));
    }

    /// <summary>
    /// 延遲應用變形，確保所有組件就緒
    /// </summary>
    private System.Collections.IEnumerator ApplyTransformationDelayed(string transformationName, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Debug.Log($"[TankTransformationManager] Applying delayed transformation: {transformationName}");
        OnUpgradeSelected(transformationName);
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
        if (smallTier1Prefab == null)
            smallTier1Prefab = Resources.Load<GameObject>("TankPrefabs/Small_T1");
        if (balancedTier1Prefab == null)
            balancedTier1Prefab = Resources.Load<GameObject>("TankPrefabs/Balanced_T1");
        if (hugeTier2AroundPrefab == null)
            hugeTier2AroundPrefab = Resources.Load<GameObject>("TankPrefabs/Huge_T2Around");
        if (smallTier2AroundPrefab == null)
            smallTier2AroundPrefab = Resources.Load<GameObject>("TankPrefabs/Small_T2Around");
        if (balancedTier2AroundPrefab == null)
            balancedTier2AroundPrefab = Resources.Load<GameObject>("TankPrefabs/Balanced_T2Around");
        if (hugeTier2FrontPrefab == null)
            hugeTier2FrontPrefab = Resources.Load<GameObject>("TankPrefabs/Huge_T2Front");
        if (smallTier2FrontPrefab == null)
            smallTier2FrontPrefab = Resources.Load<GameObject>("TankPrefabs/Small_T2Front");
        if (balancedTier2FrontPrefab == null)
            balancedTier2FrontPrefab = Resources.Load<GameObject>("TankPrefabs/Balanced_T2Front");

        DebugLog($"Prefabs loaded: Huge_T1={hugeTier1Prefab != null}, Small_T1={smallTier1Prefab != null}, Balanced_T1={balancedTier1Prefab != null}");
        DebugLog($"Tier2 Prefabs: HugeT2Front={hugeTier2FrontPrefab != null}, SmallT2Front={smallTier2FrontPrefab != null}, BalancedT2Front={balancedTier2FrontPrefab != null}");
    }

    private void SubscribeToUpgradeEvents()
    {
        DebugLog("Subscribed to upgrade events");
    }

    public void OnUpgradeSelected(string upgradeName)
    {
        DebugLog($"UPGRADE SELECTED: {upgradeName}");

        // ✅ 变形前确保数据已加载，然后再保存
        TankStats tankStats = GetComponent<TankStats>();
        if (tankStats != null && PlayerDataManager.Instance != null)
        {
            // 先尝试加载保存的数据（如果还没加载的话）
            int currentPoints = tankStats.GetAvailableUpgradePoints();
            int currentMoveLevel = tankStats.GetMoveSpeedLevel();
            
            Debug.Log($"[TankTransformationManager] 变形前检查当前数据: 点数={currentPoints}, 移动Lv={currentMoveLevel}");
            
            // 如果当前数据为空，先加载保存的数据
            if (currentPoints == 0 && currentMoveLevel == 0)
            {
                Debug.Log("[TankTransformationManager] 当前数据为空，尝试加载保存的数据");
                bool loaded = PlayerDataManager.Instance.LoadPlayerStats(tankStats);
                
                if (loaded)
                {
                    Debug.Log($"[TankTransformationManager] 加载成功: 点数={tankStats.GetAvailableUpgradePoints()}, 移动Lv={tankStats.GetMoveSpeedLevel()}");
                }
            }
            
            // 现在保存当前数据
            Debug.Log("[TankTransformationManager] save properties before transformation");
            PlayerDataManager.Instance.SavePlayerStats(tankStats);
        }
        else if (tankStats == null)
        {
            Debug.LogWarning("[TankTransformationManager] ⚠️ TankStats doesn't exist");
        }
        else
        {
            Debug.LogWarning("[TankTransformationManager] ⚠️ PlayerDataManager doesn't exist, can't save data");
        }

        ApplyVisualTransformation(upgradeName);
        ApplyStatChanges(upgradeName);
        UpdateShootingSystem();

        currentUpgrade = upgradeName;
        DebugLog($"TRANSFORMATION COMPLETE: {upgradeName}");

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SaveTankTransformation(upgradeName);
            
            // reload properties after transformation with delay
            if (tankStats != null)
            {
                StartCoroutine(RestoreStatsAfterTransformation(tankStats));
            }
        }
        else
        {
            Debug.LogWarning("[TankTransformationManager] ⚠️ PlayerDataManager doesn't exist, will auto-restore data after scene load");
        }
    }

    /// <summary>
    /// Restore properties after transformation with delay
    /// </summary>
    private System.Collections.IEnumerator RestoreStatsAfterTransformation(TankStats tankStats)
    {
        // Wait a few frames to ensure all systems are initialized
        yield return new WaitForSeconds(0.1f);
        
        if (PlayerDataManager.Instance != null && tankStats != null)
        {
            Debug.Log("[TankTransformationManager] reload properties after transformation");
            PlayerDataManager.Instance.LoadPlayerStats(tankStats);
        }
        else
        {
            Debug.LogWarning("[TankTransformationManager] ⚠️ Can't restore properties: " +
                $"PlayerDataManager={(PlayerDataManager.Instance != null)}, " +
                $"TankStats={(tankStats != null)}");
        }
    }

    public void ApplyVisualTransformation(string upgradeName)
    {
        Debug.Log($"========== APPLYING VISUAL TRANSFORMATION: {upgradeName} ==========");
        Debug.Log($"Current upgrade before change: {currentUpgrade}");

        // ✅ FIX: 更徹底的清理 - 移除所有之前的 turret prefab
        if (currentTurretPrefab != null)
        {
            DebugLog($"Destroying previous turret prefab: {currentTurretPrefab.name}");
            DestroyImmediate(currentTurretPrefab);
            currentTurretPrefab = null;
        }

        // Clear fire points
        currentFirePoints.Clear();

        // Get configuration and prefab
        currentConfig = GetTankConfiguration(upgradeName);
        if (currentConfig == null)
        {
            Debug.LogWarning($"❌ No configuration found for {upgradeName}");
            return;
        }

        GameObject prefabToUse = GetPrefabForUpgrade(upgradeName);
        if (prefabToUse == null)
        {
            Debug.LogWarning($"❌ No prefab found for {upgradeName}");
            Debug.LogWarning($"Available prefabs: Huge_T1={hugeTier1Prefab != null}, Small_T1={smallTier1Prefab != null}, Balanced_T1={balancedTier1Prefab != null}");
            Debug.LogWarning($"Tier2 Heavy: Front={hugeTier2FrontPrefab != null}, Around={hugeTier2AroundPrefab != null}");
            Debug.LogWarning($"Tier2 Rapid: Front={smallTier2FrontPrefab != null}, Around={smallTier2AroundPrefab != null}");
            Debug.LogWarning($"Tier2 Balanced: Front={balancedTier2FrontPrefab != null}, Around={balancedTier2AroundPrefab != null}");
            return;
        }
        
        Debug.Log($"✅ Using prefab: {prefabToUse.name} for upgrade: {upgradeName}");

        // FIXED: Look for Turret at PlayerTank level, not ArmTank level
        DebugLog("=== BEFORE CLEANING - PLAYERTANK LEVEL ===");
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            DebugLog($"PlayerTank Child {i}: {child.name}");
        }

        // ✅ FIX: 更強化的 Turret 清理邏輯
        List<Transform> turretsToDestroy = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            // 清理所有名為 "Turret" 或包含 "Turret" 的物件
            if (child.name == "Turret" || child.name.Contains("Turret"))
            {
                turretsToDestroy.Add(child);
                DebugLog($"Marked Turret for destruction: {child.name}");
            }
        }

        // Destroy all found turrets at PlayerTank level
        foreach (Transform turret in turretsToDestroy)
        {
            DebugLog($"🗑️ DESTROYING Turret: {turret.name}");
            DestroyImmediate(turret.gameObject);
        }

        DebugLog($"✅ Destroyed {turretsToDestroy.Count} turret(s) from PlayerTank level");

        // Now create the new turret (only if not Basic)
        if (upgradeName.ToLower() != "basic")
        {
            DebugLog($"Creating new turret from prefab: {prefabToUse.name}");
            
            // ✅ Create new turret prefab
            currentTurretPrefab = Instantiate(prefabToUse);
            currentTurretPrefab.name = "Turret";  // Name it "Turret"

            // ✅ Place it at PlayerTank level, same as original
            currentTurretPrefab.transform.SetParent(transform, false); // PlayerTank as parent
            currentTurretPrefab.transform.localPosition = originalTurretPosition;
            currentTurretPrefab.transform.localRotation = originalTurretRotation;
            currentTurretPrefab.transform.localScale = Vector3.one;
            
            // ✅ 確保新 Turret 是啟用的
            currentTurretPrefab.SetActive(true);

            DebugLog($"✅ New turret '{currentTurretPrefab.name}' created successfully");
            DebugLog($"   - Position: {originalTurretPosition}");
            DebugLog($"   - Rotation: {originalTurretRotation}");
            DebugLog($"   - Active: {currentTurretPrefab.activeSelf}");
            DebugLog($"   - Parent: {transform.name}");

            // Update reference
            originalTurret = currentTurretPrefab.transform;

            DebugLog("=== AFTER CREATING NEW TURRET - PLAYERTANK CHILDREN ===");
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                DebugLog($"  [{i}] {child.name} (Active: {child.gameObject.activeSelf})");
            }
        }
        else
        {
            DebugLog("Upgrade is 'Basic', skipping turret creation");
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
            // Tier 1 Upgrades
            case "heavy":
                return hugeTier1Prefab;
            case "rapid":
                return smallTier1Prefab;
            case "balanced":
                return balancedTier1Prefab;

            // Tier 2 Heavy Upgrades (from Heavy)
            case "armorpiercing":
                return hugeTier2FrontPrefab;
            case "superheavy":
                return hugeTier2AroundPrefab;

            // Tier 2 Rapid Upgrades (from Rapid)
            case "burst":
                return smallTier2FrontPrefab;
            case "machinegun":
                return smallTier2AroundPrefab;

            // Tier 2 Balanced Upgrades (from Balanced)
            case "tactical":
                return balancedTier2FrontPrefab;
            case "versatile":
                return balancedTier2AroundPrefab;

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

        // ✅ FIX: 獲取 TankStats 以保留升級屬性
        TankStats tankStats = GetComponent<TankStats>();
        float baseSpeed = 2.5f; // 默認基礎速度
        float baseBulletSpeed = 5f; // 默認基礎子彈速度
        float baseFireRate = 1.2f; // 默認基礎射速

        if (tankStats != null)
        {
            // 使用 TankStats 的當前值作為基礎
            baseSpeed = tankStats.GetCurrentMoveSpeed();
            baseBulletSpeed = tankStats.GetCurrentBulletSpeed();
            baseFireRate = tankStats.GetCurrentFireRate();
            DebugLog($"Using TankStats values - Speed: {baseSpeed}, BulletSpeed: {baseBulletSpeed}, FireRate: {baseFireRate}");
        }
        else
        {
            DebugLog("No TankStats found, using default values");
        }

        if (tankController != null)
        {
            // ✅ 使用乘法保留升級屬性
            float newMoveSpeed = baseSpeed * currentConfig.moveSpeedMultiplier;
            tankController.SetMoveSpeed(newMoveSpeed);
            DebugLog($"Move speed updated: {baseSpeed} x {currentConfig.moveSpeedMultiplier} = {newMoveSpeed}");
        }

        if (tankShooting != null)
        {
            // ✅ 使用乘法保留升級屬性
            float newFireRate = baseFireRate * currentConfig.fireRateMultiplier;
            tankShooting.SetFireRate(newFireRate);
            DebugLog($"Fire rate updated: {baseFireRate} x {currentConfig.fireRateMultiplier} = {newFireRate}");

            float newBulletSpeed = baseBulletSpeed * currentConfig.bulletSpeedMultiplier;
            tankShooting.SetBulletSpeed(newBulletSpeed);
            DebugLog($"Bullet speed updated: {baseBulletSpeed} x {currentConfig.bulletSpeedMultiplier} = {newBulletSpeed}");
            
            // 設置子彈大小
            if (currentConfig.bulletScale != 1f)
            {
                tankShooting.SetBulletScale(currentConfig.bulletScale);
                DebugLog($"Bullet scale set to: {currentConfig.bulletScale}");
            }
        }

        MultiTurretShooting multiTurret = GetComponent<MultiTurretShooting>();
        if (multiTurret != null)
        {
            multiTurret.SetFireRate(baseFireRate * currentConfig.fireRateMultiplier);
            multiTurret.SetBulletSpeed(baseBulletSpeed * currentConfig.bulletSpeedMultiplier);
            DebugLog($"Multi-turret stats updated");
        }

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
            // === TIER 1 CONFIGURATIONS ===
            case "heavy":
                return new TankConfiguration
                {
                    upgradeName = "Heavy",
                    tankColor = Color.white,
                    moveSpeedMultiplier = 0.7f,
                    fireRateMultiplier = 0.8f,
                    bulletSpeedMultiplier = 0.8f,
                    bulletScale = 1.8f, // 重型坦克 - 大型子彈
                };

            case "rapid":
                return new TankConfiguration
                {
                    upgradeName = "Rapid",
                    tankColor = Color.white,
                    moveSpeedMultiplier = 1.2f,
                    fireRateMultiplier = 1.8f,
                    bulletSpeedMultiplier = 1.4f,
                    bulletScale = 0.7f, // 快速坦克 - 小型子彈
                };

            case "balanced":
                return new TankConfiguration
                {
                    upgradeName = "Balanced",
                    tankColor = Color.white,
                    moveSpeedMultiplier = 1f,
                    fireRateMultiplier = 1.3f,
                    bulletSpeedMultiplier = 1f,
                    bulletScale = 1.25f, // 平衡坦克 - 標準子彈
                };

            // === TIER 2 HEAVY CONFIGURATIONS (from Heavy) ===
            case "armorpiercing":
                return new TankConfiguration
                {
                    upgradeName = "ArmorPiercing",
                    tankColor = new Color(0.9f, 0.3f, 0.3f), // Dark red
                    moveSpeedMultiplier = 0.6f, // Even slower than Heavy
                    fireRateMultiplier = 0.6f, // Slower fire rate
                    bulletSpeedMultiplier = 0.7f, // Slower bullets
                    bulletScale = 1.8f, // 穿甲彈 - 超大子彈
                };

            case "superheavy":
                return new TankConfiguration
                {
                    upgradeName = "SuperHeavy",
                    tankColor = new Color(0.8f, 0.2f, 0.2f), // Darker red
                    moveSpeedMultiplier = 0.5f, // Very slow
                    fireRateMultiplier = 0.4f, // Very slow fire rate
                    bulletSpeedMultiplier = 0.6f, // Very slow bullets
                    bulletScale = 2.0f, // 超重型 - 巨大子彈
                };

            // === TIER 2 RAPID CONFIGURATIONS (from Rapid) ===
            case "burst":
                return new TankConfiguration
                {
                    upgradeName = "Burst",
                    tankColor = new Color(0.3f, 0.9f, 0.3f), // Bright green
                    moveSpeedMultiplier = 1.3f, // Faster than Rapid
                    fireRateMultiplier = 2.2f, // Even faster fire rate
                    bulletSpeedMultiplier = 1.6f, // Faster bullets
                    bulletScale = 0.65f, // 爆發 - 小型高速子彈
                };

            case "machinegun":
                return new TankConfiguration
                {
                    upgradeName = "MachineGun",
                    tankColor = new Color(0.2f, 0.8f, 0.2f), // Dark green
                    moveSpeedMultiplier = 1.4f, // Very fast
                    fireRateMultiplier = 2.5f, // Very fast fire rate
                    bulletSpeedMultiplier = 1.8f, // Very fast bullets
                    bulletScale = 0.55f, // 機槍 - 微型高速子彈
                };

            // === TIER 2 BALANCED CONFIGURATIONS (from Balanced) ===
            case "tactical":
                return new TankConfiguration
                {
                    upgradeName = "Tactical",
                    tankColor = new Color(0.4f, 0.4f, 0.9f), // Bright blue
                    moveSpeedMultiplier = 1.1f, // Slightly faster than Balanced
                    fireRateMultiplier = 1.5f, // Better fire rate than Balanced
                    bulletSpeedMultiplier = 1.2f, // Faster bullets than Balanced
                    bulletScale = 1.2f, // 戰術 - 略大子彈
                };

            case "versatile":
                return new TankConfiguration
                {
                    upgradeName = "Versatile",
                    tankColor = new Color(0.5f, 0.3f, 0.9f), // Purple
                    moveSpeedMultiplier = 1.2f, // Good speed
                    fireRateMultiplier = 1.7f, // Good fire rate
                    bulletSpeedMultiplier = 1.3f, // Good bullet speed
                    bulletScale = 1f, // 多功能 - 標準子彈
                };

            // === BASIC CONFIGURATION ===
            case "basic":
                return new TankConfiguration
                {
                    upgradeName = "Basic",
                    tankColor = Color.white,
                    moveSpeedMultiplier = 1f,
                    fireRateMultiplier = 1f,
                    bulletSpeedMultiplier = 1f,
                    bulletScale = 1f, // 基礎 - 標準子彈
                };

            default:
                DebugLog($"No configuration for: {upgradeName}");
                return null;
        }
    }

    // === PUBLIC METHODS FOR UPGRADE WHEEL ===

    // Tier 1 Upgrade Methods
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

    // Tier 2 Heavy Upgrade Methods
    public void SelectArmorPiercingUpgrade()
    {
        OnUpgradeSelected("ArmorPiercing");
    }

    public void SelectSuperHeavyUpgrade()
    {
        OnUpgradeSelected("SuperHeavy");
    }

    // Tier 2 Rapid Upgrade Methods
    public void SelectBurstUpgrade()
    {
        OnUpgradeSelected("Burst");
    }

    public void SelectMachineGunUpgrade()
    {
        OnUpgradeSelected("MachineGun");
    }

    // Tier 2 Balanced Upgrade Methods
    public void SelectTacticalUpgrade()
    {
        OnUpgradeSelected("Tactical");
    }

    public void SelectVersatileUpgrade()
    {
        OnUpgradeSelected("Versatile");
    }

    // CONTEXT MENU TESTING - PERMANENT (SAVES TO PLAYERDATA)
    [ContextMenu("SAVE Heavy Transformation")]
    public void TestHeavy()
    {
        OnUpgradeSelected("Heavy");
    }

    [ContextMenu("SAVE Rapid Transformation")]
    public void TestRapid()
    {
        OnUpgradeSelected("Rapid");
    }

    [ContextMenu("SAVE Balanced Transformation")]
    public void TestBalanced()
    {
        OnUpgradeSelected("Balanced");
    }

    [ContextMenu("SAVE ArmorPiercing (Tier 2)")]
    public void TestArmorPiercing()
    {
        OnUpgradeSelected("ArmorPiercing");
    }

    [ContextMenu("SAVE MachineGun (Tier 2)")]
    public void TestMachineGun()
    {
        OnUpgradeSelected("MachineGun");
    }

    [ContextMenu("SAVE Tactical (Tier 2)")]
    public void TestTactical()
    {
        OnUpgradeSelected("Tactical");
    }

    // CONTEXT MENU TESTING - VISUAL ONLY (DOESN'T SAVE)
    [ContextMenu("VISUAL ONLY Heavy")]
    public void TestVisualOnlyHeavy()
    {
        ApplyVisualTransformation("Heavy");
        ApplyStatChanges("Heavy");
        UpdateShootingSystem();
        DebugLog("Applied VISUAL ONLY Heavy - not saved to PlayerData");
    }

    [ContextMenu("VISUAL ONLY Rapid")]
    public void TestVisualOnlyRapid()
    {
        ApplyVisualTransformation("Rapid");
        ApplyStatChanges("Rapid");
        UpdateShootingSystem();
        DebugLog("Applied VISUAL ONLY Rapid - not saved to PlayerData");
    }

    [ContextMenu("VISUAL ONLY ArmorPiercing")]
    public void TestVisualOnlyArmorPiercing()
    {
        ApplyVisualTransformation("ArmorPiercing");
        ApplyStatChanges("ArmorPiercing");
        UpdateShootingSystem();
        DebugLog("Applied VISUAL ONLY ArmorPiercing - not saved to PlayerData");
    }

    [ContextMenu("VISUAL ONLY MachineGun")]
    public void TestVisualOnlyMachineGun()
    {
        ApplyVisualTransformation("MachineGun");
        ApplyStatChanges("MachineGun");
        UpdateShootingSystem();
        DebugLog("Applied VISUAL ONLY MachineGun - not saved to PlayerData");
    }

    [ContextMenu("RESET to PlayerData Saved")]
    public void ResetToSaved()
    {
        LoadSavedTransformation();
        DebugLog("Reset to whatever is saved in PlayerDataManager");
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
            DebugLog($"Color: {currentConfig.tankColor}");
        }
        else
        {
            DebugLog("CurrentConfig is NULL!");
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

        // ★★★ NEW: Debug PlayerTank level hierarchy ★★★
        DebugLog("=== PLAYERTANK LEVEL HIERARCHY ===");
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            DebugLog($"PlayerTank Child {i}: {child.name} (active: {child.gameObject.activeSelf})");
        }

        // Debug prefab availability
        DebugLog("=== PREFAB AVAILABILITY CHECK ===");
        DebugLog($"hugeTier1Prefab: {(hugeTier1Prefab != null ? "✓ Loaded" : "✗ NULL")}");
        DebugLog($"smallTier1Prefab: {(smallTier1Prefab != null ? "✓ Loaded" : "✗ NULL")}");
        DebugLog($"balancedTier1Prefab: {(balancedTier1Prefab != null ? "✓ Loaded" : "✗ NULL")}");
        DebugLog($"hugeTier2FrontPrefab: {(hugeTier2FrontPrefab != null ? "✓ Loaded" : "✗ NULL")}");
        DebugLog($"hugeTier2AroundPrefab: {(hugeTier2AroundPrefab != null ? "✓ Loaded" : "✗ NULL")}");
    }

    [ContextMenu("Debug Prefab Loading")]
    public void DebugPrefabLoading()
    {
        DebugLog("=== TESTING PREFAB LOADING ===");

        var testHuge = Resources.Load<GameObject>("TankPrefabs/Huge_T1");
        DebugLog($"Manual load Huge_T1: {(testHuge != null ? "✓ SUCCESS" : "✗ FAILED")}");

        var testSmall = Resources.Load<GameObject>("TankPrefabs/Small_T1");
        DebugLog($"Manual load Small_T1: {(testSmall != null ? "✓ SUCCESS" : "✗ FAILED")}");

        var testBalanced = Resources.Load<GameObject>("TankPrefabs/Balanced_T1");
        DebugLog($"Manual load Balanced_T1: {(testBalanced != null ? "✓ SUCCESS" : "✗ FAILED")}");

        // Try without TankPrefabs folder
        var testHugeNoFolder = Resources.Load<GameObject>("Huge_T1");
        DebugLog($"Manual load Huge_T1 (no folder): {(testHugeNoFolder != null ? "✓ SUCCESS" : "✗ FAILED")}");

        DebugLog("Check your Resources folder structure!");
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
    public float bulletScale = 1f; // 子彈大小倍數
}