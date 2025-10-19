using UnityEngine;

[CreateAssetMenu(menuName = "AI/Enemy AI Parameters", fileName = "EnemyAIParameters")]
public class EnemyAIParameters : ScriptableObject
{
    // --- Basic movement/combat fields kept for Unity gameplay ---
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 150f;
    public float detectionRange = 10f;
    public float shootingRange = 8f;
    public float minDistanceToPlayer = 3f;

    [Header("Combat Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 15f;
    public float fireRate = 0.5f;
    public float maxHealth = 1f;

    [Header("AI Behavior")]
    public float patrolRadius = 5f;
    public float patrolWaitTime = 2f;
    public LayerMask obstacleLayer = 1;

    // --- Mirrored fields from TanksRebirth AiParameters ---
    [Header("Mine Logic (TanksRebirth)")]
    public float obstacleAwarenessMine;
    public int randomTimerMaxMine = 2;
    public int randomTimerMinMine = 1;
    public float tankAwarenessMine;
    public float chanceMineLayNearBreakables;
    [Range(0f, 1f)] public float chanceMineLay;

    [Header("Movement Randomization (TanksRebirth)")]
    public float maxAngleRandomTurn;
    public int randomTimerMaxMove = 2;
    public int randomTimerMinMove = 1;
    public float awarenessFriendlyMine;
    public float awarenessFriendlyShell;
    public float awarenessHostileMine;
    public float awarenessHostileShell;
    public bool cantShootWhileFleeing;
    [Tooltip("-1..1 typical range")] public float aggressivenessBias;
    public int maxQueuedMovements = 4;
    public float obstacleAwarenessMovement;

    [Header("Aiming / Shooting (TanksRebirth)")]
    [Tooltip("Radians of inaccuracy from target ray")]
    public float aimOffsetRadians;
    public float detectionForgivenessSelf;
    public float detectionForgivenessFriendly;
    public float detectionForgivenessHostile;
    public int randomTimerMaxShoot = 2;
    public int randomTimerMinShoot = 1;
    public float turretSpeed;
    [Tooltip("Seconds between turret aim updates; 0 = every frame")]
    public float turretMovementTimer;
    public float tankAwarenessShoot;

    [Header("Rebirth Exclusive (TanksRebirth)")]
    public uint remembranceTicks = 60;
    public bool smartRicochets;
    public bool bounceReset = true;
    [Tooltip("Radians; default 5 degrees")]
    public float redirectAngleRadians = Mathf.Deg2Rad * 5f;
    public bool predictsPositions;
    public bool deflectsBullets;
    public bool shootsMinesSmartly;
    public float baseXP;
}


