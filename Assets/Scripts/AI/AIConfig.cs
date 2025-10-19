using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AIConfig
{
    [Header("Movement Settings")]
    public float rotationSpeed = 1.0f;
    public float aimingAngle = 25f;
    public float rotationMultMax = 2.0f;
    public float rotationMultMin = 0.8f;
    
    [Header("Patrol Settings")]
    public float patrolRadius = 200f;
    public float distLeavePatrol = 500f;
    public float distLeaveDefend = 800f;
    public float distLeaveAttack = 200f;
    
    [Header("Dodge Settings")]
    public bool canDodgeProj = true;
    public bool canDodgeMine = true;
    public float distStartDodge = 45f;
    public float projDodgeCooldownVal = 30f;
    public float mineDodgeCooldownVal = 30f;
    public bool advancedDodge = false;
    
    [Header("Targeting Settings")]
    public bool perfectAim = false;
    public bool advancedTargeting = true;
    public bool predictiveTargeting = true;
    public float predictiveTargetingChance = 50f;
    public float shootThreshold = 10f;
    public float safeThreshold = 50f;
    public bool shootEnemyProjectiles = true;
    public float shootEnemyProjectilesRange = 100f;
    
    [Header("Combat Settings")]
    public float salvoCooldownAmount = 120f;
    public float wanderRadius = 200f;
    public float defendTime = 600f;
    public bool randomMovement = false;
    
    [Header("Mine Settings")]
    public float mineChance = 10000f;
    public float mineMinDistance = 250f;
    public bool randomMines = false;
    public float avoidMineDist = 300f;
    
    [Header("Special Settings")]
    public bool suicide = false;
    public LayerMask obstacleLayer = 1;
}

[System.Serializable]
public class TankUnitConfig
{
    [Header("Tank Properties")]
    public float tankSpeedModifier = 1.92f;
    public float projectileSpeedModifier = 3.36f;
    public float firerate = 2f;
    public int bounchLimit = 1;
    public int mineLimit = 4;
    public int projectileLimit = 5;
    public string aiPersonality = "player";
    public bool useTurret = true;
    public bool useMagReloadLogic = false;
    public int magSize = 1;
    public float reloadTime = 0f;
}

[CreateAssetMenu(fileName = "AI Database", menuName = "AI/AI Database")]
public class AIDatabase : ScriptableObject
{
    [Header("AI Personalities")]
    public List<AIPersonality> personalities = new List<AIPersonality>();
    
    [Header("Tank Units")]
    public List<TankUnit> tankUnits = new List<TankUnit>();
}

[System.Serializable]
public class AIPersonality
{
    public string name;
    public AIConfig config;
}

[System.Serializable]
public class TankUnit
{
    public string name;
    public TankUnitConfig config;
}

[System.Serializable]
public class AIPersonalityData
{
    public string name;
    public AIConfig config;
    public string description;
}

public static class AIConfigLoader
{
    public static AIConfig GetAIConfig(string personalityName)
    {
        // 從Resources或Addressables加載配置
        AIDatabase database = Resources.Load<AIDatabase>("AI/AI Database");
        
        if (database != null)
        {
            foreach (var personality in database.personalities)
            {
                if (personality.name == personalityName)
                {
                    return personality.config;
                }
            }
        }
        
        // 返回默認配置
        return new AIConfig();
    }
    
    public static TankUnitConfig GetTankUnitConfig(string unitName)
    {
        AIDatabase database = Resources.Load<AIDatabase>("AI/AI Database");
        
        if (database != null)
        {
            foreach (var unit in database.tankUnits)
            {
                if (unit.name == unitName)
                {
                    return unit.config;
                }
            }
        }
        
        // 返回默認配置
        return new TankUnitConfig();
    }
}
