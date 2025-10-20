using UnityEngine;

public class EnemyTank : MonoBehaviour, IDamageable
{
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

    [Header("Death Effects")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float explosionDuration = 2f;

    // AI狀態
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

    // 戰鬥相關
    private float currentHealth;
    private float nextFireTime;

    // 巡邏相關
    private Vector3 patrolCenter;
    private Vector3 currentPatrolTarget;
    private float patrolWaitTimer;
    private bool isWaiting = false;

    // 移動相關
    private Vector3 lastValidPosition;
    private float stuckTimer = 0f;
    private float stuckCheckInterval = 2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        currentHealth = maxHealth;
        patrolCenter = transform.position;
        lastValidPosition = transform.position;

        SetNewPatrolTarget();
    }

    void Start()
    {
        // 尋找玩家（標籤為"Player"的物件）
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (player == null)
        {
            Debug.LogWarning("EnemyTank: No player found with tag 'Player'");
        }
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
    }

    private void HandleMovement()
    {
        // 移動邏輯在各個狀態處理方法中實現
        // 這裡不需要額外的邏輯
    }

    private void UpdateAI()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CanSeePlayer();

        // 狀態機
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

        // 砲塔旋轉（在攻擊和追擊狀態時瞄準玩家）
        if ((currentState == AIState.Chase || currentState == AIState.Attack) && canSeePlayer)
        {
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

        // 巡邏邏輯
        if (!isWaiting)
        {
            MoveTowards(currentPatrolTarget);

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

        // 追擊玩家
        MoveTowards(player.position);
    }

    private void HandleAttackState(float distanceToPlayer, bool canSeePlayer)
    {
        if (!canSeePlayer || distanceToPlayer > shootingRange * 1.2f)
        {
            currentState = AIState.Chase;
            return;
        }

        // 保持距離攻擊
        if (distanceToPlayer < minDistanceToPlayer)
        {
            // 後退
            Vector3 direction = (transform.position - player.position).normalized;
            MoveTowards(transform.position + direction * 2f);
        }
        else if (distanceToPlayer > shootingRange)
        {
            // 靠近
            MoveTowards(player.position);
        }

        // 射擊
        TryShoot();
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        // 射線檢測是否被障礙物遮擋
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToPlayer, distance, obstacleLayer))
        {
            return false;
        }

        return true;
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            // 移動（修正：使用 linearVelocity 替代 velocity）
            Vector3 movement = direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(transform.position + movement);

            // 旋轉車身
            if (tankBody != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                tankBody.rotation = Quaternion.Slerp(tankBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
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
            turret.rotation = Quaternion.Slerp(turret.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void TryShoot()
    {
        if (Time.time >= nextFireTime && bulletPrefab != null && firePoint != null)
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
                // 被卡住了，設置新的巡邏目標
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

    // 實現IDamageable介面
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
            // 被攻擊時進入追擊狀態
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

        // 停止移動（修正：使用 linearVelocity 替代 velocity）
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        // Play explosion sound
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // Create explosion visual effect
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            if (explosionDuration > 0)
            {
                Destroy(explosion, explosionDuration);
            }
        }

        // 通知GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDestroyed();
        }

        // 可以在這裡添加死亡特效
        Debug.Log("Enemy tank destroyed!");

        // 銷毀物件（或者禁用組件）
        Destroy(gameObject);
    }

    // 除錯用的Gizmos
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
        Gizmos.DrawWireSphere(patrolCenter, patrolRadius);

        // 當前巡邏目標
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(currentPatrolTarget, 0.5f);

        // 顯示檢測範圍標籤（僅在編輯器中）
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"State: {currentState}");
#endif
    }
}