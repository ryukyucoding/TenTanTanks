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
        [SerializeField] private string currentUpgradePath = "Basic";
        [SerializeField] private WheelUpgradeOption currentUpgradeOption;
        [SerializeField] private WheelTankStats currentWheelStats;

        // Upgrade events
        public static event Action<WheelUpgradeOption> OnTankUpgraded;
        public static event Action<string> OnUpgradePathChanged;

        // Static access for easy reference
        public static TankUpgradeSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

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
                description = "Standard tank configuration",
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
                description = "Heavy barrel - High damage, slow fire rate",
                tier = 1,
                damageMultiplier = 2f,
                fireRateMultiplier = 0.5f,
                bulletSizeMultiplier = 1.5f,
                moveSpeedMultiplier = 0.8f,
                healthBonus = 20,
                barrelPrefabName = "HeavyBarrel",
                tankColor = new Color(0.8f, 0.4f, 0.4f)
            };

            var rapidOption = new WheelUpgradeOption
            {
                upgradeName = "Rapid",
                description = "Rapid barrel - High fire rate, low damage",
                tier = 1,
                damageMultiplier = 0.6f,
                fireRateMultiplier = 3f,
                bulletSizeMultiplier = 0.7f,
                moveSpeedMultiplier = 1.2f,
                healthBonus = -20,
                barrelPrefabName = "RapidBarrel",
                tankColor = new Color(0.4f, 0.8f, 0.4f)
            };

            var balancedOption = new WheelUpgradeOption
            {
                upgradeName = "Balanced",
                description = "Balanced barrel - Medium attributes",
                tier = 1,
                damageMultiplier = 1.2f,
                fireRateMultiplier = 1.5f,
                bulletSizeMultiplier = 1f,
                moveSpeedMultiplier = 1.1f,
                healthBonus = 0,
                barrelPrefabName = "BalancedBarrel",
                tankColor = new Color(0.4f, 0.4f, 0.8f)
            };

            // Tier 2 upgrades - Heavy variants
            var superHeavy = new WheelUpgradeOption
            {
                upgradeName = "SuperHeavy",
                description = "Super heavy barrel - Extreme damage",
                tier = 2,
                parentUpgradeName = "Heavy",
                damageMultiplier = 3f,
                fireRateMultiplier = 0.3f,
                bulletSizeMultiplier = 2f,
                moveSpeedMultiplier = 0.6f,
                healthBonus = 50,
                barrelPrefabName = "SuperHeavyBarrel",
                tankColor = new Color(0.9f, 0.2f, 0.2f)
            };

            var armorPiercing = new WheelUpgradeOption
            {
                upgradeName = "ArmorPiercing",
                description = "Armor piercing barrel - Penetrates armor",
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
                description = "Machine gun barrel - Extreme fire rate",
                tier = 2,
                parentUpgradeName = "Rapid",
                damageMultiplier = 0.3f,
                fireRateMultiplier = 5f,
                bulletSizeMultiplier = 0.5f,
                moveSpeedMultiplier = 1.4f,
                healthBonus = -40,
                barrelPrefabName = "MachineGunBarrel",
                tankColor = new Color(0.2f, 0.9f, 0.2f)
            };

            var burst = new WheelUpgradeOption
            {
                upgradeName = "Burst",
                description = "Burst barrel - Three-shot burst",
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
                description = "Versatile barrel - All-around improvement",
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
                description = "Tactical barrel - Precision shooting",
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

                // Create WheelTankStats for this upgrade
                currentWheelStats = WheelTankStats.CreateFromUpgradePath(upgradeName);

                Debug.Log($"Applied upgrade: {upgradeName}");
                Debug.Log($"WheelTankStats: {currentWheelStats}");

                // Apply the stats to actual tank components
                ApplyStatsToTank();

                // Trigger events
                OnTankUpgraded?.Invoke(currentUpgradeOption);
                OnUpgradePathChanged?.Invoke(currentUpgradePath);
            }
            else
            {
                Debug.LogError($"Upgrade option '{upgradeName}' not found!");
            }
        }

        private void ApplyStatsToTank()
        {
            if (currentWheelStats == null)
            {
                Debug.LogWarning("No WheelTankStats to apply!");
                return;
            }

            // Find tank components
            var tankController = FindObjectOfType<TankController>();
            var tankShooting = FindObjectOfType<TankShooting>();

            // Apply to TankController
            if (tankController != null)
            {
                tankController.SetMoveSpeed(currentWheelStats.moveSpeed);
                Debug.Log($"Applied move speed: {currentWheelStats.moveSpeed}");
            }
            else
            {
                Debug.LogWarning("TankController not found!");
            }

            // Apply to TankShooting
            if (tankShooting != null)
            {
                tankShooting.SetBulletSpeed(currentWheelStats.bulletSpeed);
                tankShooting.SetFireRate(currentWheelStats.fireRate);
                Debug.Log($"Applied bullet speed: {currentWheelStats.bulletSpeed}");
                Debug.Log($"Applied fire rate: {currentWheelStats.fireRate}");
            }
            else
            {
                Debug.LogWarning("TankShooting not found!");
            }

            // Apply visual changes
            ApplyVisualChanges();

            Debug.Log($"Successfully applied {currentUpgradePath} to tank!");
        }

        private void ApplyVisualChanges()
        {
            if (currentUpgradeOption == null) return;

            // FIXED: Don't override colors if TankTransformationManager is handling visual changes
            var transformationManager = FindObjectOfType<TankTransformationManager>();
            if (transformationManager != null)
            {
                Debug.Log("TankTransformationManager found - skipping color application to avoid conflicts");
                return;
            }

            // Apply tank color only if no transformation system is active
            var tankRenderer = FindObjectOfType<Renderer>();
            if (tankRenderer != null)
            {
                tankRenderer.material.color = currentUpgradeOption.tankColor;
                Debug.Log($"Applied tank color: {currentUpgradeOption.tankColor}");
            }
        }

        public List<WheelUpgradeOption> GetAvailableUpgrades(int tier, string parentName = "")
        {
            var available = new List<WheelUpgradeOption>();

            foreach (var upgrade in availableUpgrades)
            {
                if (upgrade.tier == tier)
                {
                    if (tier == 1 || string.IsNullOrEmpty(parentName))
                    {
                        available.Add(upgrade);
                    }
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

        // Public getters
        public WheelTankStats GetCurrentWheelStats() => currentWheelStats;
        public WheelUpgradeOption GetCurrentUpgradeOption() => currentUpgradeOption;
        public string GetCurrentUpgradePath() => currentUpgradePath;

        [ContextMenu("Test ArmorPiercing")]
        public void TestArmorPiercing()
        {
            ApplyUpgrade("ArmorPiercing");
        }

        [ContextMenu("Test MachineGun")]
        public void TestMachineGun()
        {
            ApplyUpgrade("MachineGun");
        }
    }
}