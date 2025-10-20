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
        CheckStuck();
    }

    void FixedUpdate()
    {
        if (currentState == AIState.Dead) return;

        HandleMovement();
    }

    private void HandleMovement()
    {
        // �����޿�b�U�Ӫ��A�B�z��k����{
        // �o�̤��ݭn�B�~���޿�
    }

    private void UpdateAI()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CanSeePlayer();

        // ���A��
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

        // �����޿�
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

        // �l�����a
        MoveTowards(player.position);
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
            MoveTowards(transform.position + direction * 2f);
        }
        else if (distanceToPlayer > shootingRange)
        {
            // �a��
            MoveTowards(player.position);
        }

        // �g��
        TryShoot();
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        // �g�u�˴��O�_�Q��ê���B��
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