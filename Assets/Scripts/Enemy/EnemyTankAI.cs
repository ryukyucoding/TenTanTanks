using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyTankAI : MonoBehaviour, IDamageable
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 150f;
    [SerializeField] private float rotationThreshold = 10f;  // 旋轉閾值，避免微小抖動
    [SerializeField] private float rotationSmoothing = 0.3f; // 旋轉平滑度
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float shootingRange = 8f;

    [Header("Tank Parts")]
    [SerializeField] private Transform tankBody;
    [SerializeField] private Transform turret;
    [SerializeField] private Transform firePoint;

    [Header("Combat Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float maxHealth = 1f;

    [Header("AI System")]
    [SerializeField] private AIParameters aiParameters = new AIParameters();
    // [SerializeField] private LayerMask obstacleLayer = 1;
    
    [Header("Detection Settings")]
    [SerializeField] private LayerMask playerLayer = 1;  // 玩家層級 (Layer 0)
    [SerializeField] private LayerMask wallLayer = 64;   // 牆壁層級 (Layer 6)
    [SerializeField] private LayerMask obstacleLayer = 128; // 障礙物層級 (Layer 7)
    [SerializeField] private LayerMask bulletLayer = 256; // 子彈層級 (Layer 8)
    [SerializeField] private LayerMask groundLayer = 1;  // 地面層級 (Layer 0)
    [SerializeField] private LayerMask obstacleLayerMask = 192; // 障礙物層級遮罩 (Layer 6 + 7)
    [SerializeField] private float visionHeight = 0.5f;  // 視線高度
    [SerializeField] private float visionCheckInterval = 0.1f; // 視覺檢測間隔
    [SerializeField] private float pathfindingCheckRadius = 0.8f; // 路徑尋找檢測半徑
    private float lastVisionCheck = 0f;
    
    [Header("Boundary Settings")]
    [SerializeField] private float boundaryX = 20f;  // X軸邊界
    [SerializeField] private float boundaryZ = 20f;  // Z軸邊界

    // AI系統變數
    private AIBehavior[] behaviors;
    private Transform player;
    private Rigidbody rb;
    
    // 平滑移動變數
    private Vector3 smoothedTargetPosition;
    private float targetSmoothingSpeed = 2f;
    
    // 目標重新檢測
    private float lastTargetCheck = 0f;
    private float targetCheckInterval = 1f; // 每秒檢查一次目標
    
    // AI狀態機
    public enum AIState
    {
        Patrol,     // 巡邏
        Chase,      // 追擊
        Attack,     // 攻擊
        Dead,       // 死亡
        Surviving   // 生存模式（躲避危險）
    }
    
    [Header("AI State")]
    [SerializeField] private AIState currentState = AIState.Patrol;
    private AIState previousState;
    
    // 路徑尋找系統
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int currentPathIndex = 0;
    private bool hasValidPath = false;
    
    // 健康系統
    private float currentHealth;
    private float nextFireTime;
    
    // 移動系統
    private Vector3 patrolCenter;
    private Vector3 currentPatrolTarget;
    
    // 路徑尋找（移除重複定義，使用上面的Vector2版本）
    private Vector3 targetPosition;
    
    // 障礙物檢測
    private Vector3 lastValidPosition;
    private float stuckTimer = 0f;
    private float stuckCheckInterval = 2f;
    
    // AI狀態
    private bool isSurviving = false;
    private bool doMovements = true;
    
    // 隨機移動計時器
    private int currentRandomMove;
    private int currentRandomShoot;
    private int currentRandomMineLay;
    
    // 目標追蹤
    private Transform targetTank;
    private Vector3 aimTarget;
    private bool seesTarget = false;
    private float targetTurretRotation;
    private float turretRotationMultiplier = 1f;
    
    // 移動隊列系統
    private Queue<Vector3> pivotQueue = new Queue<Vector3>();
    private Queue<Vector3> subPivotQueue = new Queue<Vector3>();
    
    // 危險檢測
    private List<GameObject> nearbyDangers = new List<GameObject>();
    private GameObject closestDanger;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        currentHealth = maxHealth;
        patrolCenter = transform.position;
        lastValidPosition = transform.position;

        // 初始化AI行為
        InitializeAIBehaviors();
        
        // 設置預設AI參數
        if (aiParameters == null)
            aiParameters = AIParameters.GetDefaultParameters();
            
        // 初始化隨機計時器
        InitializeRandomTimers();
    }

    void Start()
    {
        // 自動檢測邊界
        AutoDetectBoundaries();
        
        // 尋找玩家
        FindPlayerTarget();

        if (player == null)
        {
            Debug.LogWarning("EnemyTankAI: No player found");
        }
        else
        {
            Debug.Log("EnemyTankAI: Player target found: " + player.name);
        }
        
        // 初始化平滑目標位置
        smoothedTargetPosition = transform.position;
    }

    private void AutoDetectBoundaries()
    {
        // 尋找場景中的邊界物件
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        
        foreach (var obj in allObjects)
        {
            // 檢查是否是邊界牆壁
            if (obj.name.ToLower().Contains("wall") || obj.name.ToLower().Contains("boundary") || 
                obj.name.ToLower().Contains("cube") || obj.name.ToLower().Contains("barrier"))
            {
                Vector3 pos = obj.transform.position;
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minZ = Mathf.Min(minZ, pos.z);
                maxZ = Mathf.Max(maxZ, pos.z);
            }
        }
        
        // 如果找到了邊界物件，設置邊界
        if (minX != float.MaxValue)
        {
            boundaryX = Mathf.Max(Mathf.Abs(minX), Mathf.Abs(maxX)) - 1f; // 留1單位緩衝
            boundaryZ = Mathf.Max(Mathf.Abs(minZ), Mathf.Abs(maxZ)) - 1f;
            Debug.Log($"EnemyTankAI: Auto-detected boundaries - X: ±{boundaryX}, Z: ±{boundaryZ}");
        }
        else
        {
            Debug.Log("EnemyTankAI: No boundaries detected, using default values");
        }
    }

    void Update()
    {
        if (currentHealth <= 0) 
        {
            currentState = AIState.Dead;
            return;
        }

        // 更新AI行為計時器
        UpdateAIBehaviors();

        // 更新AI狀態機
        UpdateAIStateMachine();

        // 根據當前狀態執行相應行為
        ExecuteCurrentStateBehavior();

        // 檢查是否卡住
        CheckStuck();

        // 調試信息
        if (Time.frameCount % 60 == 0) // 每秒輸出一次
        {
            Debug.Log($"EnemyTankAI: State={currentState}, Target={targetTank?.name}, SeesTarget={seesTarget}, Distance={Vector3.Distance(transform.position, targetTank?.position ?? Vector3.zero):F1}");
        }
    }

    void FixedUpdate()
    {
        if (currentHealth <= 0) return;

        // 移動邏輯在Update中處理
    }

    private void InitializeAIBehaviors()
    {
        behaviors = new AIBehavior[4];
        behaviors[0] = new AIBehavior("TankChassisMovement");
        behaviors[1] = new AIBehavior("TankTurretMovement");
        behaviors[2] = new AIBehavior("TankShellFire");
        behaviors[3] = new AIBehavior("TankMinePlacement");
    }

    private void UpdateAIStateMachine()
    {
        // 檢查危險
        CheckDangers();
        
        // 檢查玩家可見性
        UpdateAim(); // 使用UpdateAim方法來檢查玩家可見性
        
        // 狀態轉換邏輯
        AIState newState = DetermineNextState();
        
        if (newState != currentState)
        {
            previousState = currentState;
            currentState = newState;
            OnStateChanged(previousState, currentState);
        }
    }

    private AIState DetermineNextState()
    {
        // 如果死亡，保持死亡狀態
        if (currentHealth <= 0)
            return AIState.Dead;
            
        // 如果有危險，進入生存模式
        if (isSurviving)
            return AIState.Surviving;
            
        // 如果有目標，根據距離決定狀態
        if (targetTank != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetTank.position);
            
            // 如果能看到目標且在射擊範圍內，進入攻擊狀態
            if (seesTarget && distanceToTarget <= shootingRange)
                return AIState.Attack;
            // 如果能看到目標但距離太遠，追擊
            else if (seesTarget && distanceToTarget > shootingRange)
                return AIState.Chase;
            // 如果看不到目標但在檢測範圍內，也追擊
            else if (distanceToTarget <= detectionRange)
                return AIState.Chase;
        }
        
        // 如果沒有目標或超出檢測範圍，巡邏
        return AIState.Patrol;
    }

    private void OnStateChanged(AIState fromState, AIState toState)
    {
        Debug.Log($"EnemyTankAI: State changed from {fromState} to {toState}");
        
        switch (toState)
        {
            case AIState.Patrol:
                SetNewPatrolTarget();
                break;
            case AIState.Chase:
                // 清除當前路徑，重新計算
                currentPath.Clear();
                hasValidPath = false;
                break;
            case AIState.Attack:
                // 停止移動，專注攻擊
                break;
            case AIState.Surviving:
                // 尋找安全位置
                break;
        }
    }

    private void ExecuteCurrentStateBehavior()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                ExecutePatrolBehavior();
                break;
            case AIState.Chase:
                ExecuteChaseBehavior();
                break;
            case AIState.Attack:
                ExecuteAttackBehavior();
                break;
            case AIState.Surviving:
                ExecuteSurvivingBehavior();
                break;
            case AIState.Dead:
                ExecuteDeadBehavior();
                break;
        }
    }

    private void InitializeRandomTimers()
    {
        currentRandomMove = Random.Range((int)aiParameters.randomTimerMinMove, (int)aiParameters.randomTimerMaxMove + 1);
        currentRandomShoot = Random.Range((int)aiParameters.randomTimerMinShoot, (int)aiParameters.randomTimerMaxShoot + 1);
        currentRandomMineLay = Random.Range((int)aiParameters.randomTimerMinMine, (int)aiParameters.randomTimerMaxMine + 1);
    }

    private void UpdateAIBehaviors()
    {
        // 更新行為計時器
        foreach (var behavior in behaviors)
        {
            behavior.value += Time.deltaTime;
        }
    }

    private void UpdateAI()
    {
        if (player == null) return;

        // 更新行為計時器
        foreach (var behavior in behaviors)
        {
            behavior.value += Time.deltaTime;
        }

        // 處理砲塔
        HandleTurret();
        
        // 處理移動
        if (doMovements)
        {
            DoMovement();
        }

        // 調試信息
        if (Time.frameCount % 60 == 0) // 每秒輸出一次
        {
            Debug.Log($"EnemyTankAI: Target={targetTank?.name}, SeesTarget={seesTarget}, Distance={Vector3.Distance(transform.position, targetTank?.position ?? Vector3.zero):F1}");
        }
    }

    private void HandleTurret()
    {
        if (turret == null) return;

        // 標準化角度
        targetTurretRotation %= 360f;
        float currentTurretRotation = turret.eulerAngles.y;
        
        // 計算角度差
        float angleDiff = targetTurretRotation - currentTurretRotation;
        if (angleDiff > 180f)
            targetTurretRotation -= 360f;
        else if (angleDiff < -180f)
            targetTurretRotation += 360f;

        // 旋轉砲塔
        float newRotation = Mathf.LerpAngle(currentTurretRotation, targetTurretRotation, 
            aiParameters.turretSpeed * turretRotationMultiplier * Time.deltaTime);
        turret.rotation = Quaternion.Euler(0, newRotation, 0);

        // 更新瞄準
        if (targetTank != null)
        {
            UpdateAim();
        }

        // 處理射擊 - 簡化射擊邏輯，當看到目標時就射擊
        if (seesTarget && Time.time >= nextFireTime)
        {
            TryShoot();
        }
    }

    private void UpdateAim()
    {
        if (targetTank == null) return;

        // 限制檢測頻率以提高性能
        if (Time.time - lastVisionCheck < visionCheckInterval) return;
        lastVisionCheck = Time.time;

        seesTarget = false;
        
        // 檢查是否在檢測範圍內
        float distance = Vector3.Distance(transform.position, targetTank.position);
        if (distance > detectionRange)
        {
            return; // 超出檢測範圍
        }
        
        // 檢查是否能看到目標
        Vector3 directionToTarget = (targetTank.position - transform.position).normalized;
        Vector3 rayStart = transform.position + Vector3.up * visionHeight;

        // 使用更精確的射線檢測
        RaycastHit hit;
        if (Physics.Raycast(rayStart, directionToTarget, out hit, distance, obstacleLayerMask))
        {
            // 檢查擊中的是否為目標本身
            if (hit.collider.transform != targetTank)
            {
                return; // 被障礙物阻擋
            }
        }

        seesTarget = true;
        aimTarget = targetTank.position;

        // 預測目標位置
        if (aiParameters.predictsPositions)
        {
            Rigidbody targetRb = targetTank.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                float timeToTarget = distance / bulletSpeed;
                aimTarget = targetTank.position + targetRb.linearVelocity * timeToTarget;
            }
        }

        // 計算瞄準角度
        Vector3 aimDirection = (aimTarget - turret.position).normalized;
        aimDirection.y = 0;
        
        if (aimDirection.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(aimDirection.x, aimDirection.z) * Mathf.Rad2Deg;
            targetTurretRotation = targetAngle + Random.Range(-aiParameters.aimOffset, aiParameters.aimOffset);
        }
        
        // 調試射線
        Debug.DrawRay(rayStart, directionToTarget * distance, seesTarget ? Color.green : Color.red, 0.1f);
    }

    private void DoMovement()
    {
        if (currentHealth <= 0) return;

        // 檢查危險
        CheckDangers();

        // 如果有目標，優先追擊目標
        if (targetTank != null && !isSurviving)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetTank.position);
            
            // 如果距離目標太遠，移動向目標
            if (distanceToTarget > shootingRange)
            {
                // 平滑目標位置，避免頻繁變化導致的快速轉動
                smoothedTargetPosition = Vector3.Lerp(smoothedTargetPosition, targetTank.position, targetSmoothingSpeed * Time.deltaTime);
                MoveTowards(smoothedTargetPosition);
            }
            else
            {
                // 距離適中時保持位置，不後退（移除後退邏輯）
                // 停止移動但保持當前朝向
            }
        }
        else if (!isSurviving)
        {
            // 沒有目標時進行巡邏
            DoPatrol();
        }

        // 處理障礙物導航
        DoBlockNavigation();
    }

    private void CheckDangers()
    {
        nearbyDangers.Clear();
        
        // 檢測附近的子彈（通過Layer檢測，更高效）
        Collider[] bulletColliders = Physics.OverlapSphere(transform.position, aiParameters.awarenessHostileShell, bulletLayer);
        foreach (var collider in bulletColliders)
        {
            if (collider != null && collider.gameObject != null)
            {
                nearbyDangers.Add(collider.gameObject);
            }
        }

        // 找到最近的危險
        if (nearbyDangers.Count > 0)
        {
            closestDanger = nearbyDangers[0];
            float closestDistance = Vector3.Distance(transform.position, closestDanger.transform.position);
            
            foreach (var danger in nearbyDangers)
            {
                if (danger != null)
                {
                    float distance = Vector3.Distance(transform.position, danger.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDanger = danger;
                        closestDistance = distance;
                    }
                }
            }
            
            // 避開危險
            AvoidDanger();
        }
    }

    private void AvoidDanger()
    {
        if (closestDanger == null) return;

        Vector3 dangerDirection = (transform.position - closestDanger.transform.position).normalized;
        Vector3 avoidPosition = transform.position + dangerDirection * 5f;
        
        subPivotQueue.Clear();
        pivotQueue.Clear();
        
        MoveTowards(avoidPosition);
        isSurviving = true;
    }

    private void DoPatrol()
    {
        // 如果沒有巡邏目標或已到達目標，設置新的巡邏目標
        if (Vector3.Distance(transform.position, currentPatrolTarget) < 1f)
        {
            SetNewPatrolTarget();
        }
        
        // 移動向巡邏目標
        MoveTowards(currentPatrolTarget);
    }

    private void SetNewPatrolTarget()
    {
        // 在巡邏中心周圍隨機選擇一個點
        Vector3 randomDirection = Random.insideUnitSphere * 5f;
        randomDirection.y = 0;
        currentPatrolTarget = patrolCenter + randomDirection;
    }

    private void DoBlockNavigation()
    {
        if (isSurviving) return;

        // 檢查前方是否有障礙物
        Vector3 forward = transform.forward;
        float checkDistance = aiParameters.obstacleAwarenessMovement / 2f;
        
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, forward, checkDistance, obstacleLayerMask))
        {
            // 檢查左右兩側
            Vector3 leftDirection = Quaternion.Euler(0, -45f, 0) * forward;
            Vector3 rightDirection = Quaternion.Euler(0, 45f, 0) * forward;
            
            bool leftBlocked = Physics.Raycast(transform.position + Vector3.up * 0.5f, leftDirection, checkDistance, obstacleLayerMask);
            bool rightBlocked = Physics.Raycast(transform.position + Vector3.up * 0.5f, rightDirection, checkDistance, obstacleLayerMask);
            
            Vector3 avoidDirection;
            if (!leftBlocked && !rightBlocked)
            {
                // 兩邊都可以走，隨機選擇
                avoidDirection = Random.Range(0, 2) == 0 ? leftDirection : rightDirection;
            }
            else if (!leftBlocked)
            {
                avoidDirection = leftDirection;
            }
            else if (!rightBlocked)
            {
                avoidDirection = rightDirection;
            }
            else
            {
                // 兩邊都被阻擋，後退
                avoidDirection = -forward;
            }
            
            Vector3 avoidPosition = transform.position + avoidDirection * 5f;
            MoveTowards(avoidPosition);
        }
    }

    // 簡化的移動隊列系統 - 暫時不使用複雜的隊列邏輯
    private void TryGenerateSubQueue()
    {
        // 簡化版本，直接處理移動
    }

    private void TryWorkSubQueue()
    {
        // 簡化版本，直接處理移動
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            // 旋轉車身朝向目標
            if (tankBody != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                float currentYRotation = tankBody.eulerAngles.y;
                float targetYRotation = targetRotation.eulerAngles.y;
                
                // 計算角度差，確保選擇最短旋轉路徑
                float angleDifference = Mathf.DeltaAngle(currentYRotation, targetYRotation);
                
                // 只有當角度差足夠大時才旋轉，避免微小抖動
                if (Mathf.Abs(angleDifference) > rotationThreshold)
                {
                    // 使用更平滑的旋轉，降低旋轉速度
                    float rotationStep = rotationSpeed * rotationSmoothing * Time.deltaTime;
                    float newYRotation = Mathf.MoveTowardsAngle(currentYRotation, targetYRotation, rotationStep);
                    tankBody.rotation = Quaternion.Euler(0, newYRotation, 0);
                }
            }

            // 計算新位置
            Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;
            
            // 檢查是否會撞到障礙物
            if (!WouldCollideWithObstacle(transform.position, newPosition))
            {
                // 檢查邊界限制
                newPosition = ClampToBoundary(newPosition);
                
                // 移動（使用Transform而不是Rigidbody以獲得更平滑的移動）
                transform.position = newPosition;
            }
            else
            {
                Debug.Log("EnemyTankAI: Movement blocked by obstacle, trying to go around");
                // 嘗試繞行
                TryGoAroundObstacle(direction);
            }
        }
    }

    private Vector3 ClampToBoundary(Vector3 position)
    {
        // 限制在邊界內
        position.x = Mathf.Clamp(position.x, -boundaryX, boundaryX);
        position.z = Mathf.Clamp(position.z, -boundaryZ, boundaryZ);
        return position;
    }

    private void TryShoot()
    {
        if (Time.time >= nextFireTime && bulletPrefab != null && firePoint != null && seesTarget)
        {
            nextFireTime = Time.time + (1f / fireRate);

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = firePoint.forward * bulletSpeed;
            }

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetShooter(gameObject);
            }
        }
    }

    private void CheckStuck()
    {
        if (Vector3.Distance(transform.position, lastValidPosition) < 0.1f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckCheckInterval)
            {
                // 卡住了，重新設置巡邏目標
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

    // IDamageable介面實現
    public void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker)
    {
        currentHealth -= damage;

        Debug.Log($"Enemy tank took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 受到攻擊時進入追擊狀態
            if (attacker != null && attacker.CompareTag("Player"))
            {
                targetTank = attacker.transform;
                isSurviving = false;
            }
        }
    }

    private void Die()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDestroyed();
        }

        Debug.Log("Enemy tank destroyed!");
        Destroy(gameObject, 1f);
    }

    // 調試用Gizmos - 始終顯示
    void OnDrawGizmos()
    {
        // 檢測範圍
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 射擊範圍
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        // 巡邏範圍
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(patrolCenter, 5f);

        // 當前目標
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(currentPatrolTarget, 0.5f);

        // 邊界可視化
        Gizmos.color = Color.white;
        Vector3 boundaryCenter = Vector3.zero;
        Vector3 boundarySize = new Vector3(boundaryX * 2, 0.1f, boundaryZ * 2);
        Gizmos.DrawWireCube(boundaryCenter, boundarySize);

        // 路徑尋找檢測範圍
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, pathfindingCheckRadius);

        // 路徑
        if (currentPath.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Vector3 start = new Vector3(currentPath[i].x, 0, currentPath[i].y);
                Vector3 end = new Vector3(currentPath[i + 1].x, 0, currentPath[i + 1].y);
                Gizmos.DrawLine(start, end);
            }
        }
        
        // 顯示isWalkable檢測網格（可選）
        DrawWalkableGrid();
    }
    
    private void DrawWalkableGrid()
    {
        // 在AI周圍顯示一個小網格，顯示哪些位置是可通行的
        int gridSize = 10;
        Vector3 aiPos = transform.position;
        
        for (int x = -gridSize/2; x <= gridSize/2; x++)
        {
            for (int z = -gridSize/2; z <= gridSize/2; z++)
            {
                Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(aiPos.x) + x, Mathf.RoundToInt(aiPos.z) + z);
                bool walkable = IsPositionWalkable(gridPos);
                
                Vector3 worldPos = new Vector3(gridPos.x, aiPos.y, gridPos.y);
                Gizmos.color = walkable ? Color.green : Color.red;
                Gizmos.DrawWireCube(worldPos, Vector3.one * 0.5f);
            }
        }
    }

    // 調試用Gizmos - 選中時顯示
    void OnDrawGizmosSelected()
    {
        // 檢測範圍
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 射擊範圍
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        // 巡邏範圍
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(patrolCenter, 5f);

        // 當前目標
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(currentPatrolTarget, 0.5f);

        // 邊界可視化
        Gizmos.color = Color.white;
        Vector3 boundaryCenter = Vector3.zero;
        Vector3 boundarySize = new Vector3(boundaryX * 2, 0.1f, boundaryZ * 2);
        Gizmos.DrawWireCube(boundaryCenter, boundarySize);

        // 路徑
        if (currentPath.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Vector3 start = new Vector3(currentPath[i].x, 0, currentPath[i].y);
                Vector3 end = new Vector3(currentPath[i + 1].x, 0, currentPath[i + 1].y);
                Gizmos.DrawLine(start, end);
            }
        }
    }

    // 狀態行為實現
    private void ExecutePatrolBehavior()
    {
        // 使用AIBehavior計時器
        if (behaviors[0].IsModOf(currentRandomMove))
        {
            DoPatrol();
        }
        
        // 處理砲塔（隨機轉向）
        if (behaviors[1].IsModOf(currentRandomMove))
        {
            HandleTurret();
        }
    }

    private void ExecuteChaseBehavior()
    {
        if (targetTank == null) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, targetTank.position);
        Debug.Log($"EnemyTankAI: Chasing target at distance {distanceToTarget:F1}");
        
        // 使用AStarPathfinder進行智能路徑尋找
        if (!hasValidPath || currentPathIndex >= currentPath.Count || 
            Time.time - lastTargetCheck > targetCheckInterval)
        {
            CalculatePathToTarget();
            lastTargetCheck = Time.time;
        }
        
        // 沿著計算出的路徑移動
        if (hasValidPath && currentPathIndex < currentPath.Count)
        {
            FollowPath();
            Debug.Log($"EnemyTankAI: Following path, waypoint {currentPathIndex}/{currentPath.Count}");
        }
        else
        {
            // 如果路徑無效，直接移動向目標（備用方案）
            MoveTowards(targetTank.position);
            Debug.Log("EnemyTankAI: No valid path, moving directly to target");
        }
        
        // 處理砲塔瞄準
        HandleTurret();
    }

    private void ExecuteAttackBehavior()
    {
        if (targetTank == null) return;
        
        // 停止移動，專注攻擊
        // 可以添加一些微調位置的行為
        
        // 處理砲塔瞄準
        HandleTurret();
        
        // 使用AIBehavior計時器控制射擊
        if (behaviors[2].IsModOf(currentRandomShoot))
        {
            TryShoot();
        }
    }

    private void ExecuteSurvivingBehavior()
    {
        // 尋找安全位置並移動
        Vector3 safePosition = FindSafePosition();
        if (safePosition != Vector3.zero)
        {
            MoveTowards(safePosition);
        }
        
        // 處理砲塔（可能瞄準威脅）
        HandleTurret();
    }

    private void ExecuteDeadBehavior()
    {
        // 死亡狀態，停止所有行為
        // 可以添加死亡動畫等
    }

    // 路徑尋找相關方法
    private void CalculatePathToTarget()
    {
        if (targetTank == null) return;
        
        Vector2Int start = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        Vector2Int end = new Vector2Int(Mathf.RoundToInt(targetTank.position.x), Mathf.RoundToInt(targetTank.position.z));
        
        Debug.Log($"EnemyTankAI: Calculating path from {start} to {end}");
        
        currentPath = AStarPathfinder.FindPath(start, end, IsPositionWalkable);
        currentPathIndex = 0;
        hasValidPath = currentPath.Count > 0;
        
        if (hasValidPath)
        {
            Debug.Log($"EnemyTankAI: Path calculated with {currentPath.Count} waypoints");
        }
        else
        {
            Debug.LogWarning("EnemyTankAI: No direct path found, trying alternative routes");
            
            // 嘗試多個中間點
            Vector2Int[] alternativeTargets = {
                new Vector2Int(end.x - 3, end.y),  // 左邊
                new Vector2Int(end.x + 3, end.y),  // 右邊
                new Vector2Int(end.x, end.y - 3),  // 下邊
                new Vector2Int(end.x, end.y + 3),  // 上邊
                new Vector2Int(end.x - 2, end.y - 2), // 左下
                new Vector2Int(end.x + 2, end.y - 2), // 右下
                new Vector2Int(end.x - 2, end.y + 2), // 左上
                new Vector2Int(end.x + 2, end.y + 2), // 右上
            };
            
            foreach (var altTarget in alternativeTargets)
            {
                if (IsPositionWalkable(altTarget))
                {
                    currentPath = AStarPathfinder.FindPath(start, altTarget, IsPositionWalkable);
                    if (currentPath.Count > 0)
                    {
                        hasValidPath = true;
                        Debug.Log($"EnemyTankAI: Found alternative path to {altTarget}");
                        break;
                    }
                }
            }
            
            // 如果還是找不到路徑，嘗試找到最近的可通行點
            if (!hasValidPath)
            {
                Vector2Int intermediateTarget = FindNearestWalkablePoint(end);
                if (intermediateTarget != end)
                {
                    Debug.Log($"EnemyTankAI: Using nearest walkable point {intermediateTarget}");
                    currentPath = AStarPathfinder.FindPath(start, intermediateTarget, IsPositionWalkable);
                    hasValidPath = currentPath.Count > 0;
                }
            }
        }
    }
    
    private Vector2Int FindNearestWalkablePoint(Vector2Int target)
    {
        // 在目標周圍尋找最近的可通行點
        int searchRadius = 5;
        Vector2Int bestPoint = target;
        float bestDistance = float.MaxValue;
        
        for (int x = -searchRadius; x <= searchRadius; x++)
        {
            for (int z = -searchRadius; z <= searchRadius; z++)
            {
                Vector2Int testPoint = target + new Vector2Int(x, z);
                if (IsPositionWalkable(testPoint))
                {
                    float distance = Vector2Int.Distance(target, testPoint);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestPoint = testPoint;
                    }
                }
            }
        }
        
        return bestPoint;
    }

    private void FollowPath()
    {
        if (currentPathIndex >= currentPath.Count) return;
        
        Vector2Int currentWaypoint = currentPath[currentPathIndex];
        Vector3 targetPosition = new Vector3(currentWaypoint.x, transform.position.y, currentWaypoint.y);
        
        float distanceToWaypoint = Vector3.Distance(transform.position, targetPosition);
        
        if (distanceToWaypoint < 1f)
        {
            currentPathIndex++;
        }
        else
        {
            MoveTowards(targetPosition);
        }
    }

    private Vector3 FindSafePosition()
    {
        // 簡單的安全位置尋找：遠離最近的危險
        if (closestDanger != null)
        {
            Vector3 dangerDirection = (transform.position - closestDanger.transform.position).normalized;
            Vector3 safePosition = transform.position + dangerDirection * 5f;
            return ClampToBoundary(safePosition);
        }
        
        return Vector3.zero;
    }

    // 尋找玩家目標
    private void FindPlayerTarget()
    {
        // 方法1：通過Player標籤尋找
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            targetTank = player;
            Debug.Log("EnemyTankAI: Found player by tag: " + player.name);
            return;
        }

        // 方法2：通過TankController組件尋找
        TankController tankController = FindFirstObjectByType<TankController>();
        if (tankController != null)
        {
            player = tankController.transform;
            targetTank = player;
            Debug.Log("EnemyTankAI: Found player by TankController: " + player.name);
            return;
        }

        // 方法3：通過名稱尋找
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj.name.ToLower().Contains("player") || 
                obj.name.ToLower().Contains("tank") && !obj.name.ToLower().Contains("enemy"))
            {
                player = obj.transform;
                targetTank = player;
                Debug.Log("EnemyTankAI: Found player by name: " + player.name);
                return;
            }
        }

        Debug.LogError("EnemyTankAI: Could not find any player target!");
    }

    // 路徑尋找輔助方法
    private bool IsPositionWalkable(Vector2Int position)
    {
        Vector3 worldPos = new Vector3(position.x, transform.position.y, position.y);
        
        // 檢查該位置是否有真正的障礙物（排除Ground）
        Collider[] colliders = Physics.OverlapSphere(worldPos, pathfindingCheckRadius * 0.5f, obstacleLayerMask);
        
        // 過濾掉Ground和其他非障礙物
        bool hasRealObstacle = false;
        string obstacleNames = "";
        foreach (var col in colliders)
        {
            if (col != null)
            {
                // 排除Ground和玩家
                if (!col.name.ToLower().Contains("ground") && 
                    !col.name.ToLower().Contains("player") &&
                    col.gameObject.layer != 0) // 排除Default層級
                {
                    hasRealObstacle = true;
                    obstacleNames += col.name + " ";
                }
            }
        }
        
        // 額外檢查：確保位置在邊界內
        bool withinBounds = Mathf.Abs(position.x) <= boundaryX && Mathf.Abs(position.y) <= boundaryZ;
        
        // 檢查是否為地面（Y軸位置合理）
        bool isOnGround = worldPos.y >= -1f && worldPos.y <= 2f;
        
        bool isWalkable = !hasRealObstacle && withinBounds && isOnGround;
        
        // 調試信息（只在有真正障礙物時輸出）
        if (!isWalkable && hasRealObstacle)
        {
            Debug.Log($"Position {position} blocked by real obstacles: {obstacleNames}");
        }
        
        return isWalkable;
    }
    
    // 檢查是否會撞到障礙物
    private bool WouldCollideWithObstacle(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        float distance = Vector3.Distance(from, to);
        
        // 使用射線檢測檢查路徑上是否有障礙物
        RaycastHit hit;
        if (Physics.Raycast(from + Vector3.up * 0.5f, direction, out hit, distance, obstacleLayerMask))
        {
            return true;
        }
        
        return false;
    }
    
    // 嘗試繞過障礙物
    private void TryGoAroundObstacle(Vector3 originalDirection)
    {
        if (targetTank == null) return;
        
        // 嘗試左右繞行
        Vector3 leftDirection = Quaternion.Euler(0, -90f, 0) * originalDirection;
        Vector3 rightDirection = Quaternion.Euler(0, 90f, 0) * originalDirection;
        
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        float checkDistance = 2f;
        
        bool leftBlocked = Physics.Raycast(rayStart, leftDirection, checkDistance, obstacleLayerMask);
        bool rightBlocked = Physics.Raycast(rayStart, rightDirection, checkDistance, obstacleLayerMask);
        
        Vector3 avoidDirection;
        if (!leftBlocked && !rightBlocked)
        {
            // 兩邊都可以走，選擇更接近目標的方向
            Vector3 leftTarget = transform.position + leftDirection * checkDistance;
            Vector3 rightTarget = transform.position + rightDirection * checkDistance;
            
            float leftDistance = Vector3.Distance(leftTarget, targetTank.position);
            float rightDistance = Vector3.Distance(rightTarget, targetTank.position);
            
            avoidDirection = leftDistance < rightDistance ? leftDirection : rightDirection;
            Debug.Log("EnemyTankAI: Both sides clear, choosing closer path");
        }
        else if (!leftBlocked)
        {
            avoidDirection = leftDirection;
            Debug.Log("EnemyTankAI: Going left around obstacle");
        }
        else if (!rightBlocked)
        {
            avoidDirection = rightDirection;
            Debug.Log("EnemyTankAI: Going right around obstacle");
        }
        else
        {
            // 兩邊都被阻擋，後退
            avoidDirection = -originalDirection;
            Debug.Log("EnemyTankAI: Both sides blocked, backing up");
        }
        
        Vector3 avoidPosition = transform.position + avoidDirection * checkDistance;
        avoidPosition = ClampToBoundary(avoidPosition);
        transform.position = avoidPosition;
    }
}
