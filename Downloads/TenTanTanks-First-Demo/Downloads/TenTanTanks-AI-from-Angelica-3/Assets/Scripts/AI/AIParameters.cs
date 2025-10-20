using UnityEngine;

[System.Serializable]
public class AIParameters
{
    [Header("Movement Settings")]
    [Range(0.1f, 10f)]
    public float maxAngleRandomTurn = 45f;
    
    [Range(0.5f, 5f)]
    public float randomTimerMinMove = 1f;
    
    [Range(0.5f, 5f)]
    public float randomTimerMaxMove = 3f;
    
    [Range(0.1f, 20f)]
    public float obstacleAwarenessMovement = 5f;
    
    [Range(-1f, 1f)]
    public float aggressivenessBias = 0.3f;
    
    [Range(1, 10)]
    public int maxQueuedMovements = 4;

    [Header("Combat Settings")]
    [Range(0.1f, 5f)]
    public float randomTimerMinShoot = 0.5f;
    
    [Range(0.1f, 5f)]
    public float randomTimerMaxShoot = 2f;
    
    [Range(0f, 1f)]
    public float aimOffset = 0.1f;
    
    [Range(0.1f, 10f)]
    public float turretSpeed = 2f;
    
    [Range(0.1f, 2f)]
    public float turretMovementTimer = 0.5f;
    
    [Range(1f, 20f)]
    public float tankAwarenessShoot = 10f;
    
    [Range(0.1f, 5f)]
    public float detectionForgivenessHostile = 1f;
    
    [Range(0.1f, 5f)]
    public float detectionForgivenessFriendly = 2f;
    
    [Range(0.1f, 5f)]
    public float detectionForgivenessSelf = 1f;

    [Header("Mine Settings")]
    [Range(0.1f, 5f)]
    public float randomTimerMinMine = 2f;
    
    [Range(0.1f, 5f)]
    public float randomTimerMaxMine = 5f;
    
    [Range(0f, 1f)]
    public float chanceMineLay = 0.3f;
    
    [Range(0f, 1f)]
    public float chanceMineLayNearBreakables = 0.6f;
    
    [Range(1f, 20f)]
    public float obstacleAwarenessMine = 5f;
    
    [Range(1f, 20f)]
    public float tankAwarenessMine = 8f;

    [Header("Awareness Settings")]
    [Range(1f, 20f)]
    public float awarenessFriendlyMine = 5f;
    
    [Range(1f, 20f)]
    public float awarenessFriendlyShell = 8f;
    
    [Range(1f, 20f)]
    public float awarenessHostileMine = 10f;
    
    [Range(1f, 20f)]
    public float awarenessHostileShell = 12f;

    [Header("Advanced Settings")]
    public bool cantShootWhileFleeing = true;
    public bool predictsPositions = false;
    public bool smartRicochets = false;
    public bool deflectsBullets = false;
    public bool bounceReset = true;
    
    [Range(0.1f, 10f)]
    public float redirectAngle = 5f;
    
    [Range(1, 300)]
    public int rememberance = 60;
    
    [Range(0.1f, 10f)]
    public float baseXP = 1f;

    // 預設參數配置
    public static AIParameters GetDefaultParameters()
    {
        return new AIParameters
        {
            maxAngleRandomTurn = 45f,
            randomTimerMinMove = 1f,
            randomTimerMaxMove = 3f,
            obstacleAwarenessMovement = 5f,
            aggressivenessBias = 0.3f,
            maxQueuedMovements = 4,
            randomTimerMinShoot = 0.5f,
            randomTimerMaxShoot = 2f,
            aimOffset = 0.1f,
            turretSpeed = 2f,
            turretMovementTimer = 0.5f,
            tankAwarenessShoot = 10f,
            detectionForgivenessHostile = 1f,
            detectionForgivenessFriendly = 2f,
            detectionForgivenessSelf = 1f,
            randomTimerMinMine = 2f,
            randomTimerMaxMine = 5f,
            chanceMineLay = 0.3f,
            chanceMineLayNearBreakables = 0.6f,
            obstacleAwarenessMine = 5f,
            tankAwarenessMine = 8f,
            awarenessFriendlyMine = 5f,
            awarenessFriendlyShell = 8f,
            awarenessHostileMine = 10f,
            awarenessHostileShell = 12f,
            cantShootWhileFleeing = true,
            predictsPositions = false,
            smartRicochets = false,
            deflectsBullets = false,
            bounceReset = true,
            redirectAngle = 5f,
            rememberance = 60,
            baseXP = 1f
        };
    }
}
