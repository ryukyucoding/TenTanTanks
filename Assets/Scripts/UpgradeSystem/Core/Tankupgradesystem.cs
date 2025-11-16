using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class TankStats
{
    [Header("Combat Stats")]
    public float damage = 1f;           // 傷害值
    public float fireRate = 1f;         // 發射頻率（每秒幾發）
    public float bulletSize = 1f;       // 子彈大小
    public float bulletSpeed = 10f;     // 子彈飛行速度
    public float reloadTime = 1f;       // 重裝填時間

    [Header("Movement Stats")]
    public float moveSpeed = 5f;        // 移動速度
    public float rotationSpeed = 180f;  // 旋轉速度

    [Header("Defense Stats")]
    public float maxHealth = 100f;      // 最大血量
    public float armor = 0f;            // 護甲值

    [Header("Visual")]
    public string barrelPrefabName;     // 砲管預製體名稱
    public Color tankColor = Color.white; // 坦克顏色
}

[System.Serializable]
public class UpgradeOption
{
    [Header("Basic Info")]
    public string upgradeName;          // 升級名稱
    public string description;          // 描述
    public Sprite icon;                 // 圖標

    [Header("Stats Modification")]
    public TankStats stats;             // 該升級的屬性

    [Header("Unlock Requirements")]
    public int tierLevel;               // 層級 (1=第二層, 2=第三層)
    public string parentUpgradeName;    // 父級升級名稱（第三層需要）
}

public class TankUpgradeSystem : MonoBehaviour
{
    [Header("Upgrade Configuration")]
    [SerializeField] private List<UpgradeOption> availableUpgrades = new List<UpgradeOption>();

    [Header("Current Tank State")]
    [SerializeField] private TankStats currentStats = new TankStats();
    [SerializeField] private string currentUpgradePath = "Basic"; // 當前升級路徑

    // 升級事件
    public static event Action<TankStats> OnTankUpgraded;
    public static event Action<string> OnUpgradePathChanged;
    private void Start()
    {
        InitializeDefaultUpgrades();
        ApplyCurrentStats();
    }

    private void InitializeDefaultUpgrades()
    {
        // 清空現有升級選項
        availableUpgrades.Clear();

        // 基礎坦克
        var basicTank = new UpgradeOption
        {
            upgradeName = "Basic",
            description = "標準坦克配置",
            tierLevel = 0,
            stats = new TankStats
            {
                damage = 25f,
                fireRate = 1f,
                bulletSize = 1f,
                bulletSpeed = 10f,
                reloadTime = 1f,
                moveSpeed = 5f,
                rotationSpeed = 180f,
                maxHealth = 100f,
                barrelPrefabName = "BasicBarrel"
            }
        };

        // 第二層升級選項
        var heavyOption = new UpgradeOption
        {
            upgradeName = "Heavy",
            description = "重型砲管 - 大傷害，慢射速",
            tierLevel = 1,
            stats = new TankStats
            {
                damage = 50f,           // 大傷害
                fireRate = 0.5f,        // 慢射速
                bulletSize = 1.5f,      // 大子彈
                bulletSpeed = 8f,       // 稍慢子彈
                reloadTime = 2f,        // 慢重裝填
                moveSpeed = 4f,         // 稍慢移動
                rotationSpeed = 150f,
                maxHealth = 120f,       // 更多血量
                barrelPrefabName = "HeavyBarrel"
            }
        };

        var rapidOption = new UpgradeOption
        {
            upgradeName = "Rapid",
            description = "快速砲管 - 高射速，小傷害",
            tierLevel = 1,
            stats = new TankStats
            {
                damage = 15f,           // 小傷害
                fireRate = 3f,          // 快射速
                bulletSize = 0.7f,      // 小子彈
                bulletSpeed = 12f,      // 快子彈
                reloadTime = 0.3f,      // 快重裝填
                moveSpeed = 6f,         // 快移動
                rotationSpeed = 200f,
                maxHealth = 80f,        // 少血量
                barrelPrefabName = "RapidBarrel"
            }
        };

        var balancedOption = new UpgradeOption
        {
            upgradeName = "Balanced",
            description = "平衡砲管 - 中等屬性",
            tierLevel = 1,
            stats = new TankStats
            {
                damage = 30f,           // 中等傷害
                fireRate = 1.5f,        // 中等射速
                bulletSize = 1f,        // 標準子彈
                bulletSpeed = 10f,      // 標準速度
                reloadTime = 0.8f,      // 中等重裝填
                moveSpeed = 5.5f,       // 稍快移動
                rotationSpeed = 180f,
                maxHealth = 100f,
                barrelPrefabName = "BalancedBarrel"
            }
        };

        // 第三層升級選項（Heavy的變體）
        var superHeavy = new UpgradeOption
        {
            upgradeName = "SuperHeavy",
            description = "超重型砲管 - 極大傷害",
            tierLevel = 2,
            parentUpgradeName = "Heavy",
            stats = new TankStats
            {
                damage = 80f,
                fireRate = 0.3f,
                bulletSize = 2f,
                bulletSpeed = 6f,
                reloadTime = 3f,
                moveSpeed = 3f,
                rotationSpeed = 120f,
                maxHealth = 150f,
                barrelPrefabName = "SuperHeavyBarrel"
            }
        };

        var armorPiercing = new UpgradeOption
        {
            upgradeName = "ArmorPiercing",
            description = "穿甲砲管 - 穿透護甲",
            tierLevel = 2,
            parentUpgradeName = "Heavy",
            stats = new TankStats
            {
                damage = 40f,
                fireRate = 0.8f,
                bulletSize = 1.2f,
                bulletSpeed = 15f,      // 快速穿甲彈
                reloadTime = 1.5f,
                moveSpeed = 4.5f,
                rotationSpeed = 160f,
                maxHealth = 110f,
                barrelPrefabName = "ArmorPiercingBarrel"
            }
        };

        // Rapid的變體
        var machineGun = new UpgradeOption
        {
            upgradeName = "MachineGun",
            description = "機槍砲管 - 極高射速",
            tierLevel = 2,
            parentUpgradeName = "Rapid",
            stats = new TankStats
            {
                damage = 8f,
                fireRate = 5f,          // 極高射速
                bulletSize = 0.5f,
                bulletSpeed = 15f,
                reloadTime = 0.1f,
                moveSpeed = 7f,
                rotationSpeed = 220f,
                maxHealth = 60f,
                barrelPrefabName = "MachineGunBarrel"
            }
        };

        var burst = new UpgradeOption
        {
            upgradeName = "Burst",
            description = "爆發砲管 - 三連發",
            tierLevel = 2,
            parentUpgradeName = "Rapid",
            stats = new TankStats
            {
                damage = 12f,
                fireRate = 2f,
                bulletSize = 0.8f,
                bulletSpeed = 12f,
                reloadTime = 0.5f,
                moveSpeed = 5.5f,
                rotationSpeed = 190f,
                maxHealth = 90f,
                barrelPrefabName = "BurstBarrel"
            }
        };

        // Balanced的變體
        var versatile = new UpgradeOption
        {
            upgradeName = "Versatile",
            description = "萬能砲管 - 全能提升",
            tierLevel = 2,
            parentUpgradeName = "Balanced",
            stats = new TankStats
            {
                damage = 35f,
                fireRate = 1.8f,
                bulletSize = 1.1f,
                bulletSpeed = 11f,
                reloadTime = 0.7f,
                moveSpeed = 6f,
                rotationSpeed = 200f,
                maxHealth = 110f,
                barrelPrefabName = "VersatileBarrel"
            }
        };

        var tactical = new UpgradeOption
        {
            upgradeName = "Tactical",
            description = "戰術砲管 - 精準射擊",
            tierLevel = 2,
            parentUpgradeName = "Balanced",
            stats = new TankStats
            {
                damage = 40f,
                fireRate = 1.2f,
                bulletSize = 0.9f,
                bulletSpeed = 14f,      // 高精度子彈
                reloadTime = 1f,
                moveSpeed = 5f,
                rotationSpeed = 160f,
                maxHealth = 95f,
                barrelPrefabName = "TacticalBarrel"
            }
        };

        // 添加所有升級選項
        availableUpgrades.AddRange(new[] {
            basicTank, heavyOption, rapidOption, balancedOption,
            superHeavy, armorPiercing, machineGun, burst, versatile, tactical
        });
    }

    public void ApplyUpgrade(string upgradeName)
    {
        var upgrade = availableUpgrades.Find(u => u.upgradeName == upgradeName);
        if (upgrade != null)
        {
            currentStats = upgrade.stats;
            currentUpgradePath = upgradeName;

            Debug.Log($"Applied upgrade: {upgradeName}");
            Debug.Log($"New stats - Damage: {currentStats.damage}, Fire Rate: {currentStats.fireRate}");

            // 觸發事件
            OnTankUpgraded?.Invoke(currentStats);
            OnUpgradePathChanged?.Invoke(currentUpgradePath);

            ApplyCurrentStats();
        }
    }

    private void ApplyCurrentStats()
    {
        // 這裡可以實際應用屬性到玩家坦克
        // 之後會整合到TankController和TankShooting
        Debug.Log($"Current tank stats applied: {currentUpgradePath}");
    }

    public List<UpgradeOption> GetAvailableUpgrades(int tierLevel, string parentName = "")
    {
        var available = new List<UpgradeOption>();

        foreach (var upgrade in availableUpgrades)
        {
            if (upgrade.tierLevel == tierLevel)
            {
                // 第二層或無父級要求
                if (tierLevel == 1 || string.IsNullOrEmpty(parentName))
                {
                    available.Add(upgrade);
                }
                // 第三層需要檢查父級
                else if (tierLevel == 2 && upgrade.parentUpgradeName == parentName)
                {
                    available.Add(upgrade);
                }
            }
        }

        return available;
    }

    public TankStats GetCurrentStats() => currentStats;
    public string GetCurrentUpgradePath() => currentUpgradePath;
}
