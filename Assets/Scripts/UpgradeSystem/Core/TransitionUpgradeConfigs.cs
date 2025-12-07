using UnityEngine;

namespace WheelUpgradeSystem
{
    /// <summary>
    /// Transition upgrade option configurations - Corresponds to specific tank models
    /// Based on your upgrade tree structure: Basic ¡÷ [doublehead, HUGE, SMALL] ¡÷ [fourhead series, HUGE-3cannon, SMALL-3cannon]
    /// </summary>
    public static class TransitionUpgradeConfigs
    {
        /// <summary>
        /// Get Level 2¡÷3 first tier upgrade options
        /// </summary>
        public static WheelUpgradeOption[] GetLevel2To3Upgrades()
        {
            return new WheelUpgradeOption[]
            {
                // 1. doublehead - middle path: retain original barrel, add one more
                new WheelUpgradeOption
                {
                    upgradeName = "doublehead",
                    description = "Dual Cannon Tank: Retains original barrel and adds one more, balanced firepower upgrade",
                    tier = 1,
                    damageMultiplier = 1.2f,
                    fireRateMultiplier = 1.1f,
                    bulletSizeMultiplier = 1.0f,
                    moveSpeedMultiplier = 0.95f,  // Slightly slower
                    healthBonus = 1,
                    barrelPrefabName = "doublehead",
                    tankColor = new Color(0.8f, 0.8f, 0.2f), // Yellow tone
                    scaleMultiplier = new Vector3(1.1f, 1.0f, 1.1f)
                },

                // 2. HUGE - heavy path: larger barrel, slow but strong
                new WheelUpgradeOption
                {
                    upgradeName = "HUGE",
                    description = "Heavy Tank: Massive barrel fires large bullets, slower movement but tremendous power",
                    tier = 1,
                    damageMultiplier = 1.8f,      // High damage
                    fireRateMultiplier = 0.7f,     // Slow fire rate
                    bulletSizeMultiplier = 1.5f,   // Large bullets
                    moveSpeedMultiplier = 0.8f,    // Slow movement
                    healthBonus = 2,
                    barrelPrefabName = "HUGE",
                    tankColor = new Color(0.8f, 0.2f, 0.2f), // Red tone
                    scaleMultiplier = new Vector3(1.2f, 1.1f, 1.2f)
                },

                // 3. SMALL - speed path: smaller barrel, fast but weak
                new WheelUpgradeOption
                {
                    upgradeName = "SMALL",
                    description = "Light Tank: Small barrel fires rapid bullets, agile movement but lower power",
                    tier = 1,
                    damageMultiplier = 0.8f,      // Low damage
                    fireRateMultiplier = 1.4f,     // Fast fire rate
                    bulletSizeMultiplier = 0.7f,   // Small bullets
                    moveSpeedMultiplier = 1.3f,    // Fast movement
                    healthBonus = 0,
                    barrelPrefabName = "SMALL",
                    tankColor = new Color(0.2f, 0.8f, 0.2f), // Green tone
                    scaleMultiplier = new Vector3(0.9f, 0.9f, 0.9f)
                }
            };
        }

        /// <summary>
        /// Get Level 4¡÷5 second tier upgrade options
        /// Based on player's previous choice provides different upgrade paths
        /// </summary>
        public static WheelUpgradeOption[] GetLevel4To5Upgrades(string parentUpgrade)
        {
            switch (parentUpgrade.ToLower())
            {
                case "doublehead":
                    return GetDoubleheadSecondTierUpgrades();

                case "huge":
                    return GetHugeSecondTierUpgrades();

                case "small":
                    return GetSmallSecondTierUpgrades();

                default:
                    Debug.LogWarning("Unknown parent upgrade: " + parentUpgrade + ", using default upgrade options");
                    return GetDoubleheadSecondTierUpgrades();
            }
        }

        /// <summary>
        /// doublehead second tier upgrade options
        /// </summary>
        private static WheelUpgradeOption[] GetDoubleheadSecondTierUpgrades()
        {
            return new WheelUpgradeOption[]
            {
                // Four cannon front-back configuration: 2 front + 2 back
                new WheelUpgradeOption
                {
                    upgradeName = "fourhead_front_back",
                    description = "Quad Cannon Tank: 2 cannons front, 2 cannons back, full directional firepower coverage",
                    tier = 2,
                    parentUpgradeName = "doublehead",
                    damageMultiplier = 1.4f,
                    fireRateMultiplier = 1.2f,
                    bulletSizeMultiplier = 1.0f,
                    moveSpeedMultiplier = 0.9f,
                    healthBonus = 2,
                    barrelPrefabName = "fourhead",
                    tankColor = new Color(0.9f, 0.7f, 0.2f), // Orange-yellow
                    scaleMultiplier = new Vector3(1.15f, 1.05f, 1.15f)
                },

                // Four cannon cross configuration: front, back, left, right
                new WheelUpgradeOption
                {
                    upgradeName = "fourhead_cross",
                    description = "Cross Fire: One cannon each direction - front, back, left, right for 360¢X no-dead-zone attack",
                    tier = 2,
                    parentUpgradeName = "doublehead",
                    damageMultiplier = 1.3f,
                    fireRateMultiplier = 1.3f,
                    bulletSizeMultiplier = 0.9f,
                    moveSpeedMultiplier = 0.95f,
                    healthBonus = 1,
                    barrelPrefabName = "fourhead", // May need special cross configuration model
                    tankColor = new Color(0.7f, 0.7f, 0.3f), // Olive color
                    scaleMultiplier = new Vector3(1.1f, 1.0f, 1.1f)
                }
            };
        }

        /// <summary>
        /// HUGE second tier upgrade options
        /// </summary>
        private static WheelUpgradeOption[] GetHugeSecondTierUpgrades()
        {
            return new WheelUpgradeOption[]
            {
                // 3 massive cannons at front
                new WheelUpgradeOption
                {
                    upgradeName = "HUGE_triple_front",
                    description = "Triple Heavy Cannon: 3 massive cannons at front, devastating frontal firepower",
                    tier = 2,
                    parentUpgradeName = "HUGE",
                    damageMultiplier = 2.2f,       // Extremely high damage
                    fireRateMultiplier = 0.6f,      // Very slow fire rate
                    bulletSizeMultiplier = 1.8f,    // Huge bullets
                    moveSpeedMultiplier = 0.7f,     // Very slow movement
                    healthBonus = 3,
                    barrelPrefabName = "HUGE", // Need special triple HUGE model
                    tankColor = new Color(0.9f, 0.1f, 0.1f), // Dark red
                    scaleMultiplier = new Vector3(1.3f, 1.2f, 1.3f)
                },

                // 3 massive cannons at 120 degree spread
                new WheelUpgradeOption
                {
                    upgradeName = "HUGE_triple_120",
                    description = "Heavy Cannon Triangle: 3 massive cannons at 120¢X spread, area devastation",
                    tier = 2,
                    parentUpgradeName = "HUGE",
                    damageMultiplier = 2.0f,
                    fireRateMultiplier = 0.65f,
                    bulletSizeMultiplier = 1.7f,
                    moveSpeedMultiplier = 0.75f,
                    healthBonus = 3,
                    barrelPrefabName = "HUGE", // Need special triangle configuration HUGE model
                    tankColor = new Color(0.8f, 0.0f, 0.2f), // Wine red
                    scaleMultiplier = new Vector3(1.25f, 1.15f, 1.25f)
                }
            };
        }

        /// <summary>
        /// SMALL second tier upgrade options
        /// </summary>
        private static WheelUpgradeOption[] GetSmallSecondTierUpgrades()
        {
            return new WheelUpgradeOption[]
            {
                // 3 small cannons at front
                new WheelUpgradeOption
                {
                    upgradeName = "SMALL_triple_front",
                    description = "Triple Rapid Fire: 3 small cannons at front, extreme speed bullet barrage attack",
                    tier = 2,
                    parentUpgradeName = "SMALL",
                    damageMultiplier = 0.9f,        // Medium damage
                    fireRateMultiplier = 1.8f,      // Extremely fast fire rate
                    bulletSizeMultiplier = 0.6f,    // Small bullets
                    moveSpeedMultiplier = 1.4f,     // Extremely fast movement
                    healthBonus = 0,
                    barrelPrefabName = "SMALL", // Need special triple SMALL model
                    tankColor = new Color(0.1f, 0.9f, 0.1f), // Bright green
                    scaleMultiplier = new Vector3(0.85f, 0.9f, 0.85f)
                },

                // 3 small cannons at 120 degree spread
                new WheelUpgradeOption
                {
                    upgradeName = "SMALL_triple_120",
                    description = "Rapid Fire Triangle: 3 small cannons at 120¢X spread, mobile bullet barrage",
                    tier = 2,
                    parentUpgradeName = "SMALL",
                    damageMultiplier = 0.85f,
                    fireRateMultiplier = 1.9f,      // Fastest fire rate
                    bulletSizeMultiplier = 0.5f,    // Smallest bullets
                    moveSpeedMultiplier = 1.5f,     // Fastest movement
                    healthBonus = 0,
                    barrelPrefabName = "SMALL", // Need special triangle configuration SMALL model
                    tankColor = new Color(0.0f, 1.0f, 0.3f), // Fluorescent green
                    scaleMultiplier = new Vector3(0.8f, 0.85f, 0.8f)
                }
            };
        }

        /// <summary>
        /// Get display name of upgrade option
        /// </summary>
        public static string GetDisplayName(string upgradeName)
        {
            switch (upgradeName.ToLower())
            {
                case "doublehead": return "Dual Cannon";
                case "huge": return "Heavy";
                case "small": return "Light";
                case "fourhead_front_back": return "Quad Front-Back";
                case "fourhead_cross": return "Cross Fire";
                case "huge_triple_front": return "Triple Heavy Cannon";
                case "huge_triple_120": return "Heavy Cannon Triangle";
                case "small_triple_front": return "Triple Rapid Fire";
                case "small_triple_120": return "Rapid Fire Triangle";
                default: return upgradeName;
            }
        }

        /// <summary>
        /// Get complete description of upgrade path
        /// </summary>
        public static string GetUpgradePathDescription(string firstTier, string secondTier = "")
        {
            string description = GetDisplayName(firstTier);

            if (!string.IsNullOrEmpty(secondTier))
            {
                description += " ¡÷ " + GetDisplayName(secondTier);
            }

            return description;
        }
    }
}