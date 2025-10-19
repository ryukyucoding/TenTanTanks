using UnityEngine;
using UnityEngine.AI;

public class EnemyTank : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private EnemyAIParameters aiParameters;
    [SerializeField] private bool useAIParameters = true;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 150f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float shootingRange = 8f;
    [SerializeField] private float minDistanceToPlayer = 3f;

    [Header("Tank Parts")]
    [SerializeField] private Transform tankBody;
    [SerializeField] private Transform turret;
    [SerializeField] private Transform firePoint;

    [Header("Combat Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float maxHealth = 1f;

    [Header("AI Behavior")]
    [SerializeField] private float patrolRadius = 5f;
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private LayerMask obstacleLayer = 1;

    [Header("Navigation")]
    [SerializeField] private bool useNavMesh = true;
    [SerializeField] private float navRepathInterval = 0.2f; // seconds
    [SerializeField] private float navStoppingDistanceBuffer = 0.2f; // extra distance to stop before minDistance
    private NavMeshAgent agent;
    private float navRepathTimer;

    // AIA
    private enum AIState
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }

    private AIState currentState = AIState.Patrol;
    private Transform player;
    private Rigidbody rb;

    // �԰�����
    private float currentHealth;
    private float nextFireTime;

    // ���ެ���
    private Vector3 patrolCenter;
    private Vector3 currentPatrolTarget;
    private float patrolWaitTimer;
    private bool isWaiting = false;

    // ���ʬ���
    private Vector3 lastValidPosition;
    private float stuckTimer = 0f;
    private float stuckCheckInterval = 2f;

    // Components for ported AI system
    [SerializeField] private AIDangerScanner dangerScanner;
    [SerializeField] private AIMovementController movementController;
    private bool isSurviving;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Apply AI parameters from ScriptableObject if assigned
        if (useAIParameters && aiParameters != null)
        {
            moveSpeed = aiParameters.moveSpeed;
            rotationSpeed = aiParameters.rotationSpeed;
            detectionRange = aiParameters.detectionRange;
            shootingRange = aiParameters.shootingRange;
            minDistanceToPlayer = aiParameters.minDistanceToPlayer;

            bulletPrefab = aiParameters.bulletPrefab != null ? aiParameters.bulletPrefab : bulletPrefab;
            bulletSpeed = aiParameters.bulletSpeed;
            fireRate = aiParameters.fireRate;
            maxHealth = aiParameters.maxHealth;

            patrolRadius = aiParameters.patrolRadius;
            patrolWaitTime = aiParameters.patrolWaitTime;
            obstacleLayer = aiParameters.obstacleLayer;
        }

        currentHealth = maxHealth;
        patrolCenter = transform.position;
        lastValidPosition = transform.position;

        SetNewPatrolTarget();

        // ensure helpers
        if (dangerScanner == null) dangerScanner = gameObject.GetComponent<AIDangerScanner>() ?? gameObject.AddComponent<AIDangerScanner>();
        if (movementController == null) movementController = gameObject.GetComponent<AIMovementController>() ?? gameObject.AddComponent<AIMovementController>();

        // setup NavMeshAgent if enabled
        if (useNavMesh)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
            agent.updateRotation = false; // we rotate tank body manually
            agent.speed = Mathf.Max(0.1f, moveSpeed);
            agent.angularSpeed = Mathf.Max(120f, rotationSpeed * 2f);
            agent.acceleration = Mathf.Max(4f, moveSpeed * 4f);
            agent.stoppingDistance = Mathf.Max(0f, Mathf.Max(minDistanceToPlayer - navStoppingDistanceBuffer, 0f));
            navRepathTimer = 0f;
        }
    }

    void Start()
    {
        //M䪱a]Ҭ"Player"^
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"[EnemyTank] Found player: {player.name}");
        }

        if (player == null)
        {
            Debug.LogWarning("EnemyTank: No player found with tag 'Player'");
        }

        // 檢查 turret 是否設定
        if (turret == null)
            Debug.LogError($"[EnemyTank {name}] Turret Transform is NULL! Please assign in Inspector.");
        else
            Debug.Log($"[EnemyTank {name}] Turret assigned: {turret.name}");

        if (tankBody == null)
            Debug.LogWarning($"[EnemyTank {name}] Tank Body is NULL.");
    }

    void Update()
    {
        if (currentState == AIState.Dead) return;

        UpdateAI();
        CheckStuck();
    }

    void FixedUpdate()
    {
        if (currentState == AIState.Dead) return;

        HandleMovement();
        // tick movement controller sub-queues
        if (!useNavMesh)
            movementController.TickMovement();
        else
            UpdateNavmeshBodyRotation();
    }

    private void HandleMovement()
    {
        // ޿bUӪABzk{
        //o̤ݭnB~޿�
    }

    private void UpdateAI()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CanSeePlayer();

        // Debug: 印出當前狀態與距離 (每秒一次)
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[EnemyTank {name}] State={currentState}, Dist={distanceToPlayer:F1}, CanSee={canSeePlayer}, DetectRange={detectionRange}, ShootRange={shootingRange}");
        }

        // Scan danger and evasion like AITank.Evasion
        if (useAIParameters && aiParameters != null)
        {
            dangerScanner.Scan(
                aiParameters.awarenessFriendlyShell,
                aiParameters.awarenessHostileShell,
                aiParameters.awarenessFriendlyMine,
                aiParameters.awarenessHostileMine
            );
            if (dangerScanner.TryGetAverageDanger(out var avg))
            {
                isSurviving = true;
                Avoid(avg);
            }
            else isSurviving = false;
        }

        // ���A�� (state machine)
        switch (currentState)
        {
            case AIState.Patrol:
                HandlePatrolState(distanceToPlayer, canSeePlayer);
                break;

            case AIState.Chase:
                HandleChaseState(distanceToPlayer, canSeePlayer);
                break;

            case AIState.Attack:
                HandleAttackState(distanceToPlayer, canSeePlayer);
                break;
        }

        // �������]�b�����M�l�����A�ɺ˷Ǫ��a�^
        // Always aim turret toward player while chasing/attacking.
        // Shooting仍依賴 canSeePlayer 與距離，但瞄準不再被遮擋判定阻擋。
        if (currentState == AIState.Chase || currentState == AIState.Attack)
        {
            if (Time.frameCount % 60 == 0)
                Debug.Log($"[EnemyTank {name}] Rotating turret toward player at {player.position}");
            RotateTurretTowards(player.position);
        }
    }

    private void HandlePatrolState(float distanceToPlayer, bool canSeePlayer)
    {
        if (canSeePlayer && distanceToPlayer <= detectionRange)
        {
            currentState = AIState.Chase;
            return;
        }

        // Random movement with aggressiveness bias similar to DoRandomMove
        if (!isWaiting)
        {
            if (useAIParameters && aiParameters != null)
            {
                // occasionally do a random turn toward/away from target
                if (Random.value < 0.02f)
                {
                    float randomTurn = Random.Range(-aiParameters.maxAngleRandomTurn, aiParameters.maxAngleRandomTurn);
                    if (player != null)
                    {
                        var toTarget = (player.position - transform.position).normalized;
                        var targetAngle = Mathf.Atan2(toTarget.x, toTarget.z);
                        float angleDiff = Mathf.DeltaAngle(Mathf.Rad2Deg * targetAngle, Mathf.Rad2Deg * ChassisRotation());
                        randomTurn += angleDiff * aiParameters.aggressivenessBias * Mathf.Deg2Rad;
                    }
                    if (!useNavMesh)
                        DesiredChassisRotationAdd(randomTurn * 0.5f);
                }
            }
            if (useNavMesh)
            {
                // if arrived or no path, pick a new random point within patrol radius
                if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(0.1f, agent.stoppingDistance + 0.1f))
                {
                    Vector3 target = patrolCenter + Random.insideUnitSphere * patrolRadius; target.y = transform.position.y;
                    if (NavMesh.SamplePosition(target, out var hit, patrolRadius, NavMesh.AllAreas))
                        agent.SetDestination(hit.position);
                }
            }
            else
            {
                MoveTowards(currentPatrolTarget);
            }

            if (Vector3.Distance(transform.position, currentPatrolTarget) < 1f)
            {
                isWaiting = true;
                patrolWaitTimer = patrolWaitTime;
            }
        }
        else
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0f)
            {
                SetNewPatrolTarget();
                isWaiting = false;
            }
        }
    }

    private void HandleChaseState(float distanceToPlayer, bool canSeePlayer)
    {
        if (!canSeePlayer || distanceToPlayer > detectionRange * 1.5f)
        {
            currentState = AIState.Patrol;
            return;
        }

        if (distanceToPlayer <= shootingRange)
        {
            currentState = AIState.Attack;
            return;
        }

        // chase player
        if (useNavMesh)
        {
            navRepathTimer -= Time.deltaTime;
            if (navRepathTimer <= 0f)
            {
                agent.isStopped = false;
                agent.stoppingDistance = Mathf.Max(0f, Mathf.Max(minDistanceToPlayer - navStoppingDistanceBuffer, 0f));
                agent.SetDestination(player.position);
                navRepathTimer = navRepathInterval;
            }
        }
        else
        {
            MoveTowards(player.position);
        }
    }

    private void HandleAttackState(float distanceToPlayer, bool canSeePlayer)
    {
        if (!canSeePlayer || distanceToPlayer > shootingRange * 1.2f)
        {
            currentState = AIState.Chase;
            return;
        }

        // �O���Z������
        if (distanceToPlayer < minDistanceToPlayer)
        {
            // ��h
            Vector3 direction = (transform.position - player.position).normalized;
            if (useNavMesh)
            {
                agent.isStopped = false;
                Vector3 flee = transform.position + direction * 2f;
                if (NavMesh.SamplePosition(flee, out var hit, 2f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
            }
            else
            {
                MoveTowards(transform.position + direction * 2f);
            }
        }
        else if (distanceToPlayer > shootingRange)
        {
            //a
            if (useNavMesh)
            {
                agent.isStopped = false;
                agent.stoppingDistance = Mathf.Max(0f, Mathf.Max(minDistanceToPlayer - navStoppingDistanceBuffer, 0f));
                agent.SetDestination(player.position);
            }
            else
            {
                MoveTowards(player.position);
            }
        }
        else if (useNavMesh)
        {
            // in optimal shooting distance: stop moving
            agent.isStopped = true;
        }

        // 15 (guard if fleeing)
        // 只有擁有明確視線時才允許開火，避免隔牆射擊
        bool allowShoot = canSeePlayer;
        bool cantShoot = useAIParameters && aiParameters != null && aiParameters.cantShootWhileFleeing && isSurviving;
        
        if (Time.frameCount % 60 == 0)
            Debug.Log($"[EnemyTank {name}] Attack: allowShoot={allowShoot}, cantShoot={cantShoot}, isSurviving={isSurviving}");
        
        if (allowShoot && !cantShoot)
            TryShoot();
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 dir = (player.position - origin).normalized;
        float distance = Vector3.Distance(origin, player.position);

        // 射線取回沿途所有命中，依距離排序，判斷第一個遇到的是障礙還是玩家
        var hits = Physics.RaycastAll(origin, dir, distance, ~0, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0)
            return true; // 沒有任何命中，視為可見

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var h in hits)
        {
            if (h.collider == null) continue;
            var go = h.collider.gameObject;

            // 忽略自身與子物件
            if (go == gameObject || go.transform.IsChildOf(transform))
                continue;

            // 先遇到玩家 => 可見
            if (go == player.gameObject)
            {
                if (Time.frameCount % 60 == 0)
                    Debug.Log($"[EnemyTank {name}] CanSeePlayer=TRUE (first hit=Player)");
                return true;
            }

            // 第一個阻擋物是否在障礙層
            bool isObstacle = (obstacleLayer & (1 << go.layer)) != 0;
            if (isObstacle)
            {
                if (Time.frameCount % 60 == 0)
                    Debug.LogWarning($"[EnemyTank {name}] CanSeePlayer=FALSE (blocked by {go.name}, Layer={LayerMask.LayerToName(go.layer)})");
                return false;
            }

            // 否則忽略（例如裝飾物或非阻擋層），繼續往下檢查
        }

        // 沿途沒有遇到障礙或玩家 => 視為可見
        return true;
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            // ���ʡ]�ץ��G�ϥ� linearVelocity ���N velocity�^
            Vector3 movement = direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(transform.position + movement);

            // ���ਮ��
            if (tankBody != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                tankBody.rotation = Quaternion.Slerp(tankBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }

    private void RotateTurretTowards(Vector3 targetPosition)
    {
        if (turret == null)
        {
            if (Time.frameCount % 120 == 0)
                Debug.LogError($"[EnemyTank {name}] RotateTurretTowards called but turret is NULL!");
            return;
        }

        Vector3 toTarget = (targetPosition - turret.position).normalized;
        // apply aim offset in radians around Y
        if (useAIParameters && aiParameters != null && Mathf.Abs(aiParameters.aimOffsetRadians) > 0.0001f)
        {
            float offset = Random.Range(-aiParameters.aimOffsetRadians, aiParameters.aimOffsetRadians);
            toTarget = Quaternion.AngleAxis(offset * Mathf.Rad2Deg, Vector3.up) * toTarget;
        }
        Vector3 direction = toTarget;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float turretSpeed = rotationSpeed;
            if (useAIParameters && aiParameters != null && aiParameters.turretSpeed > 0)
                turretSpeed = aiParameters.turretSpeed;
            turret.rotation = Quaternion.Slerp(turret.rotation, targetRotation, turretSpeed * Time.deltaTime);
        }
    }

    private void TryShoot()
    {
        // Debug 檢查開火條件
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[EnemyTank {name}] TryShoot called: Time={Time.time:F1}, NextFire={nextFireTime:F1}, CanFire={(Time.time >= nextFireTime)}, HasBullet={bulletPrefab != null}, HasFirePoint={firePoint != null}");
        }

        if (Time.time >= nextFireTime && bulletPrefab != null && firePoint != null)
        {
            Debug.Log($"[EnemyTank {name}] FIRING! Creating bullet at {firePoint.position}");

            // randomized fire cadence using TanksRebirth-like timers if set
            if (useAIParameters && aiParameters != null && (aiParameters.randomTimerMaxShoot > 0 || aiParameters.randomTimerMinShoot > 0))
            {
                int minT = Mathf.Max(0, aiParameters.randomTimerMinShoot);
                int maxT = Mathf.Max(minT, aiParameters.randomTimerMaxShoot);
                float chosen = Random.Range(minT, maxT + 1);
                nextFireTime = Time.time + chosen;
            }
            else
            {
                nextFireTime = Time.time + (1f / fireRate);
            }

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

    // --- Helpers to bridge movement controller requirements ---
    public float ObstacleAwarenessMovement() => useAIParameters && aiParameters != null ? Mathf.Max(0f, aiParameters.obstacleAwarenessMovement) : 2f;
    public float ChassisRotationDeg() => Mathf.Rad2Deg * ChassisRotation();
    private float _desiredChassisRotation; // radians
    public void SetDesiredChassisRotation(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            _desiredChassisRotation = Mathf.Atan2(dir.x, dir.z);
            RotateBodyTowards(_desiredChassisRotation);
        }
    }
    public void DesiredChassisRotationAdd(float deltaRadians)
    {
        _desiredChassisRotation += deltaRadians;
        RotateBodyTowards(_desiredChassisRotation);
    }
    private void RotateBodyTowards(float targetRad)
    {
        if (tankBody == null) return;
        var dir = new Vector3(Mathf.Sin(targetRad), 0, Mathf.Cos(targetRad));
        var targetRot = Quaternion.LookRotation(dir);
        tankBody.rotation = Quaternion.Slerp(tankBody.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
    }
    public float ChassisRotation()
    {
        if (tankBody == null) return 0f;
        var fwd = tankBody.forward;
        return Mathf.Atan2(fwd.x, fwd.z);
    }
    public int MaxQueuedMovements() => useAIParameters && aiParameters != null && aiParameters.maxQueuedMovements > 0 ? aiParameters.maxQueuedMovements : 4;
    public int ObstacleMask() => obstacleLayer;

    private void Avoid(Vector3 location)
    {
        if (aiParameters == null) return;
        var away = (transform.position - location).normalized;
        var dir = new Vector3(away.x, 0, away.z);
        SetDesiredChassisRotation(dir);
    }

    private void SetNewPatrolTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection.y = 0;
        currentPatrolTarget = patrolCenter + randomDirection;
    }

    private void CheckStuck()
    {
        if (Vector3.Distance(transform.position, lastValidPosition) < 0.1f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckCheckInterval)
            {
                // �Q�d���F�A�]�m�s�����ޥؼ�
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

    private void UpdateNavmeshBodyRotation()
    {
        if (!useNavMesh || agent == null) return;
        Vector3 vel = agent.velocity;
        vel.y = 0f;
        if (vel.sqrMagnitude > 0.01f && tankBody != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(vel.normalized);
            tankBody.rotation = Quaternion.Slerp(tankBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // {IDamageable
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
            // �Q�����ɶi�J�l�����A
            if (attacker != null && attacker.CompareTag("Player"))
            {
                player = attacker.transform;
                currentState = AIState.Chase;
            }
        }
    }

    private void Die()
    {
        currentState = AIState.Dead;

        // ����ʡ]�ץ��G�ϥ� linearVelocity ���N velocity�^
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        // �q��GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDestroyed();
        }

        // �i�H�b�o�̲K�[���`�S��
        Debug.Log("Enemy tank destroyed!");

        // �P������]�Ϊ̸T�βե�^
        Destroy(gameObject, 1f);
    }

    // �����Ϊ�Gizmos
    void OnDrawGizmosSelected()
    {
        // �˴��d��
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // �g���d��
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        // ���޽d��
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(patrolCenter, patrolRadius);

        // ���e���ޥؼ�
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(currentPatrolTarget, 0.5f);

        // ����˴��d����ҡ]�Ȧb�s�边���^
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"State: {currentState}");
#endif
    }
}