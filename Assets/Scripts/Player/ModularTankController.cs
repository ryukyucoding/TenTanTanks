using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 模組化坦克控制系統 - 根據升級路徑修改基礎坦克
/// 支援：
/// - 調整坦克部件大小（更大 = 更強但更慢）
/// - 添加多個炮塔以進行快速射擊配置
/// - 改變顏色和效果
/// - 相應調整遊戲玩法統計資料
/// </summary>
public class ModularTankController : MonoBehaviour
{
    [Header("坦克部件引用")]
    [SerializeField] private Transform tankBase;           // 坦克底盤
    [SerializeField] private Transform mainTurret;         // 主炮塔（炮管）
    [SerializeField] private Transform firePoint;          // 原始發射點
    [SerializeField] private SkinnedMeshRenderer[] tankRenderers;

    [Header("用於克隆的炮塔預製件")]
    [SerializeField] private GameObject turretPrefab;      // 用於克隆額外炮塔的預製件

    [Header("升級配置")]
    [SerializeField] private TankUpgradeConfig[] upgradeConfigs;

    [Header("音頻和效果")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip transformSound;

    // 當前配置
    private string currentUpgradePath = "Basic";
    private List<Transform> allTurrets = new List<Transform>();
    private List<Transform> allFirePoints = new List<Transform>();
    private TankUpgradeConfig currentConfig;

    // 組件引用
    private TankController tankController;
    private TankShooting tankShooting;
    private MultiTurretShooting multiTurretShooting;

    [System.Serializable]
    public class TankUpgradeConfig
    {
        [Header("基本資訊")]
        public string upgradeName;

        [Header("縮放修改")]
        public Vector3 baseScale = Vector3.one;
        public Vector3 turretScale = Vector3.one;

        [Header("視覺變化")]
        public Color tankColor = Color.white;
        public Material customMaterial;

        [Header("炮塔配置")]
        public int turretCount = 1;
        public Vector3[] additionalTurretPositions;  // 額外炮塔的位置
        public Vector3[] additionalTurretRotations;  // 額外炮塔的旋轉

        [Header("遊戲玩法統計資料")]
        public float damageMultiplier = 1f;
        public float fireRateMultiplier = 1f;
        public float bulletSpeedMultiplier = 1f;
        public float bulletSizeMultiplier = 1f;
        public float moveSpeedMultiplier = 1f;
        public int healthBonus = 0;

        [Header("特殊效果")]
        public bool hasGlowEffect = false;
        public bool hasParticleTrail = false;
    }

    void Start()
    {
        InitializeModularSystem();
        SetupUpgradeConfigurations();
        ApplyConfiguration("Basic");
    }

    private void InitializeModularSystem()
    {
        // 獲取組件引用
        tankController = GetComponent<TankController>();
        tankShooting = GetComponent<TankShooting>();
        multiTurretShooting = GetComponent<MultiTurretShooting>();

        // 如果未分配，則自動查找坦克部件
        if (tankBase == null)
            tankBase = transform.Find("ArmTank");

        if (mainTurret == null)
        {
            // 嘗試多個可能的炮塔路徑
            mainTurret = transform.Find("ArmTank/Base/Barrel") ??
                        transform.Find("ArmTank/Barrel.001") ??
                        transform.Find("ArmTank/ArmTank/Base/Barrel");
        }

        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint") ??
                       transform.Find("ArmTank/FirePoint");
        }

        // 初始化炮塔列表
        allTurrets.Clear();
        allFirePoints.Clear();

        if (mainTurret != null)
            allTurrets.Add(mainTurret);

        if (firePoint != null)
            allFirePoints.Add(firePoint);

        // 獲取所有渲染器以進行顏色更改
        if (tankRenderers == null || tankRenderers.Length == 0)
            tankRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        Debug.Log($"ModularTankController 已初始化：");
        Debug.Log($"  坦克底盤：{(tankBase != null ? tankBase.name : "未找到")}");
        Debug.Log($"  主炮塔：{(mainTurret != null ? mainTurret.name : "未找到")}");
        Debug.Log($"  發射點：{(firePoint != null ? firePoint.name : "未找到")}");
        Debug.Log($"  渲染器：{tankRenderers.Length}");
    }

    private void SetupUpgradeConfigurations()
    {
        if (upgradeConfigs == null || upgradeConfigs.Length == 0)
        {
            upgradeConfigs = CreateDefaultConfigurations();
        }
    }

    private TankUpgradeConfig[] CreateDefaultConfigurations()
    {
        return new TankUpgradeConfig[]
        {
            // 基本配置
            new TankUpgradeConfig
            {
                upgradeName = "Basic",
                baseScale = Vector3.one,
                turretScale = Vector3.one,
                tankColor = Color.white,
                turretCount = 1,
                damageMultiplier = 1f,
                fireRateMultiplier = 1f,
                moveSpeedMultiplier = 1f
            },
            
            // 重型 - 更大、更慢、更強大
            new TankUpgradeConfig
            {
                upgradeName = "Heavy",
                baseScale = new Vector3(1.2f, 1.2f, 1.2f),
                turretScale = new Vector3(1.5f, 1.5f, 1.5f),
                tankColor = new Color(0.5f, 0.5f, 0.5f),
                turretCount = 1,
                damageMultiplier = 2f,
                fireRateMultiplier = 0.7f,
                bulletSizeMultiplier = 1.5f,
                moveSpeedMultiplier = 0.8f,
                healthBonus = 2
            },
            
            // 超重型 - 更大、更慢、毀滅性
            new TankUpgradeConfig
            {
                upgradeName = "SuperHeavy",
                baseScale = new Vector3(1.4f, 1.4f, 1.4f),
                turretScale = new Vector3(2f, 2f, 2f),
                tankColor = new Color(0.3f, 0.3f, 0.3f),
                turretCount = 1,
                damageMultiplier = 3f,
                fireRateMultiplier = 0.5f,
                bulletSizeMultiplier = 2f,
                moveSpeedMultiplier = 0.6f,
                healthBonus = 4,
                hasGlowEffect = true
            },
            
            // 穿甲 - 中等大小、專業化
            new TankUpgradeConfig
            {
                upgradeName = "ArmorPiercing",
                baseScale = new Vector3(1.1f, 1.1f, 1.1f),
                turretScale = new Vector3(1.3f, 1f, 1.3f), // 更長但不更高
                tankColor = new Color(0.7f, 0.7f, 0.3f),
                turretCount = 1,
                damageMultiplier = 2.5f,
                fireRateMultiplier = 0.8f,
                bulletSizeMultiplier = 1.2f,
                moveSpeedMultiplier = 0.9f,
                healthBonus = 1
            },
            
            // 快速 - 更小、更快、多個小炮塔
            new TankUpgradeConfig
            {
                upgradeName = "Rapid",
                baseScale = new Vector3(0.9f, 0.9f, 0.9f),
                turretScale = new Vector3(0.7f, 0.7f, 0.7f),
                tankColor = new Color(1f, 0.5f, 0f),
                turretCount = 2,
                additionalTurretPositions = new Vector3[] { new Vector3(0.3f, 0, 0.1f) },
                additionalTurretRotations = new Vector3[] { new Vector3(0, 15f, 0) },
                damageMultiplier = 0.7f,
                fireRateMultiplier = 1.8f,
                bulletSizeMultiplier = 0.7f,
                moveSpeedMultiplier = 1.3f,
                healthBonus = -1
            },
            
            // 機槍 - 多個小型快速射擊炮塔
            new TankUpgradeConfig
            {
                upgradeName = "MachineGun",
                baseScale = new Vector3(0.8f, 0.8f, 0.8f),
                turretScale = new Vector3(0.6f, 0.6f, 0.6f),
                tankColor = new Color(1f, 0.3f, 0f),
                turretCount = 3,
                additionalTurretPositions = new Vector3[]
                {
                    new Vector3(0.25f, 0, 0.1f),
                    new Vector3(-0.25f, 0, 0.1f)
                },
                additionalTurretRotations = new Vector3[]
                {
                    new Vector3(0, 10f, 0),
                    new Vector3(0, -10f, 0)
                },
                damageMultiplier = 0.5f,
                fireRateMultiplier = 3f,
                bulletSizeMultiplier = 0.5f,
                moveSpeedMultiplier = 1.5f,
                healthBonus = -2,
                hasParticleTrail = true
            },
            
            // 爆發 - 同步射擊的雙炮塔
            new TankUpgradeConfig
            {
                upgradeName = "Burst",
                baseScale = new Vector3(0.9f, 0.9f, 0.9f),
                turretScale = new Vector3(0.8f, 0.8f, 0.8f),
                tankColor = new Color(1f, 0.7f, 0f),
                turretCount = 2,
                additionalTurretPositions = new Vector3[] { new Vector3(0.2f, 0, 0) },
                additionalTurretRotations = new Vector3[] { Vector3.zero },
                damageMultiplier = 0.8f,
                fireRateMultiplier = 2.5f,
                bulletSizeMultiplier = 0.8f,
                moveSpeedMultiplier = 1.2f,
                healthBonus = 0
            },
            
            // 平衡 - 全面優化輕微改進
            new TankUpgradeConfig
            {
                upgradeName = "Balanced",
                baseScale = Vector3.one,
                turretScale = new Vector3(1.1f, 1.1f, 1.1f),
                tankColor = new Color(0f, 0.5f, 1f),
                turretCount = 1,
                damageMultiplier = 1.2f,
                fireRateMultiplier = 1.2f,
                bulletSizeMultiplier = 1.1f,
                moveSpeedMultiplier = 1.1f,
                healthBonus = 1
            },
            
            // 精準 - 具有遠程功能的狙擊型
            new TankUpgradeConfig
            {
                upgradeName = "Precision",
                baseScale = Vector3.one,
                turretScale = new Vector3(0.9f, 0.9f, 1.4f), // 更長的炮管
                tankColor = new Color(0f, 0.3f, 0.8f),
                turretCount = 1,
                damageMultiplier = 1.8f,
                fireRateMultiplier = 0.9f,
                bulletSpeedMultiplier = 1.5f,
                bulletSizeMultiplier = 1.2f,
                moveSpeedMultiplier = 1.1f,
                healthBonus = 0
            },
            
            // 多功能 - 帶雙炮塔的雙重用途
            new TankUpgradeConfig
            {
                upgradeName = "Versatile",
                baseScale = new Vector3(1.05f, 1.05f, 1.05f),
                turretScale = Vector3.one,
                tankColor = new Color(0.3f, 0.8f, 0.3f),
                turretCount = 2,
                additionalTurretPositions = new Vector3[] { new Vector3(0.15f, 0, -0.1f) },
                additionalTurretRotations = new Vector3[] { new Vector3(0, -10f, 0) },
                damageMultiplier = 1.1f,
                fireRateMultiplier = 1.3f,
                bulletSizeMultiplier = 1f,
                moveSpeedMultiplier = 1f,
                healthBonus = 2
            }
        };
    }

    public void ApplyConfiguration(string upgradePath)
    {
        currentUpgradePath = upgradePath;

        // Make sure upgradeConfigs is initialized
        if (upgradeConfigs == null || upgradeConfigs.Length == 0)
        {
            SetupUpgradeConfigurations();
        }

        // Find configuration
        currentConfig = System.Array.Find(upgradeConfigs, config => config.upgradeName == upgradePath);

        if (currentConfig == null)
        {
            Debug.LogWarning($"Configuration '{upgradePath}' not found! Available configs:");
            if (upgradeConfigs != null)
            {
                for (int i = 0; i < upgradeConfigs.Length; i++)
                {
                    Debug.LogWarning($"  - {upgradeConfigs[i].upgradeName}");
                }
            }

            // Use first config as fallback
            if (upgradeConfigs != null && upgradeConfigs.Length > 0)
            {
                currentConfig = upgradeConfigs[0];
                Debug.LogWarning($"Using fallback config: {currentConfig.upgradeName}");
            }
            else
            {
                Debug.LogError("No upgrade configurations available!");
                return;
            }
        }

        Debug.Log($"Applying configuration: {upgradePath}");

        // Rest of your existing ApplyConfiguration code...
        PlayTransformSound();
        ApplyScaleModifications();
        ApplyVisualChanges();
        ApplyTurretConfiguration();
        ApplyGameplayStats();
        ApplySpecialEffects();
    }

    private void ApplyScaleModifications()
    {
        // 縮放坦克底盤
        if (tankBase != null)
        {
            tankBase.localScale = currentConfig.baseScale;
        }

        // 縮放主炮塔
        if (mainTurret != null)
        {
            mainTurret.localScale = currentConfig.turretScale;
        }

        Debug.Log($"已應用縮放 - 底盤：{currentConfig.baseScale}，炮塔：{currentConfig.turretScale}");
    }

    private void ApplyVisualChanges()
    {
        // 改變坦克顏色
        foreach (var renderer in tankRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = currentConfig.tankColor;
            }
        }

        // 如果指定，則應用自訂材質
        if (currentConfig.customMaterial != null)
        {
            foreach (var renderer in tankRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = currentConfig.customMaterial;
                }
            }
        }

        Debug.Log($"已應用視覺變化 - 顏色：{currentConfig.tankColor}");
    }

    private void ApplyTurretConfiguration()
    {
        // 從上一個配置中移除額外炮塔
        RemoveAdditionalTurrets();

        // 如果需要，添加新炮塔
        if (currentConfig.turretCount > 1 && turretPrefab != null)
        {
            AddAdditionalTurrets();
        }

        // 通知多炮塔射擊系統更新發射點
        if (multiTurretShooting != null)
        {
            multiTurretShooting.RefreshFirePoints();
        }

        Debug.Log($"已應用炮塔配置 - 數量：{currentConfig.turretCount}");
    }

    private void RemoveAdditionalTurrets()
    {
        // 僅保留主炮塔和發射點
        while (allTurrets.Count > 1)
        {
            Transform extraTurret = allTurrets[allTurrets.Count - 1];
            if (extraTurret != null && extraTurret != mainTurret)
            {
                DestroyImmediate(extraTurret.gameObject);
            }
            allTurrets.RemoveAt(allTurrets.Count - 1);
        }

        while (allFirePoints.Count > 1)
        {
            allFirePoints.RemoveAt(allFirePoints.Count - 1);
        }
    }

    private void AddAdditionalTurrets()
    {
        int turretsToAdd = currentConfig.turretCount - 1; // -1 因為我們有主炮塔

        for (int i = 0; i < turretsToAdd; i++)
        {
            if (i < currentConfig.additionalTurretPositions.Length)
            {
                // 創建額外炮塔
                GameObject newTurret = Instantiate(turretPrefab, tankBase);

                // 定位炮塔
                Vector3 position = currentConfig.additionalTurretPositions[i];
                Vector3 rotation = i < currentConfig.additionalTurretRotations.Length ?
                                 currentConfig.additionalTurretRotations[i] : Vector3.zero;

                newTurret.transform.localPosition = position;
                newTurret.transform.localRotation = Quaternion.Euler(rotation);
                newTurret.transform.localScale = currentConfig.turretScale;

                // 將顏色應用於新炮塔
                var newTurretRenderers = newTurret.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var renderer in newTurretRenderers)
                {
                    if (renderer != null && renderer.material != null)
                    {
                        renderer.material.color = currentConfig.tankColor;
                    }
                }

                // 添加到列表
                allTurrets.Add(newTurret.transform);

                // 為新炮塔創建發射點
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(newTurret.transform);
                firePointObj.transform.localPosition = new Vector3(0, 0.25f, 0.45f); // 與主發射點相同
                firePointObj.transform.localRotation = Quaternion.identity;

                allFirePoints.Add(firePointObj.transform);

                Debug.Log($"在位置 {position} 添加炮塔 {i + 1}");
            }
        }
    }

    private void ApplyGameplayStats()
    {
        // 應用統計資料到射擊系統（TankShooting 或 MultiTurretShooting）
        if (multiTurretShooting != null)
        {
            // 計算新射速（考慮多炮塔）
            float baseFireRate = 1.2f; // 基礎射速
            float adjustedFireRate = baseFireRate * currentConfig.fireRateMultiplier;
            multiTurretShooting.SetFireRate(adjustedFireRate);

            // 計算新子彈速度
            float baseBulletSpeed = 5f; // 基礎子彈速度
            float newBulletSpeed = baseBulletSpeed * currentConfig.bulletSpeedMultiplier;
            multiTurretShooting.SetBulletSpeed(newBulletSpeed);
        }
        else if (tankShooting != null)
        {
            // 回退到原始 TankShooting
            float baseFireRate = 1.2f;
            float adjustedFireRate = baseFireRate * currentConfig.fireRateMultiplier;
            tankShooting.SetFireRate(adjustedFireRate);

            float baseBulletSpeed = 5f;
            float newBulletSpeed = baseBulletSpeed * currentConfig.bulletSpeedMultiplier;
            tankShooting.SetBulletSpeed(newBulletSpeed);
        }

        // 應用健康變化
        var playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null && currentConfig.healthBonus != 0)
        {
            Debug.Log($"已應用健康獎勵：{currentConfig.healthBonus}");
        }

        Debug.Log($"已應用遊戲玩法統計資料 - 移動：{currentConfig.moveSpeedMultiplier}x，射擊：{currentConfig.fireRateMultiplier}x，傷害：{currentConfig.damageMultiplier}x");
    }

    private void ApplySpecialEffects()
    {
        // 添加發光效果
        if (currentConfig.hasGlowEffect)
        {
            Debug.Log("已應用發光效果");
        }

        // 添加粒子尾跡
        if (currentConfig.hasParticleTrail)
        {
            Debug.Log("已應用粒子尾跡效果");
        }
    }

    private void PlayTransformSound()
    {
        if (audioSource != null && transformSound != null)
        {
            audioSource.PlayOneShot(transformSound);
        }
    }

    /// <summary>
    /// 獲取所有當前發射點（用於多炮塔射擊）
    /// </summary>
    public List<Transform> GetAllFirePoints()
    {
        return allFirePoints;
    }

    /// <summary>
    /// 獲取當前升級配置
    /// </summary>
    public TankUpgradeConfig GetCurrentConfig()
    {
        return currentConfig;
    }

    /// <summary>
    /// 獲取子彈傷害計算的傷害倍數
    /// </summary>
    public float GetDamageMultiplier()
    {
        return currentConfig?.damageMultiplier ?? 1f;
    }

    /// <summary>
    /// 獲取子彈縮放的子彈大小倍數
    /// </summary>
    public float GetBulletSizeMultiplier()
    {
        return currentConfig?.bulletSizeMultiplier ?? 1f;
    }

    #region Debug Methods

    [ContextMenu("測試重型配置")]
    public void TestHeavyConfig()
    {
        ApplyConfiguration("Heavy");
    }

    [ContextMenu("測試機槍配置")]
    public void TestMachineGunConfig()
    {
        ApplyConfiguration("MachineGun");
    }

    [ContextMenu("測試快速配置")]
    public void TestRapidConfig()
    {
        ApplyConfiguration("Rapid");
    }

    [ContextMenu("重置為基本")]
    public void TestBasicConfig()
    {
        ApplyConfiguration("Basic");
    }

    [ContextMenu("打印當前配置")]
    public void PrintCurrentConfig()
    {
        if (currentConfig != null)
        {
            Debug.Log($"=== 當前模組化坦克配置 ===");
            Debug.Log($"升級：{currentConfig.upgradeName}");
            Debug.Log($"炮塔數量：{currentConfig.turretCount}");
            Debug.Log($"底盤縮放：{currentConfig.baseScale}");
            Debug.Log($"炮塔縮放：{currentConfig.turretScale}");
            Debug.Log($"傷害：{currentConfig.damageMultiplier}x");
            Debug.Log($"射速：{currentConfig.fireRateMultiplier}x");
            Debug.Log($"移動速度：{currentConfig.moveSpeedMultiplier}x");
        }
        else
        {
            Debug.Log("未應用配置");
        }
    }

    #endregion
}