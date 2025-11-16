using UnityEngine;

namespace WheelUpgradeSystem
{
    /// <summary>
    /// Tank stats specifically for the wheel upgrade system
    /// Used for pre-game tank configuration, separate from in-game upgrade stats
    /// </summary>
    [System.Serializable]
    public class WheelTankStats
    {
        [Header("Combat Stats")]
        public float damage = 1f;
        public float fireRate = 1f;
        public float bulletSpeed = 5f;
        public float bulletSize = 1f;

        [Header("Movement Stats")]
        public float moveSpeed = 5f;
        public float rotationSpeed = 200f;

        [Header("Survival Stats")]
        public int maxHealth = 5;
        public float armor = 0f;

        [Header("Special Abilities")]
        public bool hasShield = false;
        public bool hasDoubleShot = false;
        public bool hasRapidFire = false;

        // Constructor
        public WheelTankStats()
        {
            // Default basic tank stats
            damage = 1f;
            fireRate = 1f;
            bulletSpeed = 5f;
            bulletSize = 1f;
            moveSpeed = 5f;
            rotationSpeed = 200f;
            maxHealth = 5;
            armor = 0f;
            hasShield = false;
            hasDoubleShot = false;
            hasRapidFire = false;
        }

        // Copy constructor
        public WheelTankStats(WheelTankStats other)
        {
            damage = other.damage;
            fireRate = other.fireRate;
            bulletSpeed = other.bulletSpeed;
            bulletSize = other.bulletSize;
            moveSpeed = other.moveSpeed;
            rotationSpeed = other.rotationSpeed;
            maxHealth = other.maxHealth;
            armor = other.armor;
            hasShield = other.hasShield;
            hasDoubleShot = other.hasDoubleShot;
            hasRapidFire = other.hasRapidFire;
        }

        // Apply upgrade option to stats
        public void ApplyUpgrade(WheelUpgradeOption upgrade)
        {
            damage *= upgrade.damageMultiplier;
            fireRate *= upgrade.fireRateMultiplier;
            bulletSize *= upgrade.bulletSizeMultiplier;
            moveSpeed *= upgrade.moveSpeedMultiplier;
            maxHealth += upgrade.healthBonus;
        }

        // Create stats from upgrade path
        public static WheelTankStats CreateFromUpgradePath(string upgradePath)
        {
            WheelTankStats stats = new WheelTankStats();

            // Apply different configurations based on upgrade path
            switch (upgradePath)
            {
                case "Heavy":
                    stats.damage = 2f;
                    stats.fireRate = 0.7f;
                    stats.maxHealth = 7;
                    stats.moveSpeed = 4f;
                    break;

                case "SuperHeavy":
                    stats.damage = 3f;
                    stats.fireRate = 0.5f;
                    stats.maxHealth = 10;
                    stats.moveSpeed = 3f;
                    stats.armor = 1f;
                    break;

                case "ArmorPiercing":
                    stats.damage = 2.5f;
                    stats.fireRate = 0.8f;
                    stats.maxHealth = 6;
                    stats.moveSpeed = 4.5f;
                    break;

                case "Rapid":
                    stats.damage = 0.7f;
                    stats.fireRate = 2f;
                    stats.maxHealth = 4;
                    stats.moveSpeed = 6f;
                    break;

                case "MachineGun":
                    stats.damage = 0.5f;
                    stats.fireRate = 3f;
                    stats.maxHealth = 3;
                    stats.moveSpeed = 7f;
                    stats.hasRapidFire = true;
                    break;

                case "Burst":
                    stats.damage = 1.2f;
                    stats.fireRate = 1.5f;
                    stats.maxHealth = 4;
                    stats.moveSpeed = 6f;
                    stats.hasDoubleShot = true;
                    break;

                case "Balanced":
                    stats.damage = 1.2f;
                    stats.fireRate = 1.2f;
                    stats.maxHealth = 5;
                    stats.moveSpeed = 5.2f;
                    break;

                case "Versatile":
                    stats.damage = 1.3f;
                    stats.fireRate = 1.3f;
                    stats.maxHealth = 6;
                    stats.moveSpeed = 5.5f;
                    stats.hasShield = true;
                    break;

                case "Tactical":
                    stats.damage = 1.5f;
                    stats.fireRate = 1.1f;
                    stats.maxHealth = 5;
                    stats.moveSpeed = 5.8f;
                    break;

                default: // "Basic"
                         // Use default constructor values
                    break;
            }

            return stats;
        }

        public override string ToString()
        {
            return $"Damage: {damage:F1}, Fire Rate: {fireRate:F1}, Health: {maxHealth}, Speed: {moveSpeed:F1}";
        }
    }
}