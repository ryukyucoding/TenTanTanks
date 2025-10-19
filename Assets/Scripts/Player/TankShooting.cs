using UnityEngine;

public class TankShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;     // 子彈預製件
    [SerializeField] private float bulletSpeed = 20f;     // 子彈速度
    [SerializeField] private float fireRate = 1f;         // 射擊頻率（每秒幾發）
    [SerializeField] private float bulletLifetime = 5f;   // 子彈存活時間

    [Header("Audio & Effects")]
    [SerializeField] private AudioClip shootSound;        // 射擊音效
    [SerializeField] private ParticleSystem muzzleFlash;  // 槍口火焰特效

    // 組件引用
    private TankController tankController;
    private AudioSource audioSource;

    // 射擊控制
    private float nextFireTime = 0f;

    void Awake()
    {
        tankController = GetComponent<TankController>();
        audioSource = GetComponent<AudioSource>();

        // 如果沒有AudioSource組件，添加一個
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        HandleShooting();
    }

    private void HandleShooting()
    {
        // 檢查是否可以射擊
        if (tankController.IsShootPressed() && CanShoot())
        {
            Shoot();
        }
    }

    private bool CanShoot()
    {
        return Time.time >= nextFireTime && bulletPrefab != null;
    }

    private void Shoot()
    {
        // 設置下次射擊時間
        nextFireTime = Time.time + (1f / fireRate);

        // 獲取發射位置和方向
        Vector3 firePosition = tankController.GetFirePointPosition();
        Vector3 fireDirection = tankController.GetFireDirection();

        // 創建子彈
        GameObject bullet = Instantiate(bulletPrefab, firePosition, Quaternion.LookRotation(fireDirection));

        // 設置子彈速度（修正：使用 linearVelocity 替代 velocity）
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = fireDirection * bulletSpeed;
        }

        // 設置子彈存活時間
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetLifetime(bulletLifetime);
        }
        else
        {
            // 如果沒有Bullet腳本，使用Destroy作為備案
            Destroy(bullet, bulletLifetime);
        }

        // 播放音效
        PlayShootSound();

        // 播放槍口火焰特效
        PlayMuzzleFlash();

        // 除錯輸出
        Debug.Log($"Tank fired bullet at {firePosition} towards {fireDirection}");
    }

    private void PlayShootSound()
    {
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    private void PlayMuzzleFlash()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

    // 公共方法：設置子彈預製件（用於不同類型的武器）
    public void SetBulletPrefab(GameObject newBulletPrefab)
    {
        bulletPrefab = newBulletPrefab;
    }

    // 公共方法：設置射擊頻率
    public void SetFireRate(float newFireRate)
    {
        fireRate = newFireRate;
    }

    // 公共方法：強制重置射擊冷卻
    public void ResetFireCooldown()
    {
        nextFireTime = 0f;
    }
}