using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float damage = 1f;           // 固定傷害值（1點血）
    [SerializeField] private float lifetime = 5f;         // 存活時間
    [SerializeField] private LayerMask hitLayers = -1;    // 可以擊中的層級

    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;        // 擊中特效
    [SerializeField] private AudioClip hitSound;          // 擊中音效
    [SerializeField] private float hitEffectLifetime = 2f; // 特效存活時間

    [Header("Bounce Settings")]
    [SerializeField] private LayerMask wallLayers;        // 牆壁層級（會反彈）
    [SerializeField] private float bounceSpeedMultiplier = 1f; // 反彈速度倍率
    [SerializeField] private int maxBounces = 2;          // 最大反彈次數

    // 組件引用
    private Rigidbody rb;
    private Collider bulletCollider;

    // 子彈狀態
    private bool hasHit = false;
    private float spawnTime;
    private float lastBounceTime = -1f;  // 上次反彈時間
    private const float BOUNCE_COOLDOWN = 0.3f;  // 反彈冷卻時間，這段時間都不會反彈   
    private int bounceCount = 0;  // 目前已反彈次數

    // 子彈發射者（避免自傷）
    private GameObject shooter;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bulletCollider = GetComponent<Collider>();
        spawnTime = Time.time;

        // If no Rigidbody, add one
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false; // No gravity

            // Freeze Y position to keep horizontal flight
            rb.constraints = RigidbodyConstraints.FreezePositionY |
                            RigidbodyConstraints.FreezeRotationX |
                            RigidbodyConstraints.FreezeRotationZ;

            // Light mass to prevent physics issues
            rb.mass = 0.01f;
        }

        // If no Collider, add sphere collider
        if (bulletCollider == null)
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 0.25f;
            sphereCollider.isTrigger = true; // 保持 Trigger
        }
        else if (bulletCollider != null)
        {
            // 確保現有 Collider 是 Trigger
            bulletCollider.isTrigger = true;
        }
    }

    void Start()
    {
        // 設置銷毀時間
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 檢查是否超過存活時間
        if (Time.time - spawnTime >= lifetime)
        {
            DestroyBullet();
        }

        // 用 Raycast 檢測前方是否有牆壁（即使牆壁不是 Trigger）
        if (!hasHit && rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            CheckForWallCollision();
        }
    }

    private void CheckForWallCollision()
    {
        // 如果剛剛反彈過，暫時不檢測牆壁（避免連續反彈和卡牆）
        if (Time.time - lastBounceTime < BOUNCE_COOLDOWN)
        {
            return;
        }

        // 向移動方向發射射線
        Vector3 direction = rb.linearVelocity.normalized;
        float checkDistance = rb.linearVelocity.magnitude * Time.deltaTime * 2f; // 增加檢測距離

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, checkDistance))
        {
            // 忽略發射者（穿透友軍）
            if (hit.collider.gameObject == shooter) return;
            if (shooter != null && hit.collider.transform.IsChildOf(shooter.transform)) return;

            // 友軍穿透：檢查是否同陣營
            if (shooter != null && IsFriendly(hit.collider.gameObject))
            {
                Debug.Log($"友軍穿透 (Raycast): {hit.collider.name}");
                return;
            }

            // 如果是牆壁層，但在冷卻時間內，忽略
            if (IsWall(hit.collider.gameObject))
            {
                if (Time.time - lastBounceTime < BOUNCE_COOLDOWN)
                {
                    Debug.Log($"反彈冷卻中，忽略牆壁: {hit.collider.name}");
                    return;
                }
            }

            // 檢查Layer（如果設定了 hitLayers）
            if (hitLayers != -1 && ((1 << hit.collider.gameObject.layer) & hitLayers) == 0)
            {
                return;
            }

            // 檢測到牆壁或其他物體
            Debug.Log($"Raycast 偵測到: {hit.collider.name}, 法線: {hit.normal}");

            // 如果是反彈面，進行反彈
            if (IsBouncePlane(hit.collider.gameObject))
            {
                // 檢查是否還有反彈次數
                if (bounceCount < maxBounces)
                {
                    BounceOffSurface(hit.point, hit.normal);
                }
                else
                {
                    Debug.Log($"反彈次數已達上限 ({maxBounces})，子彈銷毀");
                    DestroyBullet();
                }
                return;
            }
            // 如果是普通牆壁（不是反彈面），直接穿透或銷毀
            else if (IsWall(hit.collider.gameObject))
            {
                // 碰到非反彈面的牆壁，直接銷毀（不反彈）
                Debug.Log($"碰到非反彈面牆壁，子彈銷毀: {hit.collider.name}");
                DestroyBullet();
                return;
            }

            // 否則檢查是否可造成傷害
            hasHit = true;
            HandleHit(hit.collider, hit.point, hit.normal);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 如果剛剛反彈過且碰到的是牆壁，暫時不檢測（給予無敵時間）
        if (IsWall(other.gameObject) && Time.time - lastBounceTime < BOUNCE_COOLDOWN)
        {
            Debug.Log($"反彈冷卻中，穿透牆壁: {other.name}");
            return;
        }

        // 估算擊中點和法線
        Vector3 hitPoint = transform.position;
        
        // 嘗試用 ClosestPoint 取得更精確的碰撞點
        if (other != null)
        {
            hitPoint = other.ClosestPoint(transform.position);
        }
        
        // 計算法線（從碰撞點指向子彈的方向）
        Vector3 hitNormal = (transform.position - hitPoint).normalized;
        
        // 如果法線太小，使用速度反方向
        if (hitNormal.sqrMagnitude < 0.01f && rb != null && rb.linearVelocity.sqrMagnitude > 0.001f)
        {
            hitNormal = -rb.linearVelocity.normalized;
        }
        
        HandleCollision(other, hitPoint, hitNormal);
    }

    private void HandleCollision(Collider other, Vector3 hitPoint, Vector3 hitNormal)
    {
        // 延遲0.05秒再開始碰撞檢測，讓子彈飛出發射者
        if (Time.time - spawnTime < 0.05f)
        {
            Debug.Log("子彈剛發射，暫時忽略碰撞");
            return;
        }

        // 如果在反彈冷卻期間碰到牆壁，直接忽略
        if (IsWall(other.gameObject) && Time.time - lastBounceTime < BOUNCE_COOLDOWN)
        {
            Debug.Log($"反彈冷卻中，忽略牆壁碰撞: {other.name}");
            return;
        }

        Debug.Log($"子彈碰到: {other.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)}, Tag: {other.tag}), Shooter: {(shooter != null ? shooter.name : "NULL")}");

        // 避免重複觸發
        if (hasHit) return;

        // 忽略發射者（雙重保險）
        if (other.gameObject == shooter)
        {
            Debug.Log($"✅ 忽略發射者本身: {other.name}");
            return;
        }
        
        // 也忽略發射者的子物件
        if (shooter != null && other.transform.IsChildOf(shooter.transform))
        {
            Debug.Log($"✅ 忽略發射者的子物件: {other.name}");
            return;
        }

        // 友軍穿透：檢查是否同陣營
        if (shooter != null && IsFriendly(other.gameObject))
        {
            Debug.Log($"✅ 友軍穿透 (Trigger): {other.name}, shooter: {shooter.name}");
            return;
        }

        // 檢查Layer（如果設定了 hitLayers）
        if (hitLayers != -1 && ((1 << other.gameObject.layer) & hitLayers) == 0)
        {
            Debug.Log($"Layer 不符合，忽略: {LayerMask.LayerToName(other.gameObject.layer)}");
            return;
        }

        Debug.Log("擊中目標！");

        // 如果是反彈面，進行反彈
        if (IsBouncePlane(other.gameObject))
        {
            // 檢查是否還有反彈次數
            if (bounceCount < maxBounces)
            {
                BounceOffSurface(hitPoint, hitNormal);
            }
            else
            {
                Debug.Log($"反彈次數已達上限 ({maxBounces})，子彈銷毀");
                DestroyBullet();
            }
            return;
        }
        // 如果是普通牆壁（不是反彈面），直接銷毀
        else if (IsWall(other.gameObject))
        {
            Debug.Log($"碰到非反彈面牆壁，子彈銷毀: {other.name}");
            DestroyBullet();
            return;
        }

        // 處理擊中和銷毀（只有非牆壁目標）
        hasHit = true;
        HandleHit(other, hitPoint, hitNormal);
    }

    private void HandleHit(Collider hitTarget, Vector3 hitPoint, Vector3 hitNormal)
    {
        hasHit = true;

        // 嘗試對目標造成傷害
        IDamageable damageable = hitTarget.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, hitPoint, shooter);
            Debug.Log($"Bullet hit {hitTarget.name} for {damage} damage");
        }
        else
        {
            Debug.Log($"Bullet hit {hitTarget.name} (no damage component)");
        }

        // 創建擊中特效
        CreateHitEffect(hitPoint, hitNormal);

        // 播放擊中音效
        PlayHitSound(hitPoint);

        // 銷毀子彈
        DestroyBullet();
    }

    // 判斷是否為牆壁層
    private bool IsWall(GameObject obj)
    {
        return ((1 << obj.layer) & wallLayers) != 0;
    }

    // 判斷是否為反彈面（優先檢查）
    private bool IsBouncePlane(GameObject obj)
    {
        // 檢查名稱是否包含 "BouncePlane"
        return obj.name.Contains("BouncePlane");
    }

    // 判斷是否應該忽略（玩家：只忽略自己；敵人：忽略所有敵人）
    private bool IsFriendly(GameObject obj)
    {
        if (shooter == null) return false;

        // 直接檢查是否為發射者本身
        if (obj == shooter)
        {
            Debug.Log($"忽略：碰到發射者本身 {obj.name}");
            return true;
        }

        // 檢查是否為發射者的子物件
        if (obj.transform.IsChildOf(shooter.transform))
        {
            Debug.Log($"忽略：碰到發射者的子物件 {obj.name}");
            return true;
        }

        // 檢查是否為發射者的父物件（反向檢查）
        if (shooter.transform.IsChildOf(obj.transform))
        {
            Debug.Log($"忽略：碰到發射者的父物件 {obj.name}");
            return true;
        }

        // 敵人的子彈不能打到其他敵人
        if (shooter.CompareTag("Enemy") && obj.CompareTag("Enemy"))
        {
            Debug.Log($"忽略：敵人友軍穿透 {obj.name}");
            return true;
        }

        // 玩家的子彈可以打到任何目標（已經在上面檢查過自己了）
        return false;
    }

    // 反彈處理
    private void BounceOffSurface(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (rb == null) return;

        // 增加反彈計數
        bounceCount++;

        // 記錄反彈時間
        lastBounceTime = Time.time;

        // 儲存原始法線供 Debug 使用
        Vector3 originalNormal = hitNormal;

        // 使用子彈速度方向來輔助判斷法線（智能標準化）
        Vector3 bulletDirection = rb.linearVelocity.normalized;
        hitNormal = NormalizeToMainAxisSmart(hitNormal, bulletDirection);

        Debug.Log($"子彈反彈！({bounceCount}/{maxBounces}) 入射速度: {rb.linearVelocity}, 原始法線: {originalNormal} -> 標準化法線: {hitNormal}");

        // 計算反射方向
        Vector3 incomingVelocity = rb.linearVelocity;
        Vector3 reflectDir = Vector3.Reflect(incomingVelocity, hitNormal);
        float speed = incomingVelocity.magnitude * bounceSpeedMultiplier;

        Debug.Log($"反彈後速度: {reflectDir.normalized * speed}");

        // 儲存 Debug 資訊
        debugLastHitPoint = hitPoint;
        debugLastNormal = hitNormal;
        debugLastReflectDir = reflectDir.normalized;
        debugLastBounceTime = Time.time;

        // 設置新速度
        rb.linearVelocity = reflectDir.normalized * speed;

        // 將子彈推離牆面，距離更大以避免再次碰撞
        float pushDistance = 0.3f;
        transform.position = hitPoint + hitNormal * pushDistance;

        // 調整子彈朝向
        if (speed > 0.1f)
        {
            transform.forward = reflectDir.normalized;
        }
    }

    // 智能法線標準化：使用子彈方向輔助判斷
    // 當兩個軸的法線值接近時，選擇與子彈方向最對立的軸
    private Vector3 NormalizeToMainAxisSmart(Vector3 normal, Vector3 bulletDirection)
    {
        // 取得各軸的絕對值
        float absX = Mathf.Abs(normal.x);
        float absY = Mathf.Abs(normal.y);
        float absZ = Mathf.Abs(normal.z);

        // 定義"接近"的閾值（如果兩個軸差異小於這個值，就需要用子彈方向判斷）
        float ambiguityThreshold = 0.15f;

        Vector3 result = Vector3.zero;

        // 如果 Y 軸最大，直接返回（子彈在 XZ 平面，通常不會碰到水平面）
        if (absY > absX && absY > absZ)
        {
            result = new Vector3(0, Mathf.Sign(normal.y), 0);
            Debug.Log($"法線標準化 (Y軸): ({normal.x:F2}, {normal.y:F2}, {normal.z:F2}) -> {result}");
            return result;
        }

        // 檢查 X 和 Z 是否接近（模糊情況）
        bool isAmbiguous = Mathf.Abs(absX - absZ) < ambiguityThreshold;

        if (isAmbiguous)
        {
            // X 和 Z 接近，使用子彈方向判斷
            // 選擇與子彈方向最對立的軸（點積最負的）
            float dotX = bulletDirection.x * normal.x;  // 子彈 X 方向與法線 X 的對立程度
            float dotZ = bulletDirection.z * normal.z;  // 子彈 Z 方向與法線 Z 的對立程度

            // 點積越負，表示越對立（子彈撞向這個方向的牆）
            if (dotX < dotZ)
            {
                // 子彈主要撞向 X 方向的牆
                result = new Vector3(Mathf.Sign(normal.x), 0, 0);
                Debug.Log($"法線標準化 (模糊->X軸, dotX={dotX:F2}, dotZ={dotZ:F2}): ({normal.x:F2}, {normal.y:F2}, {normal.z:F2}) -> {result}");
            }
            else
            {
                // 子彈主要撞向 Z 方向的牆
                result = new Vector3(0, 0, Mathf.Sign(normal.z));
                Debug.Log($"法線標準化 (模糊->Z軸, dotX={dotX:F2}, dotZ={dotZ:F2}): ({normal.x:F2}, {normal.y:F2}, {normal.z:F2}) -> {result}");
            }
        }
        else
        {
            // X 和 Z 差異明顯，直接選最大的
            if (absX >= absZ)
            {
                result = new Vector3(Mathf.Sign(normal.x), 0, 0);
                Debug.Log($"法線標準化 (明確->X軸): ({normal.x:F2}, {normal.y:F2}, {normal.z:F2}) -> {result}");
            }
            else
            {
                result = new Vector3(0, 0, Mathf.Sign(normal.z));
                Debug.Log($"法線標準化 (明確->Z軸): ({normal.x:F2}, {normal.y:F2}, {normal.z:F2}) -> {result}");
            }
        }

        return result;
    }

    // 舊版法線標準化（保留作為備用）
    private Vector3 NormalizeToMainAxis(Vector3 normal)
    {
        // 取得各軸的絕對值
        float absX = Mathf.Abs(normal.x);
        float absY = Mathf.Abs(normal.y);
        float absZ = Mathf.Abs(normal.z);

        // 找出最大的分量
        Vector3 result = Vector3.zero;

        if (absX >= absY && absX >= absZ)
        {
            // X 軸最大
            result = new Vector3(Mathf.Sign(normal.x), 0, 0);
        }
        else if (absZ >= absX && absZ >= absY)
        {
            // Z 軸最大
            result = new Vector3(0, 0, Mathf.Sign(normal.z));
        }
        else
        {
            // Y 軸最大（保留原始，因為子彈在 XZ 平面）
            result = new Vector3(0, Mathf.Sign(normal.y), 0);
        }

        Debug.Log($"法線標準化: ({normal.x:F2}, {normal.y:F2}, {normal.z:F2}) -> {result}");
        
        return result;
    }

    private void CreateHitEffect(Vector3 position, Vector3 normal)
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, position, Quaternion.LookRotation(normal));
            Destroy(effect, hitEffectLifetime);
        }
    }

    private void PlayHitSound(Vector3 position)
    {
        if (hitSound != null)
        {
            // 確保有 AudioListener 存在
            if (Camera.main != null && Camera.main.GetComponent<AudioListener>() != null)
            {
                // 在擊中位置播放3D音效
                AudioSource.PlayClipAtPoint(hitSound, position);
            }
            else
            {
                // 如果沒有 AudioListener，嘗試找到一個
                AudioListener listener = FindFirstObjectByType<AudioListener>();
                if (listener != null)
                {
                    AudioSource.PlayClipAtPoint(hitSound, position);
                }
                else
                {
                    Debug.LogWarning("Bullet: 找不到 AudioListener，無法播放音效！");
                }
            }
        }
        else
        {
            Debug.LogWarning("Bullet: hitSound 為空！");
        }
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }

    // 公共方法：設置子彈參數
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;
    }

    public void SetShooter(GameObject newShooter)
    {
        shooter = newShooter;
    }
    
    public GameObject GetShooter()
    {
        return shooter;
    }

    public void SetHitLayers(LayerMask layers)
    {
        hitLayers = layers;
    }

    // 除錯用的Gizmos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.05f);

        if (rb != null && Application.isPlaying)
        {
            // 顯示子彈速度方向（紅色）
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 0.5f);
        }
    }

    // 顯示反彈時的法線和反射方向
    private Vector3 debugLastHitPoint;
    private Vector3 debugLastNormal;
    private Vector3 debugLastReflectDir;
    private float debugLastBounceTime;

    void OnDrawGizmosSelected()
    {
        // 如果最近有反彈，顯示詳細資訊
        if (Application.isPlaying && Time.time - debugLastBounceTime < 2f)
        {
            // 顯示碰撞點（黃色球）
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(debugLastHitPoint, 0.2f);

            // 顯示原始法線（綠色）
            Gizmos.color = Color.green;
            Gizmos.DrawRay(debugLastHitPoint, debugLastNormal * 1f);

            // 顯示反射方向（青色）
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(debugLastHitPoint, debugLastReflectDir * 1f);
        }
    }
}

// 傷害介面（其他腳本可以實現這個介面來接收傷害）
public interface IDamageable
{
    void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker);
}