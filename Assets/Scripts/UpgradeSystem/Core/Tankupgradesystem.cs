using UnityEngine;
using System.Collections.Generic;
using System;

namespace WheelUpgradeSystem
{
    public class TankUpgradeSystem : MonoBehaviour
    {
        [Header("Upgrade Configuration")]
        [SerializeField] private List<WheelUpgradeOption> availableUpgrades = new List<WheelUpgradeOption>();

        [Header("Current Tank State")]
        [SerializeField] private string currentUpgradePath = "Basic"; // Current upgrade path
        [SerializeField] private WheelUpgradeOption currentUpgradeOption;

        // Upgrade events
        public static event Action<WheelUpgradeOption> OnTankUpgraded;
        public static event Action<string> OnUpgradePathChanged;

        private void Start()
        {
            InitializeDefaultUpgrades();
            ApplyUpgrade("Basic"); // Start with basic configuration
        }

        private void InitializeDefaultUpgrades()
        {
            availableUpgrades.Clear();

            // Basic tank (Tier 0)
            var basicTank = new WheelUpgradeOption
            {
                upgradeName = "Basic",
                description = "標準坦克配置",
                tier = 0,
                damageMultiplier = 1f,
                fireRateMultiplier = 1f,
                bulletSizeMultiplier = 1f,
                moveSpeedMultiplier = 1f,
                healthBonus = 0,
                barrelPrefabName = "BasicBarrel",
                tankColor = Color.white
            };

            // Tier 1 upgrades
            var heavyOption = new WheelUpgradeOption
            {
                upgradeName = "Heavy",
                description = "重型砲管 - 大傷害，慢射速",
                tier = 1,
                damageMultiplier = 2f,
                fireRateMultiplier = 0.5f,
                bulletSizeMultiplier = 1.5f,
                moveSpeedMultiplier = 0.8f,
                healthBonus = 20,
                barrelPrefabName = "HeavyBarrel",
                tankColor = new Color(0.8f, 0.4f, 0.4f) // Reddish
            };

            var rapidOption = new WheelUpgradeOption
            {
                upgradeName = "Rapid",
                description = "快速砲管 - 高射速，小傷害",
                tier = 1,
                damageMultiplier = 0.6f,
                fireRateMultiplier = 3f,
                bulletSizeMultiplier = 0.7f,
                moveSpeedMultiplier = 1.2f,
                healthBonus = -20,
                barrelPrefabName = "RapidBarrel",
                tankColor = new Color(0.4f, 0.8f, 0.4f) // Greenish
            };

            var balancedOption = new WheelUpgradeOption
            {
                upgradeName = "Balanced",
                description = "平衡砲管 - 中等屬性",
                tier = 1,
                damageMultiplier = 1.2f,
                fireRateMultiplier = 1.5f,
                bulletSizeMultiplier = 1f,
                moveSpeedMultiplier = 1.1f,
                healthBonus = 0,
                barrelPrefabName = "BalancedBarrel",
                tankColor = new Color(0.4f, 0.4f, 0.8f) // Bluish
            };

            // Tier 2 upgrades - Heavy variants
            var superHeavy = new WheelUpgradeOption
            {
                upgradeName = "SuperHeavy",
                description = "超重型砲管 - 極大傷害",
                tier = 2,
                parentUpgradeName = "Heavy",
                damageMultiplier = 3f,
                fireRateMultiplier = 0.3f,
                bulletSizeMultiplier = 2f,
                moveSpeedMultiplier = 0.6f,
                healthBonus = 50,
                barrelPrefabName = "SuperHeavyBarrel",
                tankColor = new Color(0.9f, 0.2f, 0.2f),
                scaleMultiplier = new Vector3(1.2f, 1.2f, 1.2f)
            };

            var armorPiercing = new WheelUpgradeOption
            {
                upgradeName = "ArmorPiercing",
                description = "穿甲砲管 - 穿透護甲",
                tier = 2,
                parentUpgradeName = "Heavy",
                damageMultiplier = 1.6f,
                fireRateMultiplier = 0.8f,
                bulletSizeMultiplier = 1.2f,
                moveSpeedMultiplier = 0.9f,
                healthBonus = 10,
                barrelPrefabName = "ArmorPiercingBarrel",
                tankColor = new Color(0.7f, 0.3f, 0.3f)
            };

            // Tier 2 upgrades - Rapid variants
            var machineGun = new WheelUpgradeOption
            {
                upgradeName = "MachineGun",
                description = "機槍砲管 - 極高射速",
                tier = 2,
                parentUpgradeName = "Rapid",
                damageMultiplier = 0.3f,
                fireRateMultiplier = 5f,
                bulletSizeMultiplier = 0.5f,
                moveSpeedMultiplier = 1.4f,
                healthBonus = -40,
                barrelPrefabName = "MachineGunBarrel",
                tankColor = new Color(0.2f, 0.9f, 0.2f),
                scaleMultiplier = new Vector3(0.9f, 0.9f, 0.9f)
            };

            var burst = new WheelUpgradeOption
            {
                upgradeName = "Burst",
                description = "爆發砲管 - 三連發",
                tier = 2,
                parentUpgradeName = "Rapid",
                damageMultiplier = 0.8f,
                fireRateMultiplier = 2f,
                bulletSizeMultiplier = 0.8f,
                moveSpeedMultiplier = 1.1f,
                healthBonus = -10,
                barrelPrefabName = "BurstBarrel",
                tankColor = new Color(0.3f, 0.7f, 0.3f)
            };

            // Tier 2 upgrades - Balanced variants
            var versatile = new WheelUpgradeOption
            {
                upgradeName = "Versatile",
                description = "萬能砲管 - 全能提升",
                tier = 2,
                parentUpgradeName = "Balanced",
                damageMultiplier = 1.4f,
                fireRateMultiplier = 1.8f,
                bulletSizeMultiplier = 1.1f,
                moveSpeedMultiplier = 1.2f,
                healthBonus = 10,
                barrelPrefabName = "VersatileBarrel",
                tankColor = new Color(0.5f, 0.5f, 0.9f)
            };

            var tactical = new WheelUpgradeOption
            {
                upgradeName = "Tactical",
                description = "戰術砲管 - 精準射擊",
                tier = 2,
                parentUpgradeName = "Balanced",
                damageMultiplier = 1.6f,
                fireRateMultiplier = 1.2f,
                bulletSizeMultiplier = 0.9f,
                moveSpeedMultiplier = 1f,
                healthBonus = -5,
                barrelPrefabName = "TacticalBarrel",
                tankColor = new Color(0.3f, 0.3f, 0.7f)
            };

            // Add all upgrades to the list
            availableUpgrades.AddRange(new[] {
                basicTank, heavyOption, rapidOption, balancedOption,
                superHeavy, armorPiercing, machineGun, burst, versatile, tactical
            });

            Debug.Log($"Initialized {availableUpgrades.Count} upgrade options");
        }

        public void ApplyUpgrade(string upgradeName)
        {
            var upgrade = availableUpgrades.Find(u => u.upgradeName == upgradeName);
            if (upgrade != null)
            {
                currentUpgradeOption = upgrade;
                currentUpgradePath = upgradeName;

                Debug.Log($"Applied upgrade: {upgradeName}");
                Debug.Log($"New multipliers - Damage: {upgrade.damageMultiplier}x, Fire Rate: {upgrade.fireRateMultiplier}x");

                // Trigger events
                OnTankUpgraded?.Invoke(currentUpgradeOption);
                OnUpgradePathChanged?.Invoke(currentUpgradePath);
            }
            else
            {
                Debug.LogError($"Upgrade option '{upgradeName}' not found!");
            }
        }

        public List<WheelUpgradeOption> GetAvailableUpgrades(int tier, string parentName = "")
        {
            var available = new List<WheelUpgradeOption>();

            foreach (var upgrade in availableUpgrades)
            {
                if (upgrade.tier == tier)
                {
                    // Tier 1 or no parent requirement
                    if (tier == 1 || string.IsNullOrEmpty(parentName))
                    {
                        available.Add(upgrade);
                    }
                    // Tier 2 needs to check parent
                    else if (tier == 2 && upgrade.parentUpgradeName == parentName)
                    {
                        available.Add(upgrade);
                    }
                }
            }

            Debug.Log($"Found {available.Count} available upgrades for tier {tier}" +
                      (string.IsNullOrEmpty(parentName) ? "" : $" with parent {parentName}"));

            return available;
        }

        public WheelUpgradeOption GetCurrentUpgradeOption() => currentUpgradeOption;
        public string GetCurrentUpgradePath() => currentUpgradePath;

        // Get all available upgrades (for debugging)
        public List<WheelUpgradeOption> GetAllUpgrades() => availableUpgrades;

        // Reset to basic configuration
        [ContextMenu("Reset to Basic")]
        public void ResetToBasic()
        {
            ApplyUpgrade("Basic");
        }

        // Debug method to list all upgrades
        [ContextMenu("List All Upgrades")]
        public void ListAllUpgrades()
        {
            Debug.Log("=== ALL UPGRADE OPTIONS ===");
            foreach (var upgrade in availableUpgrades)
            {
                Debug.Log($"Tier {upgrade.tier}: {upgrade.upgradeName}" +
                         (string.IsNullOrEmpty(upgrade.parentUpgradeName) ? "" : $" (parent: {upgrade.parentUpgradeName})"));
            }
        }
    }
}