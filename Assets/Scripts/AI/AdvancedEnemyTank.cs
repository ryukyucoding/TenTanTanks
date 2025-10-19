using System.Collections.Generic;
using UnityEngine;

public class AdvancedEnemyTank : MonoBehaviour, IDamageable
{
    [Header("Tank Components")]
    [SerializeField] private Transform tankBody;
    [SerializeField] private Transform turret;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Rigidbody rb;
    
    [Header("AI Settings")]
    [SerializeField] private string aiPersonality = "brown";
    [SerializeField] private string tankUnitType = "brown_tank";
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    // AI Components
    private AIConfig aiConfig;
    private TankUnitConfig unitConfig;
    private AIStateData stateData;
    private AStarPathfinding pathfinding;
    
    // Targeting
    private Transform player;
    private List<Transform> potentialTargets = new List<Transform>();
    private Transform currentTarget;
    private bool targetInSight = false;
    private float distanceToTarget = 9999f;
    
    // Movement
    private List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex = 0;
    private Vector3 patrolCenter;
    private Vector3 currentPatrolTarget;
    private bool isMoving = false;
    
    // Combat
    private float nextFireTime = 0f;
    private float salvoCooldown = 0f;
    private float dodgeCooldown = 0f;
    private float defendTimer = 0f;
    
    // Projectile Detection
    private Transform closestProjectile;
    private float closestProjectileDistance = 9999f;
    private Vector3 predictedProjectileDirection;
    
    // Mine Detection
    private Transform closestMine;
    private float closestMineDistance = 9999f;
    
    // State Timers
    private float stateTimer = 0f;
    private float stuckTimer = 0f;
    private Vector3 lastValidPosition;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
        
        patrolCenter = transform.position;
        lastValidPosition = transform.position;
        
        // 初始化AI配置
        LoadAIConfig();
    }
    
    void Start()
    {
        // 找到玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            potentialTargets.Add(player);
        }
        
        // 初始化路徑尋找
        pathfinding = FindFirstObjectByType<AStarPathfinding>();
        if (pathfinding == null)
        {
            GameObject pathfindingObj = new GameObject("Pathfinding");
            pathfinding = pathfindingObj.AddComponent<AStarPathfinding>();
        }
        
        // 初始化狀態
        stateData = new AIStateData();
        stateData.ChangeState(AIBehaviorState.Idle);
        
        SetNewPatrolTarget();
    }
    
    void Update()
    {
        if (stateData.IsInState(AIBehaviorState.Dead)) return;
        
        UpdateAI();
        UpdateTimers();
        CheckStuck();
    }
    
    void FixedUpdate()
    {
        if (stateData.IsInState(AIBehaviorState.Dead)) return;
        
        // HandleMovement is now integrated into UpdateAI
    }
    
    private void LoadAIConfig()
    {
        aiConfig = AIConfigLoader.GetAIConfig(aiPersonality);
        unitConfig = AIConfigLoader.GetTankUnitConfig(tankUnitType);
        
        // 應用單位配置到坦克
        if (rb != null)
        {
            rb.mass = 1f / unitConfig.tankSpeedModifier;
        }
    }
    
    private void UpdateAI()
    {
        if (currentTarget == null) return;
        
        UpdateTargeting();
        UpdateProjectileDetection();
        UpdateMineDetection();
        
        // 檢查閃避
        if (aiConfig.canDodgeProj && closestProjectile != null && 
            closestProjectileDistance < aiConfig.distStartDodge && dodgeCooldown <= 0f)
        {
            stateData.ChangeState(AIBehaviorState.Dodge);
            HandleDodge();
            dodgeCooldown = aiConfig.projDodgeCooldownVal;
            return;
        }
        
        if (aiConfig.canDodgeMine && closestMine != null && 
            closestMineDistance < aiConfig.avoidMineDist && dodgeCooldown <= 0f)
        {
            HandleMineAvoidance();
            dodgeCooldown = aiConfig.mineDodgeCooldownVal;
        }
        
        // 處理AI狀態
        switch (stateData.currentState)
        {
            case AIBehaviorState.Idle:
                HandleIdleState();
                break;
            case AIBehaviorState.Patrolling:
                HandlePatrolState();
                break;
            case AIBehaviorState.Defending:
                HandleDefendState();
                break;
            case AIBehaviorState.Attacking:
                HandleAttackState();
                break;
            case AIBehaviorState.Wander:
                HandleWanderState();
                break;
            case AIBehaviorState.Dodge:
                HandleDodgeState();
                break;
        }
    }
    
    private void HandleIdleState()
    {
        if (aiConfig.randomMovement)
        {
            stateData.ChangeState(AIBehaviorState.Wander);
        }
        else
        {
            stateData.ChangeState(AIBehaviorState.Patrolling);
        }
    }
    
    private void HandlePatrolState()
    {
        if (targetInSight && distanceToTarget < aiConfig.distLeavePatrol)
        {
            stateData.ChangeState(AIBehaviorState.Defending);
            return;
        }
        
        if (currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            SetNewPatrolTarget();
        }
        else
        {
            MoveTowards(currentPath[currentPathIndex]);
            
            if (Vector3.Distance(transform.position, currentPath[currentPathIndex]) < 1f)
            {
                currentPathIndex++;
            }
        }
    }
    
    private void HandleDefendState()
    {
        if (!targetInSight || distanceToTarget > aiConfig.distLeaveDefend)
        {
            stateData.ChangeState(AIBehaviorState.Patrolling);
            return;
        }
        
        if (distanceToTarget <= aiConfig.distLeaveAttack)
        {
            stateData.ChangeState(AIBehaviorState.Attacking);
            return;
        }
        
        MoveTowards(currentTarget.position);
    }
    
    private void HandleAttackState()
    {
        if (!targetInSight || distanceToTarget > aiConfig.distLeaveAttack * 1.2f)
        {
            stateData.ChangeState(AIBehaviorState.Defending);
            return;
        }
        
        // 保持適當距離
        if (distanceToTarget < 3f)
        {
            Vector3 direction = (transform.position - currentTarget.position).normalized;
            MoveTowards(transform.position + direction * 2f);
        }
        else if (distanceToTarget > aiConfig.distLeaveAttack)
        {
            MoveTowards(currentTarget.position);
        }
        
        // 射擊
        TryShoot();
    }
    
    private void HandleWanderState()
    {
        if (targetInSight && distanceToTarget < aiConfig.distLeavePatrol)
        {
            stateData.ChangeState(AIBehaviorState.Defending);
            return;
        }
        
        if (currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            SetRandomTarget();
        }
        else
        {
            MoveTowards(currentPath[currentPathIndex]);
            
            if (Vector3.Distance(transform.position, currentPath[currentPathIndex]) < 1f)
            {
                currentPathIndex++;
            }
        }
    }
    
    private void HandleDodgeState()
    {
        if (closestProjectile == null || closestProjectileDistance > aiConfig.distStartDodge)
        {
            stateData.ChangeState(AIBehaviorState.Defending);
            return;
        }
        
        if (currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            stateData.ChangeState(AIBehaviorState.Defending);
            return;
        }
        
        MoveTowards(currentPath[currentPathIndex]);
        
        if (Vector3.Distance(transform.position, currentPath[currentPathIndex]) < 1f)
        {
            currentPathIndex++;
        }
    }
    
    private void UpdateTargeting()
    {
        if (currentTarget == null && potentialTargets.Count > 0)
        {
            currentTarget = potentialTargets[0];
        }
        
        if (currentTarget != null)
        {
            distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            targetInSight = CanSeeTarget(currentTarget);
            
            if (targetInSight)
            {
                RotateTurretTowards(currentTarget.position);
            }
        }
    }
    
    private void UpdateProjectileDetection()
    {
        closestProjectile = null;
        closestProjectileDistance = 9999f;
        
        // 檢測所有子彈
        Bullet[] bullets = FindObjectsByType<Bullet>(FindObjectsSortMode.None);
        foreach (Bullet bullet in bullets)
        {
            if (bullet.gameObject == gameObject) continue; // 忽略自己的子彈
            
            float distance = Vector3.Distance(transform.position, bullet.transform.position);
            if (distance < closestProjectileDistance)
            {
                closestProjectile = bullet.transform;
                closestProjectileDistance = distance;
                
                // 預測子彈方向
                if (aiConfig.advancedDodge)
                {
                    predictedProjectileDirection = bullet.transform.forward;
                }
            }
        }
    }
    
    private void UpdateMineDetection()
    {
        closestMine = null;
        closestMineDistance = 9999f;
        
        // 檢測所有地雷
        Mine[] mines = FindObjectsByType<Mine>(FindObjectsSortMode.None);
        foreach (Mine mine in mines)
        {
            float distance = Vector3.Distance(transform.position, mine.transform.position);
            if (distance < closestMineDistance)
            {
                closestMine = mine.transform;
                closestMineDistance = distance;
            }
        }
    }
    
    private bool CanSeeTarget(Transform target)
    {
        if (target == null) return false;
        
        Vector3 direction = (target.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);
        
        // 射線檢測障礙物
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, distance, aiConfig.obstacleLayer))
        {
            return false;
        }
        
        return true;
    }
    
    private void HandleDodge()
    {
        if (closestProjectile == null) return;
        
        Vector3 projectileDirection = aiConfig.advancedDodge ? 
            predictedProjectileDirection : closestProjectile.forward;
        
        // 計算垂直閃避方向
        Vector3 dodgeDirection1 = Vector3.Cross(projectileDirection, Vector3.up).normalized;
        Vector3 dodgeDirection2 = -dodgeDirection1;
        
        // 選擇較好的閃避方向
        Vector3 dodgeTarget1 = transform.position + dodgeDirection1 * 3f;
        Vector3 dodgeTarget2 = transform.position + dodgeDirection2 * 3f;
        
        Vector3 chosenTarget = Vector3.Distance(dodgeTarget1, closestProjectile.position) > 
                              Vector3.Distance(dodgeTarget2, closestProjectile.position) ? 
                              dodgeTarget1 : dodgeTarget2;
        
        // 尋找路徑
        currentPath = pathfinding.FindPath(transform.position, chosenTarget);
        currentPathIndex = 0;
    }
    
    private void HandleMineAvoidance()
    {
        if (closestMine == null) return;
        
        // 尋找遠離地雷的安全位置
        List<PathfindingNode> safeNodes = pathfinding.GetNodesInRadius(transform.position, 10f);
        PathfindingNode bestNode = null;
        float bestDistance = 0f;
        
        foreach (PathfindingNode node in safeNodes)
        {
            float distanceToMine = Vector3.Distance(node.position, closestMine.position);
            if (distanceToMine > bestDistance)
            {
                bestDistance = distanceToMine;
                bestNode = node;
            }
        }
        
        if (bestNode != null)
        {
            currentPath = pathfinding.FindPath(transform.position, bestNode.position);
            currentPathIndex = 0;
        }
    }
    
    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        
        if (direction.magnitude > 0.1f)
        {
            Vector3 movement = direction * unitConfig.tankSpeedModifier * Time.fixedDeltaTime;
            rb.MovePosition(transform.position + movement);
            
            // 旋轉坦克朝向移動方向
            if (tankBody != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                tankBody.rotation = Quaternion.Slerp(tankBody.rotation, targetRotation, 
                    aiConfig.rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }
    
    private void RotateTurretTowards(Vector3 targetPosition)
    {
        if (turret == null) return;
        
        Vector3 direction = (targetPosition - turret.position).normalized;
        direction.y = 0;
        
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            turret.rotation = Quaternion.Slerp(turret.rotation, targetRotation, 
                aiConfig.rotationSpeed * Time.deltaTime);
        }
    }
    
    private void TryShoot()
    {
        if (Time.time >= nextFireTime && salvoCooldown <= 0f)
        {
            nextFireTime = Time.time + (1f / unitConfig.firerate);
            salvoCooldown = aiConfig.salvoCooldownAmount;
            
            // 這裡需要實現射擊邏輯
            // ShootBullet();
        }
    }
    
    private void SetNewPatrolTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * aiConfig.patrolRadius;
        randomDirection.y = 0;
        currentPatrolTarget = patrolCenter + randomDirection;
        
        currentPath = pathfinding.FindPath(transform.position, currentPatrolTarget);
        currentPathIndex = 0;
    }
    
    private void SetRandomTarget()
    {
        PathfindingNode randomNode = pathfinding.GetRandomWalkableNode();
        if (randomNode != null)
        {
            currentPath = pathfinding.FindPath(transform.position, randomNode.position);
            currentPathIndex = 0;
        }
    }
    
    private void UpdateTimers()
    {
        if (salvoCooldown > 0f) salvoCooldown -= Time.deltaTime;
        if (dodgeCooldown > 0f) dodgeCooldown -= Time.deltaTime;
        if (defendTimer > 0f) defendTimer -= Time.deltaTime;
        
        stateTimer += Time.deltaTime;
    }
    
    private void CheckStuck()
    {
        if (Vector3.Distance(transform.position, lastValidPosition) < 0.1f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= 2f)
            {
                SetNewPatrolTarget();
                stuckTimer = 0f;
            }
        }
        else
        {
            lastValidPosition = transform.position;
            stuckTimer = 0f;
        }
    }
    
    // IDamageable implementation
    public void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker)
    {
        // 實現受傷邏輯
        if (attacker != null && attacker.CompareTag("Player"))
        {
            currentTarget = attacker.transform;
            stateData.ChangeState(AIBehaviorState.Defending);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        // 繪製AI狀態信息
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aiConfig.distLeavePatrol);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aiConfig.distLeaveAttack);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(patrolCenter, aiConfig.patrolRadius);
        
        // 繪製當前路徑
        if (currentPath.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
        }
    }
}
