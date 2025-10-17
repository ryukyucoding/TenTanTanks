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

        // 如果沒有Rigidbody，添加一個
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false; // 子彈不受重力影響
        }

        // 如果沒有Collider，添加一個球形碰撞器
        if (bulletCollider == null)
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 0.05f;
            sphereCollider.isTrigger = true;
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
    }

    void OnTriggerEnter(Collider other)
    {
        // 避免重複觸發
        if (hasHit) return;

        // 忽略發射者
        if (other.gameObject == shooter) return;

        // 檢查是否在可擊中的層級中
        if (((1 << other.gameObject.layer) & hitLayers) == 0) return;

        // 處理擊中
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
            // 在擊中位置播放3D音效
            AudioSource.PlayClipAtPoint(hitSound, position);
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