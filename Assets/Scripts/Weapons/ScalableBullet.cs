using UnityEngine;

/// <summary>
/// 增強子彈腳本，支持：
/// - 基於坦克升級的傷害倍數
/// - 基於炮塔大小的視覺和物理縮放
/// - 不同升級的不同子彈類型
/// - 為縮放子彈改進碰撞檢測
/// </summary>
public class ScalableBullet : MonoBehaviour
{
    [Header("子彈設置")]
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask collisionLayers = -1;

    [Header("視覺效果")]
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private GameObject impactEffectPrefab;

    [Header("縮放設置")]
    [SerializeField] private bool scalePhysics = true;    // 根據子彈大小縮放碰撞器
    [SerializeField] private bool scaleEffects = true;    // 根據子彈大小縮放效果
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3f;

    // 運行時資料
    private float currentDamage;
    private float damageMultiplier = 1f;
    private float bulletScale = 1f;
    private GameObject shooter;
    private Rigidbody bulletRigidbody;
    private Collider bulletCollider;
    private bool hasExploded = false;

    // 引用
    private AudioSource audioSource;

    void Awake()
    {
        // 獲取組件
        bulletRigidbody = GetComponent<Rigidbody>();
        bulletCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();

        // 如果缺少則添加 AudioSource
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 初始化傷害
        currentDamage = baseDamage;

        // 生命週期後自動銷毀
        Destroy(gameObject, lifetime);
    }

    void Start()
    {
        // 應用任何待處理的縮放變更
        ApplyScaling();
    }

    /// <summary>
    /// 從坦克升級設置傷害倍數
    /// </summary>
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
        currentDamage = baseDamage * damageMultiplier;

        Debug.Log($"子彈傷害設置為：{currentDamage:F2}（基礎：{baseDamage}，倍數：{damageMultiplier:F2}）");
    }

    /// <summary>
    /// 設置子彈縮放（影響視覺效果、物理和傷害）
    /// </summary>
    public void SetBulletScale(float scale)
    {
        bulletScale = Mathf.Clamp(scale, minScale, maxScale);
        ApplyScaling();

        Debug.Log($"子彈縮放設置為：{bulletScale:F2}");
    }

    /// <summary>
    /// 設置子彈生命週期
    /// </summary>
    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;

        // 更新銷毀時間
        CancelInvoke(nameof(DestroyBullet));
        Invoke(nameof(DestroyBullet), lifetime);
    }

    /// <summary>
    /// 設置射擊者以避免自我傷害
    /// </summary>
    public void SetShooter(GameObject shooterObject)
    {
        shooter = shooterObject;

        // 忽略與射擊者的碰撞
        if (shooter != null)
        {
            Collider shooterCollider = shooter.GetComponent<Collider>();
            if (shooterCollider != null && bulletCollider != null)
            {
                Physics.IgnoreCollision(bulletCollider, shooterCollider);
            }
        }
    }

    private void ApplyScaling()
    {
        // 縮放視覺外觀
        transform.localScale = Vector3.one * bulletScale;

        // 如果啟用則縮放物理
        if (scalePhysics && bulletCollider != null)
        {
            // 對於球體碰撞器
            SphereCollider sphereCollider = bulletCollider as SphereCollider;
            if (sphereCollider != null)
            {
                // 不需要額外縮放，因為 transform.localScale 已經處理了
            }

            // 對於盒子碰撞器
            BoxCollider boxCollider = bulletCollider as BoxCollider;
            if (boxCollider != null)
            {
                // 不需要額外縮放，因為 transform.localScale 已經處理了
            }
        }

        // 根據體積縮放質量（立方縮放）
        if (bulletRigidbody != null)
        {
            float baseMass = 1f; // 設置基礎質量
            bulletRigidbody.mass = baseMass * (bulletScale * bulletScale * bulletScale);
        }

        // 基於大小的額外傷害縮放
        if (bulletScale != 1f)
        {
            float sizeDamageBonus = Mathf.Pow(bulletScale, 1.5f); // 略少於立方縮放
            currentDamage = baseDamage * damageMultiplier * sizeDamageBonus;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.collider);
    }

    private void HandleCollision(Collider other)
    {
        if (hasExploded || other == null) return;

        // 忽略與射擊者的碰撞
        if (other.gameObject == shooter) return;

        // 檢查是否應該與此層碰撞
        if ((collisionLayers.value & (1 << other.gameObject.layer)) == 0) return;

        // 標記為已爆炸以防止多次爆炸
        hasExploded = true;

        // 計算撞擊點
        Vector3 impactPoint = other.ClosestPoint(transform.position);

        // 嘗試對被擊中的物件造成傷害
        TryDamageTarget(other.gameObject, impactPoint);

        // 創建爆炸效果
        CreateExplosionEffect(impactPoint);

        // 播放爆炸聲音
        PlayExplosionSound();

        // 銷毀子彈
        DestroyBullet();
    }

    private void TryDamageTarget(GameObject target, Vector3 hitPoint)
    {
        // 嘗試不同的傷害介面
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(currentDamage, hitPoint, shooter);
            Debug.Log($"對 {target.name} 造成 {currentDamage:F2} 傷害");
            return;
        }

        // 嘗試 PlayerHealth 組件
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage((int)currentDamage, hitPoint, shooter);
            Debug.Log($"對玩家 {target.name} 造成 {currentDamage:F2} 傷害");
            return;
        }

        // 嘗試 EnemyTank 組件（基於你的項目結構）
        var enemyTank = target.GetComponent<EnemyTank>();
        if (enemyTank != null)
        {
            enemyTank.TakeDamage(currentDamage, hitPoint, shooter);
            Debug.Log($"對敵人坦克 {target.name} 造成 {currentDamage:F2} 傷害");
            return;
        }

        Debug.Log($"擊中 {target.name} 但未找到傷害組件");
    }

    private void CreateExplosionEffect(Vector3 position)
    {
        // 創建粒子爆炸效果
        if (explosionEffect != null)
        {
            ParticleSystem explosion = Instantiate(explosionEffect, position, Quaternion.identity);

            // 根據子彈大小縮放爆炸效果
            if (scaleEffects)
            {
                explosion.transform.localScale = Vector3.one * bulletScale;

                // 調整粒子系統屬性以進行縮放
                var main = explosion.main;
                main.startSize = main.startSize.constant * bulletScale;
                main.startSpeed = main.startSpeed.constant * bulletScale;
            }

            // 自動銷毀爆炸效果
            Destroy(explosion.gameObject, 2f);
        }

        // 創建撞擊效果
        if (impactEffectPrefab != null)
        {
            GameObject impact = Instantiate(impactEffectPrefab, position, Quaternion.identity);

            // 縮放撞擊效果
            if (scaleEffects)
            {
                impact.transform.localScale = Vector3.one * bulletScale;
            }

            // 自動銷毀撞擊效果
            Destroy(impact, 1f);
        }
    }

    private void PlayExplosionSound()
    {
        if (audioSource != null && explosionSound != null)
        {
            // 根據子彈縮放調整音量和音調
            audioSource.volume = Mathf.Clamp(0.5f * bulletScale, 0.1f, 1f);
            audioSource.pitch = Mathf.Clamp(1f / bulletScale, 0.5f, 2f);
            audioSource.PlayOneShot(explosionSound);
        }
    }

    private void DestroyBullet()
    {
        // 即使在超時時也創建小爆炸效果
        if (!hasExploded)
        {
            CreateExplosionEffect(transform.position);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// 獲取當前傷害值
    /// </summary>
    public float GetDamage()
    {
        return currentDamage;
    }

    /// <summary>
    /// 獲取當前縮放
    /// </summary>
    public float GetScale()
    {
        return bulletScale;
    }

    /// <summary>
    /// 設置自訂碰撞層
    /// </summary>
    public void SetCollisionLayers(LayerMask layers)
    {
        collisionLayers = layers;
    }

    #region Static Factory Methods

    /// <summary>
    /// 創建具有特定屬性的子彈
    /// </summary>
    public static GameObject CreateScaledBullet(GameObject bulletPrefab, Vector3 position, Vector3 direction,
                                               float speed, float scale, float damageMultiplier, GameObject shooter)
    {
        if (bulletPrefab == null) return null;

        // 實例化子彈
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.LookRotation(direction));

        // 設置速度
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        // 配置可縮放子彈
        ScalableBullet bulletScript = bullet.GetComponent<ScalableBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetBulletScale(scale);
            bulletScript.SetDamageMultiplier(damageMultiplier);
            bulletScript.SetShooter(shooter);
        }

        return bullet;
    }

    #endregion

    #region Debug Methods

    [ContextMenu("測試小子彈")]
    public void TestSmallBullet()
    {
        SetBulletScale(0.5f);
        SetDamageMultiplier(0.5f);
    }

    [ContextMenu("測試大子彈")]
    public void TestLargeBullet()
    {
        SetBulletScale(2f);
        SetDamageMultiplier(2f);
    }

    [ContextMenu("打印子彈資訊")]
    public void PrintBulletInfo()
    {
        Debug.Log($"=== 子彈資訊 ===");
        Debug.Log($"當前傷害：{currentDamage:F2}");
        Debug.Log($"傷害倍數：{damageMultiplier:F2}");
        Debug.Log($"縮放：{bulletScale:F2}");
        Debug.Log($"質量：{(bulletRigidbody != null ? bulletRigidbody.mass.ToString("F2") : "N/A")}");
        Debug.Log($"射擊者：{(shooter != null ? shooter.name : "無")}");
    }

    #endregion
}