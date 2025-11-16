using UnityEngine;
using UnityEngine.InputSystem;  // 加入 Input System 命名空間

public class TankShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;     // ?????
    [SerializeField] private float bulletSpeed = 20f;     // ????
    [SerializeField] private float fireRate = 1f;         // ??????????
    [SerializeField] private float bulletLifetime = 5f;   // ??????

    [Header("Auto Fire Settings")]
    // 不再使用 KeyCode，改用 Input System 的 Key
    
    [Header("Audio & Effects")]
    [SerializeField] private AudioClip shootSound;        // ????
    [SerializeField] private ParticleSystem muzzleFlash;  // ??????

    // ????
    private TankController tankController;
    private AudioSource audioSource;

    // ????
    private float nextFireTime = 0f;
    private bool isAutoFireEnabled = false;  // 自動射擊開關
    private bool wasAutoFireKeyPressed = false;  // 記錄上一幀的按鍵狀態

    void Awake()
    {
        tankController = GetComponent<TankController>();
        audioSource = GetComponent<AudioSource>();

        // ????AudioSource???????
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // 檢查自動射擊切換鍵
        HandleAutoFireToggle();
        
        // 處理射擊
        HandleShooting();
    }

    private void HandleAutoFireToggle()
    {
        // 使用 Input System 的 Keyboard.current 來檢測 E 鍵
        if (Keyboard.current != null)
        {
            bool isEKeyPressed = Keyboard.current.eKey.isPressed;
            
            // 檢測按鍵從未按下變成按下（類似 GetKeyDown）
            if (isEKeyPressed && !wasAutoFireKeyPressed)
            {
                isAutoFireEnabled = !isAutoFireEnabled;
                Debug.Log($"自動射擊: {(isAutoFireEnabled ? "開啟" : "關閉")}");
            }
            
            // 更新按鍵狀態
            wasAutoFireKeyPressed = isEKeyPressed;
        }
    }

    private void HandleShooting()
    {
        // 檢查是否應該射擊
        bool shouldShoot = false;
        
        // 情況 1: 按住左鍵（手動射擊）
        if (tankController.IsShootPressed())
        {
            shouldShoot = true;
        }
        // 情況 2: 自動射擊模式開啟
        else if (isAutoFireEnabled)
        {
            shouldShoot = true;
        }

        // 如果應該射擊且冷卻時間已過
        if (shouldShoot && CanShoot())
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
        // ????????
        nextFireTime = Time.time + (1f / fireRate);

        // ?????????
        Vector3 firePosition = tankController.GetFirePointPosition();
        Vector3 fireDirection = tankController.GetFireDirection();

        // ????
        GameObject bullet = Instantiate(bulletPrefab, firePosition, Quaternion.LookRotation(fireDirection));

        // ???????????? linearVelocity ?? velocity?
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = fireDirection * bulletSpeed;
        }

        // ????????
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetLifetime(bulletLifetime);
            bulletScript.SetShooter(gameObject);  // 設定發射者，避免打到自己
        }
        else
        {
            // ????Bullet?????Destroy????
            Destroy(bullet, bulletLifetime);
        }

        // ????
        PlayShootSound();

        // ????????
        PlayMuzzleFlash();

        // ????
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

    // ???????????????????????
    public void SetBulletPrefab(GameObject newBulletPrefab)
    {
        bulletPrefab = newBulletPrefab;
    }

    // ???????????
    public void SetFireRate(float newFireRate)
    {
        fireRate = newFireRate;
    }

    // ?????????????
    public void ResetFireCooldown()
    {
        nextFireTime = 0f;
    }
}