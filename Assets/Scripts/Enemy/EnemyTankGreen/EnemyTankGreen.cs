using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// EnemyTankGreen
/// 特性：
/// - 會主動追蹤玩家並靠近
/// - 會旋轉砲塔朝向玩家並射擊
/// - 不會做任何子彈 / 危險偵測與躲避，行為比紫色坦克簡單很多
/// </summary>
public class EnemyTankGreen : MonoBehaviour, IDamageable
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
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
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float maxHealth = 1f;

    [Header("Death Effects")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float explosionDuration = 2f;

    [Header("Spawn Effects")]
    [SerializeField] private GameObject spawnEffectPrefab;  // 生成特效（FX_Green.prefab）
    [SerializeField] private float spawnDuration = 2f;     // 生成持续时间（秒）
    [SerializeField] private float effectBrightness = 0.5f;  // 特效亮度（0-1，越小越暗）

    private enum AIState
    {
        Spawning,  // 生成中
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
    private bool canSeePlayer = false;  // 視線檢查

    // 生成相關
    private float spawnStartTime;
    private Vector3 spawnTargetPosition;  // 生成完成後的最終位置
    private float spawnStartY;  // 生成開始時的 Y 位置（地下）
    private GameObject spawnEffectInstance;  // 生成特效實例

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
            Debug.LogWarning("[EnemyTankGreen] No player found with tag 'Player'");
        }

        // 開始生成流程
        StartSpawning();
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
        // Green 坦克的移動邏輯放在 HandleChase / HandleAttack 裡用 rb.MovePosition 控制
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
        // 檢查視線
        CheckLineOfSight();

        if (canSeePlayer)
        {
            // 能看到玩家，原地調整砲塔並射擊
            RotateTurretTowards(player.position);
            TryShoot();
        }
        else
        {
            // 看不到玩家（被牆擋住），繼續按照 A* 路徑移動靠近
            // 使用跟 Chase 一樣的移動邏輯
            HandleChase(distanceToPlayer);
        }
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

    /// <summary>
    /// 檢查是否能看到玩家（中間沒有障礙物擋住）
    /// 使用 SphereCast 考慮子彈的半徑，避免在轉角處誤判
    /// </summary>
    private void CheckLineOfSight()
    {
        canSeePlayer = false;

        if (player == null || turret == null) return;

        Vector3 directionToPlayer = (player.position - turret.position).normalized;
        float distanceToPlayer = Vector3.Distance(turret.position, player.position);

        // 使用子彈的半徑進行檢測（子彈半徑約 0.25，使用 0.3 稍微寬鬆一點）
        float bulletRadius = 0.3f;
        Vector3 rayStart = turret.position;
        
        // 使用 SphereCast 從砲塔位置向玩家發射有粗細的射線
        RaycastHit hit;
        if (Physics.SphereCast(rayStart, bulletRadius, directionToPlayer, out hit, distanceToPlayer, obstacleLayerMask))
        {
            // 如果射線打到的是玩家，表示能看到
            if (hit.collider.CompareTag("Player"))
            {
                canSeePlayer = true;
            }
            // 否則中間有障礙物擋住，不能射擊
        }
        else
        {
            // 沒有擊中任何障礙物，視為可以看到玩家
            canSeePlayer = true;
        }
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
        // 生成期間無敵
        if (currentState == AIState.Spawning)
        {
            return;
        }

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

        // 通知 GameManager 有一個敵人被消滅，用於統計關卡進度
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDestroyed();
        }

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


