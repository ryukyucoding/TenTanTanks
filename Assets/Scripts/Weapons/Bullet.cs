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

    // 組件引用
    private Rigidbody rb;
    private Collider bulletCollider;

    // 子彈狀態
    private bool hasHit = false;
    private float spawnTime;

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
            sphereCollider.radius = 0.25f;  // Bigger than before but reasonable
            sphereCollider.isTrigger = true; // Must be trigger
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
        // 向移動方向發射射線
        Vector3 direction = rb.linearVelocity.normalized;
        float checkDistance = rb.linearVelocity.magnitude * Time.deltaTime * 1.5f; // 稍微多一點距離

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, checkDistance))
        {
            // 忽略發射者
            if (hit.collider.gameObject == shooter) return;
            if (shooter != null && hit.collider.transform.IsChildOf(shooter.transform)) return;

            // 檢查Layer（如果設定了 hitLayers）
            if (hitLayers != -1 && ((1 << hit.collider.gameObject.layer) & hitLayers) == 0)
            {
                return;
            }

            // 檢測到牆壁或其他物體
            Debug.Log($"Raycast 偵測到: {hit.collider.name}");
            
            hasHit = true;
            HandleHit(hit.collider);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
    }

    private void HandleCollision(Collider other)
    {
        // 延遲0.05秒再開始碰撞檢測，讓子彈飛出發射者
        if (Time.time - spawnTime < 0.05f)
        {
            Debug.Log("子彈剛發射，暫時忽略碰撞");
            return;
        }

        Debug.Log($"子彈碰到: {other.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");

        // 避免重複觸發
        if (hasHit) return;

        // 忽略發射者（雙重保險）
        if (other.gameObject == shooter) return;
        // 也忽略發射者的子物件
        if (shooter != null && other.transform.IsChildOf(shooter.transform)) return;

        // 檢查Layer（如果設定了 hitLayers）
        if (hitLayers != -1 && ((1 << other.gameObject.layer) & hitLayers) == 0)
        {
            Debug.Log($"Layer 不符合，忽略: {LayerMask.LayerToName(other.gameObject.layer)}");
            return;
        }

        Debug.Log("擊中目標！");

        // 處理擊中和銷毀
        hasHit = true;
        HandleHit(other);
    }

    private void HandleHit(Collider hitTarget)
    {
        hasHit = true;

        // 獲取擊中位置
        Vector3 hitPoint = transform.position;
        Vector3 hitNormal = -transform.forward; // 假設子彈朝前飛行

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

    public void SetHitLayers(LayerMask layers)
    {
        hitLayers = layers;
    }

    // 除錯用的Gizmos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.05f);

        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 0.5f);
        }
    }
}

// 傷害介面（其他腳本可以實現這個介面來接收傷害）
public interface IDamageable
{
    void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker);
}