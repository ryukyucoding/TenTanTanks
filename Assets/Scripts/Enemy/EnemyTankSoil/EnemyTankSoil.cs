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

    // AI���A
    private enum AIState
    {
        Idle,
        Attack,
        Dead
    }

    private AIState currentState = AIState.Idle;
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

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        currentHealth = maxHealth;
    }

    void Start()
    {
        // �M�䪱�a�]���Ҭ�"Player"������^
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
    }

    void FixedUpdate()
    {
        if (currentState == AIState.Dead) return;

        HandleMovement();
    }

    private void HandleMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void UpdateAI()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CanSeePlayer();

        // ���A��
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdleState(distanceToPlayer, canSeePlayer);
                break;

            case AIState.Attack:
                HandleAttackState(distanceToPlayer, canSeePlayer);
                break;
        }

        // �������]�b�����M�l�����A�ɺ˷Ǫ��a�^
        if (currentState == AIState.Attack && canSeePlayer)
        {
            RotateTurretTowards(player.position);
        }
    }

    private void HandleIdleState(float distanceToPlayer, bool canSeePlayer)
    {
        if (canSeePlayer && distanceToPlayer <= detectionRange)
        {
            currentState = AIState.Attack;
            return;
        }

        // �����޿�
        // 否則保持待機狀態，不移動
    }

    private void HandleAttackState(float distanceToPlayer, bool canSeePlayer)
    {
        // 如果玩家不在視線範圍內或距離太遠，回到待機狀態
        if (!canSeePlayer || distanceToPlayer > detectionRange * 1.5f)
        {
            currentState = AIState.Idle;
            return;
        }

        // 在攻擊範圍內就開火（不移動）
        if (distanceToPlayer <= shootingRange)
        {
            TryShoot();
        }
    }

    /// <summary>
    /// 視線判定：最笨版本只看距離，不做遮蔽物判斷
    /// 只要玩家在 detectionRange 內，就視為「看到了玩家」。
    /// </summary>
    private bool CanSeePlayer()
    {
        if (player == null) return false;

        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= detectionRange;
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

    // ��{IDamageable����
    public void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker)
    {
        // 只接受玩家造成的傷害，忽略其他敵人坦克的子彈
        if (attacker == null || !attacker.CompareTag("Player"))
        {
            Debug.Log($"[{gameObject.name}] 忽略非玩家傷害，攻擊者: {(attacker != null ? attacker.name : "null")}");
            return;
        }

        currentHealth -= damage;

        Debug.Log($"Enemy tank took {damage} damage from player. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // �Q�����ɶi�J�l�����A
            // 受到攻擊時進入追擊狀態（attacker 已經是玩家，不需要再檢查）
            player = attacker.transform;
            currentState = AIState.Attack;
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

        // �q��GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDestroyed();
        }

        // �i�H�b�o�̲K�[���`�S��
        Debug.Log("Enemy tank destroyed!");

        // �P������]�Ϊ̸T�βե�^
        Destroy(gameObject);
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