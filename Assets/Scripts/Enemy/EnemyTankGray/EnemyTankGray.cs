using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// EnemyTankGray
/// 特性：
/// - 會主動追蹤玩家並靠近
/// - 會旋轉砲塔朝向玩家並射擊
/// - 不會做任何子彈 / 危險偵測與躲避，行為比紫色坦克簡單很多
/// </summary>
public class EnemyTankGray : MonoBehaviour, IDamageable
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 150f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float shootingRange = 4f;
    [SerializeField] private float stopDistance = 2f;   // 跟玩家太近就不再往前

    [Header("Pathfinding Settings")]
    [SerializeField] private float pathfindingCheckRadius = 0.8f;      // 用來檢查該格是否可通行
    [SerializeField] private LayerMask obstacleLayerMask;              // 牆壁 / 障礙物的 Layer（請在 Inspector 設定）
    [SerializeField] private float boundaryX = 20f;                    // X 邊界（避免走出地圖）
    [SerializeField] private float boundaryZ = 20f;                    // Z 邊界
    [SerializeField] private float pathUpdateInterval = 1.0f;          // 重新計算路徑的間隔秒數

    [Header("Tank Parts")]
    [SerializeField] private Transform tankBody;
    [SerializeField] private Transform turret;
    [SerializeField] private Transform firePoint;

    [Header("Combat Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 7f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float maxHealth = 1f;

    [Header("Death Effects")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float explosionDuration = 2f;

    private enum AIState
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    [SerializeField] private AIState currentState = AIState.Idle;

    private Transform player;
    private Rigidbody rb;
    private float currentHealth;
    private float nextFireTime;

    // 簡易 A* 路徑資料（比紫色坦克簡化很多，不做牆壁成本計算，只要能走就好）
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int currentPathIndex = 0;
    private bool hasValidPath = false;
    private float lastPathUpdateTime = -999f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        currentHealth = maxHealth;
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (player == null)
        {
            Debug.LogWarning("[EnemyTankGray] No player found with tag 'Player'");
        }
    }

    void Update()
    {
        if (currentState == AIState.Dead) return;
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 簡單狀態切換
        switch (currentState)
        {
            case AIState.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = AIState.Chase;
                }
                break;

            case AIState.Chase:
                if (distanceToPlayer <= shootingRange)
                {
                    currentState = AIState.Attack;
                }
                else if (distanceToPlayer > detectionRange * 1.5f)
                {
                    currentState = AIState.Idle;
                }
                break;

            case AIState.Attack:
                if (distanceToPlayer > shootingRange * 1.2f)
                {
                    currentState = AIState.Chase;
                }
                else if (distanceToPlayer > detectionRange * 1.5f)
                {
                    currentState = AIState.Idle;
                }
                break;
        }

        // 執行對應狀態邏輯
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdle();
                break;
            case AIState.Chase:
                HandleChase(distanceToPlayer);
                break;
            case AIState.Attack:
                HandleAttack(distanceToPlayer);
                break;
        }
    }

    void FixedUpdate()
    {
        if (currentState == AIState.Dead) return;
        // Gray 坦克的移動邏輯放在 HandleChase / HandleAttack 裡用 rb.MovePosition 控制
    }

    private void HandleIdle()
    {
        // 不做任何事，原地待機
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void HandleChase(float distanceToPlayer)
    {
        if (player == null || rb == null) return;

        // 先嘗試用 A* 找到到玩家的路徑
        UpdatePathToPlayer();

        Vector3 moveDir = Vector3.zero;

        if (hasValidPath && currentPathIndex < currentPath.Count && distanceToPlayer > stopDistance)
        {
            // 取得目前要前往的路徑節點
            Vector2Int currentWaypoint = currentPath[currentPathIndex];
            Vector3 targetPos = new Vector3(currentWaypoint.x, transform.position.y, currentWaypoint.y);

            float distToWaypoint = Vector3.Distance(transform.position, targetPos);
            if (distToWaypoint < 0.5f)
            {
                // 到達該節點，前往下一個
                currentPathIndex++;
            }
            else
            {
                moveDir = (targetPos - transform.position);
                moveDir.y = 0;
                moveDir.Normalize();
            }
        }
        else
        {
            // 如果目前沒有有效路徑，就退回「直接往玩家方向走」的簡單行為
            Vector3 directionToPlayer = (player.position - transform.position);
            directionToPlayer.y = 0;
            if (directionToPlayer.sqrMagnitude > 0.0001f)
            {
                moveDir = directionToPlayer.normalized;
            }
        }

        // 只要距離大於停止距離，而且有有效移動方向，就往前走
        if (distanceToPlayer > stopDistance && moveDir.sqrMagnitude > 0.0001f)
        {
            Vector3 movement = moveDir * moveSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + movement);
        }

        // 旋轉坦克身體面向「移動方向」（如果沒有移動，就朝玩家）
        Vector3 bodyDir = moveDir;
        if (bodyDir.sqrMagnitude < 0.0001f)
        {
            bodyDir = (player.position - transform.position);
            bodyDir.y = 0;
        }

        if (tankBody != null && bodyDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(bodyDir.normalized);
            tankBody.rotation = Quaternion.Slerp(tankBody.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // 砲塔永遠朝向玩家
        RotateTurretTowards(player.position);
    }

    private void HandleAttack(float distanceToPlayer)
    {
        // 不再往前衝，只原地調整砲塔並射擊
        RotateTurretTowards(player.position);
        TryShoot();
    }

    private void RotateTurretTowards(Vector3 targetPos)
    {
        if (turret == null) return;

        Vector3 dir = (targetPos - turret.position);
        dir.y = 0;
        if (dir.sqrMagnitude < 0.0001f) return;

        dir.Normalize();
        Quaternion targetRot = Quaternion.LookRotation(dir);
        turret.rotation = Quaternion.Slerp(turret.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    private void TryShoot()
    {
        if (Time.time < nextFireTime) return;
        if (bulletPrefab == null || firePoint == null) return;

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

    // IDamageable 實作
    public void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker)
    {
        // 只接受玩家的傷害
        if (attacker == null || !attacker.CompareTag("Player"))
        {
            return;
        }

        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            // 被打到時，如果還沒看到玩家，強制鎖定攻擊目標
            player = attacker.transform;
            if (currentState != AIState.Dead)
            {
                currentState = AIState.Chase;
            }
        }
    }

    private void Die()
    {
        currentState = AIState.Dead;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            if (explosionDuration > 0)
            {
                Destroy(explosion, explosionDuration);
            }
        }

        // TODO: 如需通知 GameManager，可在此呼叫
        Destroy(gameObject);
    }

    // ======== A* 路徑尋找（簡化版，只用來追玩家，不做躲子彈） ========

    /// <summary>
    /// 定期重新計算從自己位置到玩家位置的 A* 路徑。
    /// </summary>
    private void UpdatePathToPlayer()
    {
        if (player == null) return;

        // 間隔時間內已經有有效路徑就不重算
        if (hasValidPath &&
            currentPathIndex < currentPath.Count &&
            Time.time - lastPathUpdateTime < pathUpdateInterval)
        {
            return;
        }

        lastPathUpdateTime = Time.time;

        Vector2Int start = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z));

        Vector2Int end = new Vector2Int(
            Mathf.RoundToInt(player.position.x),
            Mathf.RoundToInt(player.position.z));

        currentPath = AStarPathfinder.FindPath(start, end, IsPositionWalkable, GetPositionCost);
        currentPathIndex = 0;
        hasValidPath = currentPath != null && currentPath.Count > 0;
    }

    /// <summary>
    /// 判斷某個格子是否可以通行（只檢查是否撞到障礙物，邊界也一起限制）。
    /// </summary>
    private bool IsPositionWalkable(Vector2Int position)
    {
        // 邊界限制，避免走出地圖
        if (Mathf.Abs(position.x) > boundaryX || Mathf.Abs(position.y) > boundaryZ)
            return false;

        Vector3 worldPos = new Vector3(position.x, transform.position.y, position.y);

        // 檢查是否有障礙物
        Collider[] colliders = Physics.OverlapSphere(worldPos, pathfindingCheckRadius * 0.5f, obstacleLayerMask);
        return colliders == null || colliders.Length == 0;
    }

    /// <summary>
    /// 灰坦克不需要牆壁成本，只要能走就好，所以成本固定 1。
    /// </summary>
    private float GetPositionCost(Vector2Int position)
    {
        return 1f;
    }
}


