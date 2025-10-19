using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AI Personality Setup", menuName = "AI/AI Personality Setup")]
public class AIPersonalitySetup : ScriptableObject
{
    [Header("AI Personalities Configuration")]
    [SerializeField] private List<AIPersonalityData> allPersonalities = new List<AIPersonalityData>();
    
    [Header("Settings")]
    [SerializeField] private bool autoInitialize = true;
    
    void OnEnable()
    {
        if (autoInitialize && allPersonalities.Count == 0)
        {
            InitializeAllPersonalities();
        }
    }
    
    [ContextMenu("Initialize All 12 AI Personalities")]
    public void InitializeAllPersonalities()
    {
        allPersonalities.Clear();
        
        // 1. Brown - 基礎防禦型
        AddPersonality("brown", CreateBrownConfig(), "基礎防禦型，高級瞄準但無預測射擊");
        
        // 2. Ash - 巡邏型
        AddPersonality("ash", CreateAshConfig(), "巡邏型，有預測射擊和閃避能力");
        
        // 3. Marine - 精準射擊型
        AddPersonality("marine", CreateMarineConfig(), "精準射擊型，完美瞄準但無預測");
        
        // 4. Yellow - 隨機移動型
        AddPersonality("yellow", CreateYellowConfig(), "隨機移動型，可射擊敵方子彈");
        
        // 5. Pink - 高級瞄準型
        AddPersonality("pink", CreatePinkConfig(), "高級瞄準型，無閃避能力");
        
        // 6. Green - 綜合型
        AddPersonality("green", CreateGreenConfig(), "高級瞄準+預測射擊，可射擊敵方子彈");
        
        // 7. Violet - 地雷專家
        AddPersonality("violet", CreateVioletConfig(), "綜合型，有地雷能力");
        
        // 8. White - 高級閃避型
        AddPersonality("white", CreateWhiteConfig(), "高級閃避型，連射能力");
        
        // 9. Black - 最強型
        AddPersonality("black", CreateBlackConfig(), "最強型，完美瞄準+高級閃避+地雷");
        
        // 10. ZBlue - 進階藍色
        AddPersonality("zblue", CreateZBlueConfig(), "進階藍色，高級閃避+連射");
        
        // 11. ZBrown - 進階棕色
        AddPersonality("zbrown", CreateZBrownConfig(), "進階棕色，快速射擊");
        
        // 12. ZAsh - 進階灰色
        AddPersonality("zash", CreateZAshConfig(), "進階灰色，高級閃避+預測射擊");
        
        Debug.Log($"已初始化 {allPersonalities.Count} 種AI個性");
    }
    
    private void AddPersonality(string name, AIConfig config, string description)
    {
        AIPersonalityData personality = new AIPersonalityData();
        personality.name = name;
        personality.config = config;
        personality.description = description;
        allPersonalities.Add(personality);
    }
    
    public AIConfig GetPersonalityConfig(string name)
    {
        foreach (var personality in allPersonalities)
        {
            if (personality.name == name)
            {
                return personality.config;
            }
        }
        
        Debug.LogWarning($"AI個性 '{name}' 未找到，使用默認配置");
        return CreateDefaultConfig();
    }
    
    public List<AIPersonalityData> GetAllPersonalities()
    {
        return new List<AIPersonalityData>(allPersonalities);
    }
    
    // 1. Brown - 基礎防禦型
    private AIConfig CreateBrownConfig()
    {
        AIConfig config = new AIConfig();
        config.rotationSpeed = 0.8f;
        config.aimingAngle = 180f;
        config.rotationMultMax = 1.0f;
        config.rotationMultMin = 0.75f;
        config.advancedTargeting = true;
        config.predictiveTargeting = false;
        config.predictiveTargetingChance = 50f;
        config.shootThreshold = 15f;
        config.safeThreshold = 60f;
        config.shootEnemyProjectiles = false;
        return config;
    }
    
    // 2. Ash - 巡邏型
    private AIConfig CreateAshConfig()
    {
        AIConfig config = new AIConfig();
        config.patrolRadius = 250f;
        config.distLeavePatrol = 450f;
        config.distLeaveDefend = 800f;
        config.distLeaveAttack = 250f;
        config.canDodgeProj = true;
        config.distStartDodge = 50f;
        config.projDodgeCooldownVal = 200f;
        config.rotationSpeed = 0.8f;
        config.aimingAngle = 45f;
        config.rotationMultMax = 1.0f;
        config.rotationMultMin = 0.75f;
        config.perfectAim = false;
        config.advancedTargeting = false;
        config.predictiveTargeting = true;
        config.predictiveTargetingChance = 60f;
        config.shootThreshold = 15f;
        config.safeThreshold = 60f;
        config.shootEnemyProjectiles = false;
        config.wanderRadius = 150f;
        config.defendTime = 2000f;
        return config;
    }
    
    // 3. Marine - 精準射擊型
    private AIConfig CreateMarineConfig()
    {
        AIConfig config = new AIConfig();
        config.patrolRadius = 250f;
        config.distLeavePatrol = 450f;
        config.distLeaveDefend = 9999f;
        config.distLeaveAttack = 9999f;
        config.canDodgeProj = true;
        config.distStartDodge = 50f;
        config.projDodgeCooldownVal = 25f;
        config.rotationSpeed = 1.2f;
        config.aimingAngle = 1f;
        config.rotationMultMax = 2.0f;
        config.rotationMultMin = 0.75f;
        config.perfectAim = true;
        config.predictiveTargeting = false;
        config.shootThreshold = 15f;
        config.safeThreshold = 60f;
        config.shootEnemyProjectiles = false;
        config.wanderRadius = 150f;
        config.defendTime = 999999f;
        return config;
    }
    
    // 4. Yellow - 隨機移動型
    private AIConfig CreateYellowConfig()
    {
        AIConfig config = new AIConfig();
        config.patrolRadius = 500f;
        config.distLeavePatrol = 500f;
        config.distLeaveDefend = 800f;
        config.distLeaveAttack = 100f;
        config.canDodgeProj = false;
        config.distStartDodge = 50f;
        config.projDodgeCooldownVal = 25f;
        config.rotationSpeed = 1.2f;
        config.aimingAngle = 20f;
        config.rotationMultMax = 2.0f;
        config.rotationMultMin = 0.75f;
        config.randomMovement = true;
        config.advancedTargeting = false;
        config.predictiveTargeting = true;
        config.predictiveTargetingChance = 60f;
        config.shootThreshold = 15f;
        config.safeThreshold = 60f;
        config.shootEnemyProjectiles = true;
        config.shootEnemyProjectilesRange = 90f;
        config.wanderRadius = 150f;
        config.defendTime = 500f;
        config.avoidMineDist = 300f;
        config.mineDodgeCooldownVal = 100f;
        config.mineChance = 400f;
        config.randomMines = true;
        return config;
    }
    
    // 5. Pink - 高級瞄準型
    private AIConfig CreatePinkConfig()
    {
        AIConfig config = new AIConfig();
        config.patrolRadius = 250f;
        config.distLeavePatrol = 450f;
        config.distLeaveDefend = 800f;
        config.distLeaveAttack = 250f;
        config.canDodgeProj = false;
        config.distStartDodge = 50f;
        config.projDodgeCooldownVal = 25f;
        config.rotationSpeed = 1.2f;
        config.aimingAngle = 45f;
        config.rotationMultMax = 2.0f;
        config.rotationMultMin = 0.75f;
        config.perfectAim = false;
        config.advancedTargeting = true;
        config.predictiveTargeting = false;
        config.shootThreshold = 15f;
        config.safeThreshold = 60f;
        config.shootEnemyProjectiles = false;
        config.wanderRadius = 150f;
        config.defendTime = 200f;
        return config;
    }
    
    // 6. Green - 綜合型
    private AIConfig CreateGreenConfig()
    {
        AIConfig config = new AIConfig();
        config.rotationSpeed = 1f;
        config.aimingAngle = 70f;
        config.rotationMultMax = 2.0f;
        config.rotationMultMin = 0.75f;
        config.advancedTargeting = true;
        config.predictiveTargeting = true;
        config.predictiveTargetingChance = 50f;
        config.salvoCooldownAmount = 150f;
        config.shootThreshold = 15f;
        config.safeThreshold = 60f;
        config.shootEnemyProjectiles = true;
        config.shootEnemyProjectilesRange = 90f;
        return config;
    }
    
    // 7. Violet - 地雷專家
    private AIConfig CreateVioletConfig()
    {
        AIConfig config = new AIConfig();
        config.patrolRadius = 250f;
        config.distLeavePatrol = 450f;
        config.distLeaveDefend = 800f;
        config.distLeaveAttack = 250f;
        config.canDodgeProj = false;
        config.distStartDodge = 50f;
        config.projDodgeCooldownVal = 25f;
        config.rotationSpeed = 1.2f;
        config.aimingAngle = 45f;
        config.rotationMultMax = 2.0f;
        config.rotationMultMin = 0.75f;
        config.advancedTargeting = true;
        config.predictiveTargeting = true;
        config.predictiveTargetingChance = 50f;
        config.shootThreshold = 15f;
        config.safeThreshold = 60f;
        config.shootEnemyProjectiles = true;
        config.shootEnemyProjectilesRange = 90f;
        config.wanderRadius = 150f;
        config.defendTime = 200f;
        config.mineChance = 1500f;
        return config;
    }
    
    // 8. White - 高級閃避型
    private AIConfig CreateWhiteConfig()
    {
        AIConfig config = new AIConfig();
        config.patrolRadius = 250f;
        config.distLeavePatrol = 450f;
        config.distLeaveDefend = 800f;
        config.distLeaveAttack = 250f;
        config.salvoCooldownAmount = 150f;
        config.canDodgeProj = true;
        config.distStartDodge = 50f;
        config.projDodgeCooldownVal = 25f;
        config.advancedDodge = true;
        config.rotationSpeed = 2.4f;
        config.aimingAngle = 45f;
        config.rotationMultMax = 4.0f;
        config.rotationMultMin = 0.75f;
        config.perfectAim = false;
        config.advancedTargeting = true;
        config.predictiveTargeting = true;
        config.predictiveTargetingChance = 50f;
        config.shootThreshold = 15f;
        config.safeThreshold = 60f;
        config.shootEnemyProjectiles = true;
        config.shootEnemyProjectilesRange = 100f;
        config.wanderRadius = 150f;
        config.defendTime = 200f;
        return config;
    }
    
    // 9. Black - 最強型
    private AIConfig CreateBlackConfig()
    {
        AIConfig config = new AIConfig();
        config.patrolRadius = 250f;
        config.distLeavePatrol = 850f;
        config.distLeaveDefend = 800f;
        config.distLeaveAttack = 100f;
        config.canDodgeProj = true;
        config.distStartDodge = 50f;
        config.projDodgeCooldownVal = 20f;
        config.advancedDodge = true;
        config.rotationSpeed = 2f;
        config.aimingAngle = 20f;
        config.rotationMultMax = 5.0f;
        config.rotationMultMin = 0.75f;
        config.perfectAim = true;
        config.advancedTargeting = true;
        config.predictiveTargeting = false;
        config.shootThreshold = 15f;
        config.safeThreshold = 60f;
        config.shootEnemyProjectiles = true;
        config.shootEnemyProjectilesRange = 90f;
        config.wanderRadius = 150f;
        config.defendTime = 100f;
        config.mineChance = 1f;
        config.mineMinDistance = 150f;
        return config;
    }
    
    // 10. ZBlue - 進階藍色
    private AIConfig CreateZBlueConfig()
    {
        AIConfig config = new AIConfig();
        config.patrolRadius = 250f;
        config.distLeavePatrol = 450f;
        config.distLeaveDefend = 800f;
        config.distLeaveAttack = 250f;
        config.salvoCooldownAmount = 150f;
        config.canDodgeProj = true;
        config.distStartDodge = 50f;
        config.projDodgeCooldownVal = 20f;
        config.advancedDodge = true;
        config.rotationSpeed = 2.4f;
        config.aimingAngle = 45f;
        config.rotationMultMax = 4.0f;
        config.rotationMultMin = 0.75f;
        config.perfectAim = false;
        config.advancedTargeting = true;
        config.predictiveTargeting = true;
        config.predictiveTargetingChance = 50f;
        config.shootThreshold = 15f;
        config.safeThreshold = 60f;
        config.shootEnemyProjectiles = true;
        config.shootEnemyProjectilesRange = 100f;
        config.wanderRadius = 150f;
        config.defendTime = 800f;
        return config;
    }
    
    // 11. ZBrown - 進階棕色
    private AIConfig CreateZBrownConfig()
    {
        AIConfig config = new AIConfig();
        config.rotationSpeed = 0.8f;
        config.rotationMultMax = 1.0f;
        config.rotationMultMin = 0.75f;
        config.perfectAim = false;
        config.advancedTargeting = true;
        config.predictiveTargeting = false;
        config.predictiveTargetingChance = 50f;
        config.salvoCooldownAmount = 0f;
        config.aimingAngle = 5f;
        config.shootThreshold = 15f;
        config.safeThreshold = 60f;
        config.shootEnemyProjectiles = false;
        return config;
    }
    
    // 12. ZAsh - 進階灰色
    private AIConfig CreateZAshConfig()
    {
        AIConfig config = new AIConfig();
        config.patrolRadius = 250f;
        config.distLeavePatrol = 450f;
        config.distLeaveDefend = 800f;
        config.distLeaveAttack = 250f;
        config.canDodgeProj = true;
        config.distStartDodge = 50f;
        config.projDodgeCooldownVal = 50f;
        config.rotationSpeed = 0.8f;
        config.aimingAngle = 45f;
        config.rotationMultMax = 1.0f;
        config.rotationMultMin = 0.75f;
        config.perfectAim = false;
        config.advancedTargeting = true;
        config.predictiveTargeting = true;
        config.advancedDodge = true;
        config.predictiveTargetingChance = 60f;
        config.shootThreshold = 15f;
        config.safeThreshold = 60f;
        config.shootEnemyProjectiles = false;
        config.wanderRadius = 150f;
        config.defendTime = 50f;
        return config;
    }
    
    private AIConfig CreateDefaultConfig()
    {
        AIConfig config = new AIConfig();
        config.rotationSpeed = 1.0f;
        config.aimingAngle = 25f;
        config.patrolRadius = 200f;
        config.distLeavePatrol = 500f;
        config.distLeaveDefend = 800f;
        config.distLeaveAttack = 200f;
        config.canDodgeProj = true;
        config.canDodgeMine = true;
        config.advancedTargeting = true;
        config.predictiveTargeting = true;
        config.shootThreshold = 10f;
        config.safeThreshold = 50f;
        return config;
    }
}


// 主要AI參數設定
// 1. 移動與巡邏
// patrol_radius: 巡邏半徑
// dist_leave_patrol: 離開巡邏的距離
// dist_leave_defend: 離開防禦的距離
// dist_leave_attack: 離開攻擊的距離
// wander_radius: 遊蕩半徑
// random_movement: 是否隨機移動
// 2. 瞄準與射擊
// rotation_speed: 砲塔旋轉速度
// aiming_angle: 瞄準角度誤差
// perfect_aim: 是否完美瞄準
// advanced_targeting: 高級瞄準（射線檢測）
// predictive_targeting: 預測射擊
// predictive_targeting_chance: 預測射擊機率
// shoot_threshold: 射擊精確度閾值
// safe_threshold: 安全射擊閾值
// 3. 閃避系統
// can_dodge_proj: 能否閃避子彈
// can_dodge_mine: 能否閃避地雷
// dist_start_dodge: 開始閃避的距離
// proj_dodge_cooldown_val: 子彈閃避冷卻
// advanced_dodge: 高級閃避
/// 4. 特殊能力
// shoot_enemy_projectiles: 射擊敵方子彈
// mine_chance: 放置地雷機率
// salvo_cooldown_amount: 連射冷卻
// suicide: 自殺式攻擊
