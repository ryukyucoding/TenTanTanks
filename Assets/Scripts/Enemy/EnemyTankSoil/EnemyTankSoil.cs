using UnityEngine;
using System.Collections;

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

    [Header("Spawn Effects")]
    [SerializeField] private GameObject spawnEffectPrefab;  // 生成特效（FX_Blue.prefab）
    [SerializeField] private float spawnDuration = 2f;     // 生成持续时间（秒）
    [SerializeField] private float effectBrightness = 0.5f;  // 特效亮度（0-1，越小越暗）

    // AI���A
    private enum AIState
    {
        Spawning,  // 生成中
        Idle,
        Attack,
        Dead
    }

    private AIState currentState = AIState.Idle;
    private Transform player;
    private Rigidbody rb;
    private bool isDead = false;  // 防止重复死亡

    // �԰�����
    private float currentHealth;
    private float nextFireTime;

    // 生成相關
    private float spawnStartTime;
    private Vector3 spawnTargetPosition;  // 生成完成後的最終位置
    private float spawnStartY;  // 生成開始時的 Y 位置（地下）
    private GameObject spawnEffectInstance;  // 生成特效實例

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
        // 開始生成流程
        StartSpawning();
        
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

        // 處理生成狀態
        if (currentState == AIState.Spawning)
        {
            HandleSpawning();
            return;
        }

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

    // IDamageable 實作
    public void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker)
    {
        // 生成期間無敵
        if (currentState == AIState.Spawning)
        {
            Debug.Log($"[{gameObject.name}] 生成期間無敵，忽略傷害");
            return;
        }

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
        // 防止重复死亡
        if (isDead) return;
        isDead = true;

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

        // 通知 UpgradePointManager 敵人已被擊殺
        UpgradePointManager upgradeManager = FindFirstObjectByType<UpgradePointManager>();
        if (upgradeManager != null)
        {
            upgradeManager.OnEnemyKilled();
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

    /// <summary>
    /// 開始生成流程：播放特效、設置初始狀態、開始從底下冒出
    /// </summary>
    private void StartSpawning()
    {
        currentState = AIState.Spawning;
        spawnStartTime = Time.time;

        // 記錄最終位置和初始位置（地下）
        spawnTargetPosition = transform.position;
        spawnStartY = spawnTargetPosition.y - 2f;  // 從地下 2 單位開始
        
        // 將坦克移到地下
        transform.position = new Vector3(spawnTargetPosition.x, spawnStartY, spawnTargetPosition.z);

        // 播放生成特效（在目標位置）
        if (spawnEffectPrefab != null)
        {
            spawnEffectInstance = Instantiate(spawnEffectPrefab, spawnTargetPosition, Quaternion.identity);
            // 調整特效亮度
            AdjustEffectBrightness(spawnEffectInstance, effectBrightness);
            // 給特效添加旋轉動畫，讓它看起來更動態
            StartCoroutine(RotateSpawnEffect());
            // 特效會在約2秒後自動銷毀
            Destroy(spawnEffectInstance, spawnDuration);
        }
        
        // 禁用移動（生成期間）
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// 處理生成狀態：從底下慢慢冒出，生成完成後切換到 Idle
    /// </summary>
    private void HandleSpawning()
    {
        float elapsed = Time.time - spawnStartTime;
        float progress = Mathf.Clamp01(elapsed / spawnDuration);

        // 使用緩動函數讓動畫更平滑（Ease Out）
        float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

        // 從地下慢慢冒出：Y 軸位置從 spawnStartY 移動到 spawnTargetPosition.y
        float currentY = Mathf.Lerp(spawnStartY, spawnTargetPosition.y, easedProgress);
        transform.position = new Vector3(spawnTargetPosition.x, currentY, spawnTargetPosition.z);

        // 生成完成
        if (elapsed >= spawnDuration)
        {
            // 確保位置精確
            transform.position = spawnTargetPosition;
            currentState = AIState.Idle;
        }
    }

    /// <summary>
    /// 調整特效的亮度（通過降低材質顏色強度）
    /// </summary>
    private void AdjustEffectBrightness(GameObject effect, float brightnessMultiplier)
    {
        if (effect == null) return;

        // 獲取特效的所有 Renderer 組件
        Renderer[] renderers = effect.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            // 獲取所有材質（使用 materials 而不是 sharedMaterials 以創建實例）
            Material[] materials = renderer.materials;
            
            foreach (Material mat in materials)
            {
                if (mat == null) continue;

                // 調整基礎顏色（_BaseColor 或 _Color）
                if (mat.HasProperty("_BaseColor"))
                {
                    Color baseColor = mat.GetColor("_BaseColor");
                    baseColor *= brightnessMultiplier;
                    mat.SetColor("_BaseColor", baseColor);
                }
                else if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color *= brightnessMultiplier;
                    mat.color = color;
                }

                // 調整發光顏色（如果有 Emission）
                if (mat.HasProperty("_EmissionColor"))
                {
                    Color emissionColor = mat.GetColor("_EmissionColor");
                    emissionColor *= brightnessMultiplier;
                    mat.SetColor("_EmissionColor", emissionColor);
                }
                else if (mat.IsKeywordEnabled("_EMISSION"))
                {
                    // 如果啟用了 Emission，嘗試調整
                    if (mat.HasProperty("_Emission"))
                    {
                        Color emission = mat.GetColor("_Emission");
                        emission *= brightnessMultiplier;
                        mat.SetColor("_Emission", emission);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 旋轉生成特效，讓它看起來更動態
    /// </summary>
    private System.Collections.IEnumerator RotateSpawnEffect()
    {
        if (spawnEffectInstance == null) yield break;

        float rotationSpeed = 90f;  // 每秒旋轉 90 度
        
        while (spawnEffectInstance != null && Time.time - spawnStartTime < spawnDuration)
        {
            spawnEffectInstance.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            yield return null;
        }
    }
}