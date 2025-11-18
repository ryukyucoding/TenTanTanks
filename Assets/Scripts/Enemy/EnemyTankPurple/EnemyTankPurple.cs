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

    [Header("Death Effects")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float explosionDuration = 2f;

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

    [Header("Wall Avoidance Settings (方案3)")]
    [SerializeField] private float wallDangerZone = 2.0f; // 牆壁危險區域半徑（在此範圍內成本增加）
    [SerializeField] private float wallCostMultiplier = 3.0f; // 靠近牆壁時的成本倍數（越大越不想靠近）

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
    private float dangerCheckInterval = 0.02f; // 危險檢測間隔（提高到每0.02秒，更快反應反彈子彈）
    private float lastDangerCheck = 0f;
    private bool enableBulletAvoidanceDebug = true; // 調試信息開關
    
    // 躲避方向記憶（避免左右搖擺）
    private Vector3 lastAvoidanceDirection = Vector3.zero;
    private GameObject lastAvoidanceBullet = null;
    private int lastAvoidanceBulletId = -1; // 使用ID追蹤子彈，避免位置變化導致誤判
    private float directionChangeCooldown = 0f; // 方向切換冷卻時間
    private float lastDirectionDecisionTime = 0f; // 上次決定方向的時間
    private float directionLockDuration = 0.5f; // 方向鎖定時間（0.5秒內不重新選擇）

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

        // 優先檢查危險（高優先級，不受頻率限制太多）
        // 在生存狀態下，每幀都檢查；其他狀態按頻率檢查
        if (isSurviving || Time.time - lastDangerCheck >= dangerCheckInterval)
        {
            CheckDangers();
        }

        // 更新AI狀態機
        UpdateAIStateMachine();

        // 根據當前狀態執行相應行為
        ExecuteCurrentStateBehavior();

        // 檢查是否卡住
        CheckStuck();

        // 調試信息
        if (Time.frameCount % 60 == 0) // 每秒輸出一次
        {
            string targetName = (targetTank != null && targetTank) ? targetTank.name : "null";
            string dangerName = (closestDanger != null && closestDanger) ? closestDanger.name : "null";
            float distance = (targetTank != null && targetTank) ? Vector3.Distance(transform.position, targetTank.position) : 0f;

            Debug.Log($"[{gameObject.name}] 📊 狀態總覽：State={currentState}, Target={targetName}, " +
                     $"SeesTarget={seesTarget}, IsSurviving={isSurviving}, " +
                     $"closestDanger={dangerName}, Distance={distance:F1}");
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
        // 檢查危險（已在Update中優先檢查，這裡不再重複調用）
        // CheckDangers(); // 移除重複調用，已在Update中優先處理
        
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

        // 計算距離和方向
        float distance = Vector3.Distance(transform.position, targetTank.position);
        Vector3 directionToTarget = (targetTank.position - transform.position).normalized;
        Vector3 rayStart = transform.position + Vector3.up * visionHeight;

        // 檢查是否能看到目標（用於射擊判斷）
        seesTarget = false;
        if (distance <= detectionRange)
        {
        // 使用更精確的射線檢測
        RaycastHit hit;
        if (Physics.Raycast(rayStart, directionToTarget, out hit, distance, obstacleLayerMask))
        {
            // 檢查擊中的是否為目標本身
                if (hit.collider.transform == targetTank)
            {
                    seesTarget = true;
            }
        }
            else
            {
                // 沒有擊中障礙物，視為可以看到目標
        seesTarget = true;
            }
        }

        // 無論是否看到目標，都計算瞄準角度（讓炮管一直指向玩家）
        aimTarget = targetTank.position;

        // 預測目標位置（只有在看到目標時才預測）
        if (seesTarget && aiParameters.predictsPositions)
        {
            Rigidbody targetRb = targetTank.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                float timeToTarget = distance / bulletSpeed;
                aimTarget = targetTank.position + targetRb.linearVelocity * timeToTarget;
            }
        }

        // 計算瞄準角度（無論是否看到目標都計算，讓炮管一直指向玩家）
        Vector3 aimDirection = (aimTarget - turret.position).normalized;
        aimDirection.y = 0;
        
        if (aimDirection.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(aimDirection.x, aimDirection.z) * Mathf.Rad2Deg;
            // 只有在看到目標時才添加隨機偏移（射擊準度），否則直接瞄準
            if (seesTarget)
            {
            targetTurretRotation = targetAngle + Random.Range(-aiParameters.aimOffset, aiParameters.aimOffset);
            }
            else
            {
                targetTurretRotation = targetAngle;
            }
        }
        
        // 調試射線（只在檢測範圍內顯示）
        if (distance <= detectionRange * 2f) // 擴大顯示範圍以便調試
        {
            Debug.DrawRay(rayStart, directionToTarget * distance, seesTarget ? Color.green : Color.yellow, 0.1f);
        }
    }

    private void DoMovement()
    {
        if (currentHealth <= 0) return;

        // 檢查危險（已在Update中優先檢查，這裡不再重複）
        // CheckDangers(); // 移除重複調用
        
        // 如果有危險，優先躲避（在生存狀態下會自動處理）
        if (isSurviving)
        {
            // 躲避邏輯在 ExecuteSurvivingBehavior() 中處理
            // 這裡只處理障礙物導航（但在躲避時通常跳過）
            return;
        }

        // 如果有目標，追擊目標
        if (targetTank != null)
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
        else
        {
            // 沒有目標時進行巡邏
            DoPatrol();
        }

        // 處理障礙物導航
        DoBlockNavigation();
    }

    private void CheckDangers()
    {
        // 在生存狀態下，每幀都檢查（不限制頻率）
        // 其他狀態下限制檢測頻率以提高性能
        if (!isSurviving && Time.time - lastDangerCheck < dangerCheckInterval) return;
        lastDangerCheck = Time.time;
        
        nearbyDangers.Clear();
        closestDanger = null;
        
        // 檢測附近的子彈（擴大檢測範圍，讓敵人有足夠時間反應）
        // 使用更大的檢測範圍（1.5倍），讓敵人能更早發現子彈
        float extendedDetectionRange = aiParameters.awarenessHostileShell * 1.5f;
        Collider[] bulletColliders = Physics.OverlapSphere(transform.position, extendedDetectionRange, bulletLayer);
        
        if (enableBulletAvoidanceDebug)
        {
            if (bulletColliders.Length > 0)
            {
                Debug.Log($"[{gameObject.name}] 🔍 檢測到 {bulletColliders.Length} 個子彈在附近（範圍: {extendedDetectionRange:F2}, 當前狀態: {currentState}, isSurviving: {isSurviving})");
            }
            else if (isSurviving)
            {
                Debug.Log($"[{gameObject.name}] 🔍 生存模式中，但未檢測到子彈（範圍: {extendedDetectionRange:F2})");
            }
        }
        
        float closestThreatDistance = float.MaxValue;
        GameObject mostThreateningBullet = null;
        
        foreach (var collider in bulletColliders)
        {
            if (collider == null || collider.gameObject == null) continue;
            
            GameObject bullet = collider.gameObject;
            
            // 檢查子彈是否是自己發射的（不躲避自己的子彈）
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                GameObject bulletShooter = bulletScript.GetShooter();
                // 如果是自己發射的子彈，跳過
                if (bulletShooter == gameObject || 
                    (bulletShooter != null && bulletShooter.transform.IsChildOf(transform)))
                {
                    if (enableBulletAvoidanceDebug)
                        Debug.Log($"[{gameObject.name}] 跳過自己的子彈");
                    continue;
                }
            }
            
            // 預測子彈路徑，判斷是否會擊中自己（使用更寬鬆的判斷）
            if (WillBulletHitMe(bullet))
            {
                nearbyDangers.Add(bullet);
                
                // 計算威脅等級（距離越近、速度越快、角度越正對威脅越大）
                float threatLevel = CalculateThreatLevel(bullet);
                if (threatLevel < closestThreatDistance)
                {
                    closestThreatDistance = threatLevel;
                    mostThreateningBullet = bullet;
                }
                
                if (enableBulletAvoidanceDebug)
                {
                    float distance = Vector3.Distance(transform.position, bullet.transform.position);
                    Debug.Log($"[{gameObject.name}] ⚠️ 檢測到危險子彈！距離: {distance:F2}, 威脅等級: {threatLevel:F2}");
                }
            }
        }

        // 如果檢測到會擊中自己的子彈，進入躲避模式
        if (mostThreateningBullet != null)
        {
            // 使用子彈的實例ID來判斷是否為新子彈（避免位置變化導致誤判）
            int currentBulletId = mostThreateningBullet.GetInstanceID();
            bool isNewBullet = (lastAvoidanceBulletId != currentBulletId) || (closestDanger != mostThreateningBullet && closestDanger != null);
            
            // 只有真正是新子彈時才重置方向記憶
            if (isNewBullet)
            {
                lastAvoidanceBullet = null;
                lastAvoidanceBulletId = -1;
                lastAvoidanceDirection = Vector3.zero;
                directionChangeCooldown = 0f;
                lastDirectionDecisionTime = 0f;
                
                if (enableBulletAvoidanceDebug)
                {
                    Debug.Log($"[{gameObject.name}] 🔄 檢測到新子彈（ID: {currentBulletId}），重置方向記憶");
                }
            }
            else if (enableBulletAvoidanceDebug && Time.frameCount % 30 == 0)
            {
                Debug.Log($"[{gameObject.name}] 🔄 同一顆子彈（ID: {currentBulletId}），保持方向記憶");
            }
            
            closestDanger = mostThreateningBullet;
            lastAvoidanceBulletId = currentBulletId;
            bool wasSurviving = isSurviving;
            isSurviving = true;
            
            if (enableBulletAvoidanceDebug)
            {
                float distance = Vector3.Distance(transform.position, mostThreateningBullet.transform.position);
                Rigidbody bulletRb = mostThreateningBullet.GetComponent<Rigidbody>();
                float bulletSpeed = bulletRb != null ? bulletRb.linearVelocity.magnitude : 0f;
                string statusChange = wasSurviving ? "持續躲避" : "🚨 進入躲避模式";
                Debug.Log($"[{gameObject.name}] {statusChange}！子彈ID: {currentBulletId}, 距離: {distance:F2}, 速度: {bulletSpeed:F2}, 威脅等級: {closestThreatDistance:F2}");
            }
        }
        else
        {
            // 沒有危險，退出生存模式，清除方向記憶
            if (isSurviving)
            {
                lastAvoidanceBullet = null;
                lastAvoidanceBulletId = -1;
                lastAvoidanceDirection = Vector3.zero;
                directionChangeCooldown = 0f;
                lastDirectionDecisionTime = 0f;
                
                if (enableBulletAvoidanceDebug)
                {
                    Debug.Log($"[{gameObject.name}] ✅ 退出躲避模式（未檢測到危險子彈）");
                }
            }
            isSurviving = false;
            closestDanger = null;
        }
    }
    
    // 判斷子彈是否會擊中自己（改進版本，更敏感）
    private bool WillBulletHitMe(GameObject bullet)
    {
        if (bullet == null) return false;
        
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb == null || bulletRb.linearVelocity.magnitude < 0.1f) return false;
        
        Vector3 bulletPosition = bullet.transform.position;
        Vector3 bulletVelocity = bulletRb.linearVelocity;
        Vector3 myPosition = transform.position;
        
        // 計算距離和方向
        Vector3 toMe = (myPosition - bulletPosition);
        float distance = toMe.magnitude;
        Vector3 toMeNormalized = toMe.normalized;
        Vector3 bulletDir = bulletVelocity.normalized;
        
        // 計算子彈朝向我們的角度（0-1，1表示完全朝向我們）
        float alignment = Vector3.Dot(bulletDir, toMeNormalized);
        
        // 如果子彈背向我們移動（alignment < 0），不會擊中
        if (alignment < -0.1f)
            return false;
        
        // 計算子彈最接近我們時的距離
        float closestApproachDistance = CalculateClosestApproachDistance(
            bulletPosition, bulletVelocity, myPosition, moveSpeed);
        
        // 擴大安全距離，讓躲避更靈敏，並且在更遠的距離就開始躲避
        float tankRadius = 1.5f; // 坦克的半徑
        float safetyDistance = tankRadius + 2.5f; // 增加緩衝距離（從1.5增加到2.5）
        
        // 預測時間：計算子彈到達我們需要的時間
        float timeToImpact = distance / bulletVelocity.magnitude;
        
        // 如果子彈在較遠距離但朝向我們且在預測時間內會擊中，也視為危險
        // 在10單位內且對齊度>0.5，或15單位內且對齊度>0.7，都視為危險
        bool isThreat = (distance < 10f && alignment > 0.5f) || 
                        (distance < 15f && alignment > 0.7f) ||
                        (timeToImpact < 0.5f && alignment > 0.3f); // 0.5秒內會到達且朝向我們
        
        // 如果最近距離小於安全距離，或在威脅範圍內，視為會擊中
        bool willHit = closestApproachDistance < safetyDistance || isThreat;
        
        if (enableBulletAvoidanceDebug && willHit)
        {
            Debug.Log($"[{gameObject.name}] 子彈威脅判斷: 距離={distance:F2}, 對齊度={alignment:F2}, 最近距離={closestApproachDistance:F2}, 安全距離={safetyDistance:F2}");
        }
        
        return willHit;
    }
    
    // 計算子彈與坦克的最近距離
    private float CalculateClosestApproachDistance(Vector3 bulletPos, Vector3 bulletVel, 
        Vector3 tankPos, float tankMaxSpeed)
    {
        // 簡化計算：假設坦克保持當前速度移動
        Vector3 relativePos = tankPos - bulletPos;
        Vector3 relativeVel = -bulletVel; // 相對於子彈的速度
        
        // 如果相對速度為0或很小，直接返回當前距離
        if (relativeVel.magnitude < 0.1f)
            return relativePos.magnitude;
        
        // 計算最小距離的時間
        float t = Vector3.Dot(relativePos, relativeVel) / (relativeVel.magnitude * relativeVel.magnitude);
        
        // 如果t為負數，說明子彈已經錯過，返回當前距離
        if (t < 0)
            return relativePos.magnitude;
        
        // 計算該時間點的距離
        Vector3 closestPos = bulletPos + bulletVel * t;
        Vector3 tankFuturePos = tankPos; // 簡化：假設坦克不動（或移動很小）
        return Vector3.Distance(closestPos, tankFuturePos);
    }
    
    // 計算威脅等級（值越小威脅越大）
    private float CalculateThreatLevel(GameObject bullet)
    {
        if (bullet == null) return float.MaxValue;
        
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb == null) return float.MaxValue;
        
        float distance = Vector3.Distance(transform.position, bullet.transform.position);
        Vector3 bulletDirection = bulletRb.linearVelocity.normalized;
        Vector3 toMe = (transform.position - bullet.transform.position).normalized;
        
        // 角度越正對（cos越大），威脅越大
        float alignment = Vector3.Dot(bulletDirection, toMe);
        
        // 威脅等級 = 距離 / (對齊度 + 0.1)，距離越近、對齊度越高，威脅越大
        return distance / (alignment + 0.1f);
    }

    // 計算指定方向的可用空間大小
    private float CalculateAvailableSpace(Vector3 start, Vector3 direction, float maxDistance)
    {
        // 使用多個射線檢測，計算可用空間
        float space = 0f;
        int rayCount = 5; // 發射5條射線檢查
        
        for (int i = 0; i < rayCount; i++)
        {
            // 在不同高度發射射線（避免只檢查一個點）
            Vector3 rayStart = start + Vector3.up * (i * 0.2f - 0.4f);
            RaycastHit hit;
            
            if (Physics.Raycast(rayStart, direction, out hit, maxDistance, obstacleLayerMask))
            {
                space += hit.distance;
            }
            else
            {
                space += maxDistance; // 沒有擊中障礙物，空間最大
            }
        }
        
        return space / rayCount; // 返回平均空間
    }

    private void AvoidDanger()
    {
        if (closestDanger == null) 
        {
            if (enableBulletAvoidanceDebug)
            {
                Debug.Log($"[{gameObject.name}] ⚠️ AvoidDanger被調用但closestDanger為null");
            }
            isSurviving = false;
            lastAvoidanceBullet = null;
            lastAvoidanceBulletId = -1;
            lastAvoidanceDirection = Vector3.zero;
            return;
        }

        Rigidbody bulletRb = closestDanger.GetComponent<Rigidbody>();
        if (bulletRb == null || bulletRb.linearVelocity.magnitude < 0.1f)
        {
            if (enableBulletAvoidanceDebug)
            {
                Debug.Log($"[{gameObject.name}] ⚠️ 子彈Rigidbody無效或速度為0，退出躲避");
            }
            isSurviving = false;
            return;
        }
        
        // 每幀都執行躲避，確保快速反應
        Vector3 bulletPosition = closestDanger.transform.position;
        Vector3 bulletVelocity = bulletRb.linearVelocity;
        Vector3 myPosition = transform.position;
        
        // 計算子彈相對於我們的位置和方向
        Vector3 toBullet = (bulletPosition - myPosition);
        float distanceToBullet = toBullet.magnitude;
        Vector3 bulletDirection = bulletVelocity.normalized;
        
        // 每幀都輸出（但限制頻率避免刷屏）
        if (enableBulletAvoidanceDebug && Time.frameCount % 3 == 0)
        {
            Debug.Log($"[{gameObject.name}] 🏃 正在躲避！子彈距離: {distanceToBullet:F2}, 子彈速度: {bulletVelocity.magnitude:F2}, " +
                     $"子彈位置: {bulletPosition}, 我的位置: {myPosition}");
        }
        
        // 優先左右躲避，而不是遠離（因為遠離可能來不及）
        Vector3 rightDirection = Vector3.Cross(Vector3.up, bulletDirection).normalized;
        Vector3 leftDirection = -rightDirection;
        
        Vector3 checkStart = transform.position + Vector3.up * 0.5f;
        float checkDistance = 5f; // 檢查距離（用於判斷空間大小）
        
        // 計算左右兩側的可用空間大小
        float rightSpace = CalculateAvailableSpace(checkStart, rightDirection, checkDistance);
        float leftSpace = CalculateAvailableSpace(checkStart, leftDirection, checkDistance);
        
        // 檢查是否被完全阻擋
        bool rightBlocked = rightSpace < 1f; // 空間小於1單位視為被阻擋
        bool leftBlocked = leftSpace < 1f;
        
        // 根據空間大小選擇方向（選擇空間更大的方向）
        Vector3 chosenDirection;
        
        // 如果左右都被阻擋，向後移動
        if (rightBlocked && leftBlocked)
        {
            chosenDirection = -bulletDirection;
            if (enableBulletAvoidanceDebug)
                Debug.Log($"[{gameObject.name}] 左右都被阻擋，向後移動");
        }
        // 如果只有一側被阻擋，選擇另一側
        else if (rightBlocked && !leftBlocked)
        {
            chosenDirection = leftDirection;
            if (enableBulletAvoidanceDebug)
                Debug.Log($"[{gameObject.name}] 右側被阻擋（空間={rightSpace:F2}），選擇左側（空間={leftSpace:F2}）");
        }
        else if (leftBlocked && !rightBlocked)
        {
            chosenDirection = rightDirection;
            if (enableBulletAvoidanceDebug)
                Debug.Log($"[{gameObject.name}] 左側被阻擋（空間={leftSpace:F2}），選擇右側（空間={rightSpace:F2}）");
        }
        // 兩側都有空間，選擇空間更大的方向
        else
        {
            // 檢查是否需要切換方向（避免頻繁搖擺）
            bool shouldSwitchDirection = false;
            Vector3 preferredDirection = rightSpace > leftSpace ? rightDirection : leftDirection;
            
            // 檢查是否是同一顆子彈（使用ID判斷，更可靠）
            int currentBulletId = closestDanger.GetInstanceID();
            bool isSameBullet = (lastAvoidanceBulletId == currentBulletId) && 
                               (lastAvoidanceBullet == closestDanger || lastAvoidanceBullet == null);
            
            // 檢查方向鎖定時間（避免頻繁重新選擇）
            float timeSinceLastDecision = Time.time - lastDirectionDecisionTime;
            bool directionLocked = timeSinceLastDecision < directionLockDuration;
            
            // 如果這是同一顆子彈，且有記憶的方向，且方向鎖定時間未過
            if (isSameBullet && lastAvoidanceDirection != Vector3.zero && directionLocked)
            {
                // 方向鎖定期間，強制保持當前方向（除非被阻擋）
                bool currentDirectionBlocked = (Vector3.Dot(lastAvoidanceDirection, rightDirection) > 0.5f && rightBlocked) ||
                                              (Vector3.Dot(lastAvoidanceDirection, leftDirection) > 0.5f && leftBlocked);
                
                if (!currentDirectionBlocked)
                {
                    // 保持當前方向，不重新選擇
                    shouldSwitchDirection = false;
                    
                    if (enableBulletAvoidanceDebug && Time.frameCount % 30 == 0)
                    {
                        Debug.Log($"[{gameObject.name}] 🔒 方向鎖定中（剩餘: {directionLockDuration - timeSinceLastDecision:F2}秒），保持當前方向");
                    }
                }
                else
                {
                    // 當前方向被阻擋，必須切換
                    shouldSwitchDirection = true;
                    if (enableBulletAvoidanceDebug)
                    {
                        Debug.Log($"[{gameObject.name}] ⚠️ 當前方向被阻擋，強制切換（方向鎖定被打破）");
                    }
                }
            }
            else if (isSameBullet && lastAvoidanceDirection != Vector3.zero && !directionLocked)
            {
                // 同一顆子彈但鎖定時間過了，檢查是否需要切換
                float spaceDifference = Mathf.Abs(rightSpace - leftSpace);
                
                // 如果空間差異明顯（>3單位），且當前的方向空間明顯更小，才切換
                if (spaceDifference > 3f)
                {
                    float currentSpace = Vector3.Dot(lastAvoidanceDirection, rightDirection) > 0.5f ? rightSpace : 
                                        Vector3.Dot(lastAvoidanceDirection, leftDirection) > 0.5f ? leftSpace : 0f;
                    float preferredSpace = rightSpace > leftSpace ? rightSpace : leftSpace;
                    
                    // 如果偏好方向的空間比當前方向大3單位以上，才切換
                    if (preferredSpace > currentSpace + 3f && directionChangeCooldown <= 0f)
                    {
                        shouldSwitchDirection = true;
                        if (enableBulletAvoidanceDebug)
                        {
                            Debug.Log($"[{gameObject.name}] 空間差異明顯（{spaceDifference:F2}），切換方向");
                        }
                    }
                    else
                    {
                        shouldSwitchDirection = false;
                    }
                }
                else
                {
                    // 空間差異不大，保持當前方向
                    shouldSwitchDirection = false;
                }
                
                // 如果當前方向被阻擋，必須切換
                if ((Vector3.Dot(lastAvoidanceDirection, rightDirection) > 0.5f && rightBlocked) ||
                    (Vector3.Dot(lastAvoidanceDirection, leftDirection) > 0.5f && leftBlocked))
                {
                    shouldSwitchDirection = true;
                }
            }
            else
            {
                // 新子彈或沒有記憶，直接選擇空間更大的方向
                shouldSwitchDirection = true;
            }
            
            if (shouldSwitchDirection)
            {
                chosenDirection = preferredDirection;
                lastAvoidanceDirection = chosenDirection;
                lastAvoidanceBullet = closestDanger;
                lastAvoidanceBulletId = closestDanger.GetInstanceID();
                directionChangeCooldown = 0.3f; // 設置切換冷卻時間（0.3秒內不再切換）
                lastDirectionDecisionTime = Time.time; // 記錄決定方向的時間
                
                if (enableBulletAvoidanceDebug)
                {
                    Debug.Log($"[{gameObject.name}] 🎯 選擇空間更大的方向：{(rightSpace > leftSpace ? "右側" : "左側")} " +
                             $"(右={rightSpace:F2}, 左={leftSpace:F2}), 子彈ID: {lastAvoidanceBulletId}, " +
                             $"方向鎖定: {directionLockDuration}秒");
                }
            }
            else
            {
                // 保持當前方向（但確保方向有效）
                if (lastAvoidanceDirection != Vector3.zero && lastAvoidanceDirection.magnitude > 0.1f)
                {
                    chosenDirection = lastAvoidanceDirection;
                    if (enableBulletAvoidanceDebug && Time.frameCount % 30 == 0)
                    {
                        Debug.Log($"[{gameObject.name}] 保持當前躲避方向（避免搖擺）");
                    }
                }
                else
                {
                    // 如果記憶的方向無效，選擇空間更大的方向
                    chosenDirection = preferredDirection;
                    lastAvoidanceDirection = chosenDirection;
                    lastAvoidanceBullet = closestDanger;
                    lastAvoidanceBulletId = closestDanger.GetInstanceID();
                    directionChangeCooldown = 0.3f;
                    lastDirectionDecisionTime = Time.time;
                    
                    if (enableBulletAvoidanceDebug)
                    {
                        Debug.Log($"[{gameObject.name}] 記憶方向無效，重新選擇：{(rightSpace > leftSpace ? "右側" : "左側")}, 子彈ID: {lastAvoidanceBulletId}");
                    }
                }
            }
        }
        
        // 確保chosenDirection有效（防止為零向量）
        if (chosenDirection == Vector3.zero || chosenDirection.magnitude < 0.1f)
        {
            // 如果方向無效，強制選擇一個方向（優先選擇空間更大的）
            if (rightSpace > leftSpace)
            {
                chosenDirection = rightDirection;
            }
            else
            {
                chosenDirection = leftDirection;
            }
            
            if (enableBulletAvoidanceDebug)
            {
                Debug.Log($"[{gameObject.name}] ⚠️ 方向無效，強制選擇：{(rightSpace > leftSpace ? "右側" : "左側")}, 子彈ID: {closestDanger.GetInstanceID()}");
            }
            
            // 更新記憶
            lastAvoidanceBullet = closestDanger;
            lastAvoidanceBulletId = closestDanger.GetInstanceID();
            lastDirectionDecisionTime = Time.time;
        }
        
        // 更新冷卻時間
        if (directionChangeCooldown > 0f)
        {
            directionChangeCooldown -= Time.deltaTime;
        }
        
        // 使用正常速度或稍快的速度移動（不要暴沖）
        // 根據距離子彈的遠近調整速度：距離越近，速度越快（但最多1.5倍）
        float distanceFactor = Mathf.Clamp01(distanceToBullet / 8f); // 8單位內開始加速
        float avoidanceSpeed = moveSpeed * (1f + (1f - distanceFactor) * 0.5f); // 1倍到1.5倍之間
        
        Vector3 moveDirection = chosenDirection;
        moveDirection.y = 0;
        moveDirection = moveDirection.normalized;
        
        // 確保方向是左右，而不是前後（避免往前暴沖）
        // 檢查moveDirection是否與bulletDirection太平行
        float alignmentWithBullet = Mathf.Abs(Vector3.Dot(moveDirection, bulletDirection));
        if (alignmentWithBullet > 0.5f) // 如果移動方向與子彈方向太平行
        {
            // 強制使用左右方向（更垂直於子彈方向）
            if (alignmentWithBullet > 0.7f) // 如果太平行，重新選擇
            {
                // 選擇點積更小的方向（更垂直）
                float rightAlignment = Mathf.Abs(Vector3.Dot(rightDirection, bulletDirection));
                float leftAlignment = Mathf.Abs(Vector3.Dot(leftDirection, bulletDirection));
                moveDirection = rightAlignment < leftAlignment ? rightDirection : leftDirection;
                moveDirection.y = 0;
                moveDirection = moveDirection.normalized;
                
                if (enableBulletAvoidanceDebug)
                    Debug.Log($"[{gameObject.name}] 方向調整：避免向前移動，改用更垂直的方向");
            }
        }
        
        // 確保moveDirection有效
        if (moveDirection.magnitude < 0.1f)
        {
            // 如果moveDirection無效，使用chosenDirection
            moveDirection = chosenDirection;
            moveDirection.y = 0;
            moveDirection = moveDirection.normalized;
            
            if (enableBulletAvoidanceDebug)
            {
                Debug.Log($"[{gameObject.name}] ⚠️ moveDirection無效，使用chosenDirection");
            }
        }
        
        if (moveDirection.magnitude > 0.1f)
        {
            // 計算新位置（使用正常速度）
            Vector3 newPosition = transform.position + moveDirection * avoidanceSpeed * Time.deltaTime;
            
            // 檢查是否會撞到障礙物（躲避時使用更寬鬆的檢測）
            bool canMove = !WouldCollideWithObstacle(transform.position, newPosition);
            
            // 如果直接移動被阻擋，嘗試稍微偏移（沿牆壁移動）
            if (!canMove)
            {
                // 嘗試沿牆壁移動（與牆壁平行）
                Vector3 wallParallel = Vector3.Cross(moveDirection, Vector3.up).normalized;
                Vector3 offsetPosition1 = transform.position + (moveDirection + wallParallel * 0.3f) * avoidanceSpeed * Time.deltaTime;
                Vector3 offsetPosition2 = transform.position + (moveDirection - wallParallel * 0.3f) * avoidanceSpeed * Time.deltaTime;
                
                if (!WouldCollideWithObstacle(transform.position, offsetPosition1))
                {
                    newPosition = offsetPosition1;
                    canMove = true;
                }
                else if (!WouldCollideWithObstacle(transform.position, offsetPosition2))
                {
                    newPosition = offsetPosition2;
                    canMove = true;
                }
            }
            
            if (canMove)
            {
                Vector3 oldPosition = transform.position;
                newPosition = ClampToBoundary(newPosition);
                transform.position = newPosition;
                
                float actualMoveDistance = Vector3.Distance(oldPosition, newPosition);
                
                if (enableBulletAvoidanceDebug && Time.frameCount % 3 == 0)
                {
                    Debug.Log($"[{gameObject.name}] ✅ 躲避移動成功！方向={moveDirection}, 速度={avoidanceSpeed:F2}, " +
                             $"移動距離={actualMoveDistance:F3}, 子彈距離={distanceToBullet:F2}, " +
                             $"選擇方向={(Vector3.Dot(moveDirection, rightDirection) > 0.5f ? "右" : "左")}");
                }
            }
            else
            {
                if (enableBulletAvoidanceDebug && Time.frameCount % 5 == 0)
                {
                    Debug.Log($"[{gameObject.name}] ❌ 躲避移動被阻擋！嘗試的方向={moveDirection}, 子彈距離={distanceToBullet:F2}");
                }
                
                // 如果被阻擋，嘗試另一個方向
                Vector3 altDirection = (Vector3.Dot(chosenDirection, rightDirection) > 0.5f) ? leftDirection : rightDirection;
                altDirection.y = 0;
                altDirection = altDirection.normalized;
                Vector3 altPosition = transform.position + altDirection * avoidanceSpeed * Time.deltaTime;
                
                if (!WouldCollideWithObstacle(transform.position, altPosition))
                {
                    altPosition = ClampToBoundary(altPosition);
                    transform.position = altPosition;
                    
                    // 更新記憶方向
                    lastAvoidanceDirection = altDirection;
                    
                    if (enableBulletAvoidanceDebug)
                    {
                        Debug.Log($"[{gameObject.name}] 原方向被阻擋，改用另一側：{altDirection}");
                    }
                }
                else
                {
                    // 兩個方向都被阻擋，嘗試向後移動
                    Vector3 backwardDirection = -bulletDirection;
                    backwardDirection.y = 0;
                    backwardDirection = backwardDirection.normalized;
                    Vector3 backwardPosition = transform.position + backwardDirection * avoidanceSpeed * Time.deltaTime;
                    
                    if (!WouldCollideWithObstacle(transform.position, backwardPosition))
                    {
                        backwardPosition = ClampToBoundary(backwardPosition);
                        transform.position = backwardPosition;
                        
                        if (enableBulletAvoidanceDebug)
                        {
                            Debug.Log($"[{gameObject.name}] 左右都被阻擋，向後移動");
                        }
                    }
                    else
                    {
                        if (enableBulletAvoidanceDebug && Time.frameCount % 10 == 0)
                        {
                            Debug.Log($"[{gameObject.name}] ⚠️ 所有方向都被阻擋，無法移動");
                        }
                    }
                }
            }
            
            // 旋轉車身朝向躲避方向（正常旋轉速度，不要太快）
            if (tankBody != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                float currentYRotation = tankBody.eulerAngles.y;
                float targetYRotation = targetRotation.eulerAngles.y;
                
                // 躲避時稍微快一點旋轉（1.5倍速度），但不要太快
                float rotationStep = rotationSpeed * 1.5f * Time.deltaTime;
                float newYRotation = Mathf.MoveTowardsAngle(currentYRotation, targetYRotation, rotationStep);
                tankBody.rotation = Quaternion.Euler(0, newYRotation, 0);
            }
        }
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
            
            // 檢查是否會撞到障礙物（使用更寬鬆的檢測）
            if (!WouldCollideWithObstacle(transform.position, newPosition))
            {
                // 檢查邊界限制
                newPosition = ClampToBoundary(newPosition);
                
                // 移動（使用Transform而不是Rigidbody以獲得更平滑的移動）
                transform.position = newPosition;
            }
            else
            {
                // 如果直接移動被阻擋，嘗試沿著牆壁移動（在轉彎時特別有用）
                // 先嘗試稍微向左偏移
                Vector3 leftOffset = Quaternion.Euler(0, -30f, 0) * direction;
                Vector3 leftPosition = transform.position + leftOffset * moveSpeed * Time.deltaTime;
                
                if (!WouldCollideWithObstacle(transform.position, leftPosition))
                {
                    leftPosition = ClampToBoundary(leftPosition);
                    transform.position = leftPosition;
                    return;
                }
                
                // 再嘗試稍微向右偏移
                Vector3 rightOffset = Quaternion.Euler(0, 30f, 0) * direction;
                Vector3 rightPosition = transform.position + rightOffset * moveSpeed * Time.deltaTime;
                
                if (!WouldCollideWithObstacle(transform.position, rightPosition))
                {
                    rightPosition = ClampToBoundary(rightPosition);
                    transform.position = rightPosition;
                    return;
                }
                
                // 如果左右偏移都無法移動，嘗試繞行
                if (enableBulletAvoidanceDebug)
                {
                    Debug.Log($"[{gameObject.name}] 移動被阻擋，嘗試繞行");
                }
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

        Debug.Log("Enemy tank destroyed!");

        // Play explosion sound
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // Create explosion visual effect
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);

            // Auto-destroy the explosion effect after duration
            if (explosionDuration > 0)
            {
                Destroy(explosion, explosionDuration);
            }
        }

        // Notify game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDestroyed();
        }

        // Destroy the enemy tank immediately
        Destroy(gameObject);
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
        
        // 處理砲塔（如果有目標就指向目標，否則可以隨機轉向）
            HandleTurret();
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
        if (enableBulletAvoidanceDebug && Time.frameCount % 10 == 0)
        {
            Debug.Log($"[{gameObject.name}] 📊 生存模式執行中：closestDanger={(closestDanger != null ? closestDanger.name : "null")}, " +
                     $"isSurviving={isSurviving}, currentState={currentState}, " +
                     $"lastDirection={lastAvoidanceDirection}, directionCooldown={directionChangeCooldown:F2}");
        }
        
        // 優先躲避子彈（必須執行，確保移動）
        if (closestDanger != null)
        {
            AvoidDanger();
        }
        else
        {
            // 沒有直接危險時，尋找安全位置並移動
            if (enableBulletAvoidanceDebug && Time.frameCount % 10 == 0)
            {
                Debug.Log($"[{gameObject.name}] ⚠️ 生存模式但closestDanger為null，尋找安全位置");
            }
            
        Vector3 safePosition = FindSafePosition();
        if (safePosition != Vector3.zero)
        {
            MoveTowards(safePosition);
            }
            else if (enableBulletAvoidanceDebug && Time.frameCount % 10 == 0)
            {
                Debug.Log($"[{gameObject.name}] ⚠️ 生存模式但沒有危險目標，也沒有安全位置");
            }
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

        // 方案3：使用成本函數版本的 FindPath（讓路徑遠離牆壁）
        currentPath = AStarPathfinder.FindPath(start, end, IsPositionWalkable, GetPositionCost);
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
                    currentPath = AStarPathfinder.FindPath(start, altTarget, IsPositionWalkable, GetPositionCost);
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
                    currentPath = AStarPathfinder.FindPath(start, intermediateTarget, IsPositionWalkable, GetPositionCost);
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

    // 方案3：計算位置成本（靠近牆壁成本更高）
    private float GetPositionCost(Vector2Int position)
    {
        Vector3 worldPos = new Vector3(position.x, transform.position.y, position.y);

        // 基礎成本：靠近牆壁的成本
        Collider[] nearbyObstacles = Physics.OverlapSphere(worldPos, wallDangerZone, obstacleLayerMask);

        float minDistanceToWall = float.MaxValue;
        foreach (var col in nearbyObstacles)
        {
            if (col != null)
            {
                // 排除Ground和玩家
                if (!col.name.ToLower().Contains("ground") &&
                    !col.name.ToLower().Contains("player") &&
                    col.gameObject.layer != 0)
                {
                    Vector3 closestPoint = col.ClosestPoint(worldPos);
                    float distance = Vector3.Distance(worldPos, closestPoint);
                    minDistanceToWall = Mathf.Min(minDistanceToWall, distance);
                }
            }
        }

        // 基礎成本：距離牆壁越近，成本越高
        float cost = 1f;
        if (minDistanceToWall < wallDangerZone)
        {
            float normalizedDistance = minDistanceToWall / wallDangerZone;
            cost = 1f + (1f - normalizedDistance) * (wallCostMultiplier - 1f);
        }

        return cost;
    }

    // 檢查是否會撞到障礙物
    private bool WouldCollideWithObstacle(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        float distance = Vector3.Distance(from, to);
        
        // 如果距離太短（小於0.1），不檢測（避免自己檢測到自己）
        if (distance < 0.1f)
            return false;
        
        // 使用SphereCast而不是Raycast，考慮坦克的半徑
        float tankRadius = 0.8f; // 坦克的半徑（稍微小一點，避免過於敏感）
        Vector3 rayStart = from + Vector3.up * 0.5f;
        
        RaycastHit hit;
        // 使用SphereCast檢查路徑上是否有障礙物
        if (Physics.SphereCast(rayStart, tankRadius, direction, out hit, distance, obstacleLayerMask))
        {
            // 忽略自己
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                return false;
            }
            
            // 如果碰撞點距離起點很近（小於坦克半徑），可能是誤判，允許通過
            if (hit.distance < tankRadius * 0.5f)
            {
                return false;
            }
            
            return true;
        }
        
        // 額外檢查：在目標位置周圍是否有障礙物（防止進入牆角）
        Collider[] colliders = Physics.OverlapSphere(to, tankRadius * 0.8f, obstacleLayerMask);
        foreach (var col in colliders)
        {
            if (col != null && col.gameObject != gameObject && !col.transform.IsChildOf(transform))
            {
                // 檢查是否是真正的障礙物（不是地面等）
                if (col.gameObject.layer != 0) // 排除Default層級
                {
                    return true;
                }
            }
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
