using UnityEngine;

namespace WheelUpgradeSystem
{
    /// <summary>
    /// Upgrade option specifically for the wheel upgrade system
    /// Separate from the in-game upgrade system
    /// </summary>
    [System.Serializable]
    public class WheelUpgradeOption
    {
        [Header("Basic Info")]
        public string upgradeName;
        public string description;
        public Sprite icon;

        [Header("Hierarchy")]
        public string parentUpgradeName = ""; // For tier 2 upgrades
        public int tier = 1; // 1 for tier 1, 2 for tier 2

        [Header("Tank Configuration")]
        public float damageMultiplier = 1f;
        public float fireRateMultiplier = 1f;
        public float bulletSizeMultiplier = 1f;
        public float moveSpeedMultiplier = 1f;
        public int healthBonus = 0;

        [Header("Visual Changes")]
        public string barrelPrefabName = "";
        public Color tankColor = Color.white;
        public Vector3 scaleMultiplier = Vector3.one;

        // Default constructor
        public WheelUpgradeOption()
        {
            upgradeName = "Basic";
            description = "Basic tank configuration";
            tier = 1;
            damageMultiplier = 1f;
            fireRateMultiplier = 1f;
            bulletSizeMultiplier = 1f;
            moveSpeedMultiplier = 1f;
            healthBonus = 0;
            tankColor = Color.white;
            scaleMultiplier = Vector3.one;
        }

        public WheelUpgradeOption(string name, string desc, int upgradeRank = 1)
        {
            upgradeName = name;
            description = desc;
            tier = upgradeRank;
        }

        // Constructor for tier 2 upgrades
        public WheelUpgradeOption(string name, string desc, string parent, int upgradeRank = 2)
        {
            upgradeName = name;
            description = desc;
            parentUpgradeName = parent;
            tier = upgradeRank;
        }

        public bool IsBasicUpgrade()
        {
            return upgradeName == "Basic";
        }

        public bool IsTier1Upgrade()
        {
            return tier == 1 && !IsBasicUpgrade();
        }

        public bool IsTier2Upgrade()
        {
            return tier == 2;
        }

        public bool IsChildOf(string parentName)
        {
            return parentUpgradeName == parentName;
        }
    }
}