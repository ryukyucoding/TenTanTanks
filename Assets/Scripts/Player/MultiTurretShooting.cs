using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// 增強坦克射擊系統，支持：
/// - 多炮塔同時射擊
/// - 基於炮塔大小的子彈大小縮放
/// - 不同升級的不同子彈類型
/// - 所有炮塔的同步射擊
/// </summary>
public class MultiTurretShooting : MonoBehaviour
{
    [Header("射擊設置")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float baseBulletSpeed = 5f;
    [SerializeField] private float baseFireRate = 1.2f;
    [SerializeField] private float bulletLifetime = 5f;

    [Header("多炮塔設置")]
    [SerializeField] private bool fireAllTurretsSimultaneously = true;
    [SerializeField] private float turretFireDelay = 0.05f; // 炮塔間的輕微延遲以產生效果

    [Header("自動射擊設置")]
    [SerializeField] private bool enableAutoFire = false;

    [Header("音頻和效果")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private ParticleSystem muzzleFlashPrefab;

    // 組件引用
    private TankController tankController;
    private ModularTankController modularTank;
    private AudioSource audioSource;

    // 射擊狀態
    private float nextFireTime = 0f;
    private bool wasAutoFireKeyPressed = false;

    // 多炮塔資料
    private List<Transform> allFirePoints = new List<Transform>();
    private List<ParticleSystem> muzzleFlashes = new List<ParticleSystem>();

    void Awake()
    {
        // 獲取組件引用
        tankController = GetComponent<TankController>();
        modularTank = GetComponent<ModularTankController>();
        audioSource = GetComponent<AudioSource>();

        // 如果缺少則添加 AudioSource
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        InitializeFirePoints();
    }

    private void InitializeFirePoints()
    {
        // 從模組化坦克系統獲取發射點
        if (modularTank != null)
        {
            allFirePoints = modularTank.GetAllFirePoints();
        }

        // 如果模組化系統不可用，則回退到單一發射點
        if (allFirePoints.Count == 0)
        {
            // 嘗試查找 FirePoint 物件
            Transform firePointTransform = transform.Find("FirePoint");
            if (firePointTransform != null)
            {
                allFirePoints.Add(firePointTransform);
            }
            else
            {
                // 如果沒找到 FirePoint，創建一個臨時的
                GameObject tempFirePoint = new GameObject("TempFirePoint");
                tempFirePoint.transform.SetParent(transform);
                tempFirePoint.transform.localPosition = new Vector3(0, 0.25f, 0.45f);
                allFirePoints.Add(tempFirePoint.transform);
                Debug.LogWarning("未找到 FirePoint，創建了臨時發射點");
            }
        }

        // 為每個發射點創建槍口閃光效果
        SetupMuzzleFlashes();

        Debug.Log($"MultiTurretShooting 已初始化，有 {allFirePoints.Count} 個發射點");
    }

    private void SetupMuzzleFlashes()
    {
        muzzleFlashes.Clear();

        if (muzzleFlashPrefab == null) return;

        foreach (Transform firePoint in allFirePoints)
        {
            if (firePoint != null)
            {
                // 為此發射點創建槍口閃光
                ParticleSystem flash = Instantiate(muzzleFlashPrefab, firePoint);
                flash.transform.localPosition = Vector3.zero;
                flash.transform.localRotation = Quaternion.identity;
                muzzleFlashes.Add(flash);
            }
        }
    }

    /// <summary>
    /// 設置子彈速度（由統計系統調用）
    /// </summary>
    public void SetBulletSpeed(float speed)
    {
        baseBulletSpeed = speed;
        Debug.Log($"✓ MultiTurretShooting.SetBulletSpeed: {speed:F2}");
    }

    /// <summary>
    /// 設置射速（由統計系統調用）
    /// </summary>
    public void SetFireRate(float rate)
    {
        baseFireRate = rate;
        Debug.Log($"✓ MultiTurretShooting.SetFireRate: {rate:F2}");
    }

    void Update()
    {
        // 如果模組化坦克改變，更新發射點
        UpdateFirePoints();

        // 處理自動射擊切換
        HandleAutoFireToggle();

        // 處理射擊
        HandleShooting();
    }

    private void UpdateFirePoints()
    {
        // 檢查發射點是否已更改（例如，升級後）
        if (modularTank != null)
        {
            var newFirePoints = modularTank.GetAllFirePoints();
            if (newFirePoints.Count != allFirePoints.Count)
            {
                allFirePoints = newFirePoints;
                SetupMuzzleFlashes();
                Debug.Log($"發射點已更新：{allFirePoints.Count} 個炮塔");
            }
        }
    }

    private void HandleAutoFireToggle()
    {
        if (Keyboard.current != null)
        {
            bool isEKeyPressed = Keyboard.current.eKey.isPressed;

            if (isEKeyPressed && !wasAutoFireKeyPressed)
            {
                enableAutoFire = !enableAutoFire;
                Debug.Log($"自動射擊：{(enableAutoFire ? "已啟用" : "已禁用")}");
            }

            wasAutoFireKeyPressed = isEKeyPressed;
        }
    }

    private void HandleShooting()
    {
        bool shouldShoot = false;

        // 手動射擊（左鍵點擊）
        if (tankController != null && tankController.IsShootPressed())
        {
            shouldShoot = true;
        }
        // 自動射擊模式
        else if (enableAutoFire)
        {
            shouldShoot = true;
        }

        if (shouldShoot && CanShoot())
        {
            if (fireAllTurretsSimultaneously)
            {
                FireAllTurrets();
            }
            else
            {
                FireTurretsSequentially();
            }
        }
    }

    private bool CanShoot()
    {
        return Time.time >= nextFireTime && bulletPrefab != null && allFirePoints.Count > 0;
    }

    private void FireAllTurrets()
    {
        // 設置下次射擊時間
        nextFireTime = Time.time + (1f / baseFireRate);

        // 從所有炮塔同時射擊
        for (int i = 0; i < allFirePoints.Count; i++)
        {
            if (allFirePoints[i] != null)
            {
                FireFromTurret(i);
            }
        }

        // 為所有炮塔播放一次聲音
        PlayShootSound();

        Debug.Log($"從 {allFirePoints.Count} 個炮塔同時射擊");
    }

    private void FireTurretsSequentially()
    {
        // 這可以用於特殊射擊模式
        // 現在，只是用輕微延遲射擊所有炮塔
        for (int i = 0; i < allFirePoints.Count; i++)
        {
            float delay = i * turretFireDelay;
            StartCoroutine(FireTurretWithDelay(i, delay));
        }

        // 設置下次射擊時間
        nextFireTime = Time.time + (1f / baseFireRate);

        // 播放聲音
        PlayShootSound();
    }

    private System.Collections.IEnumerator FireTurretWithDelay(int turretIndex, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (turretIndex < allFirePoints.Count && allFirePoints[turretIndex] != null)
        {
            FireFromTurret(turretIndex);
        }
    }

    private void FireFromTurret(int turretIndex)
    {
        if (turretIndex >= allFirePoints.Count || allFirePoints[turretIndex] == null)
            return;

        Transform firePoint = allFirePoints[turretIndex];

        // 計算射擊位置和方向
        Vector3 firePosition = firePoint.position;
        Vector3 fireDirection;

        // 使用 TankController 的射擊方向，或者使用發射點的方向
        if (tankController != null)
        {
            fireDirection = tankController.GetFireDirection();
        }
        else
        {
            fireDirection = firePoint.forward;
        }

        // 從模組化坦克系統獲取子彈縮放
        float bulletScale = 1f;
        if (modularTank != null)
        {
            bulletScale = modularTank.GetBulletSizeMultiplier();
        }

        // 創建子彈
        GameObject bullet = Instantiate(bulletPrefab, firePosition, Quaternion.LookRotation(fireDirection));

        // 根據炮塔大小縮放子彈
        bullet.transform.localScale = Vector3.one * bulletScale;

        // 設置子彈速度
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = fireDirection * baseBulletSpeed;
        }

        // 配置子彈腳本 - 檢查兩種可能的子彈腳本
        ScalableBullet scalableBullet = bullet.GetComponent<ScalableBullet>();
        if (scalableBullet != null)
        {
            scalableBullet.SetLifetime(bulletLifetime);
            scalableBullet.SetShooter(gameObject);

            // 從模組化坦克應用傷害倍數
            if (modularTank != null)
            {
                float damageMultiplier = modularTank.GetDamageMultiplier();
                scalableBullet.SetDamageMultiplier(damageMultiplier);
            }
        }
        else
        {
            // 回退到原始 Bullet 腳本
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetLifetime(bulletLifetime);
                bulletScript.SetShooter(gameObject);
            }
            else
            {
                // 如果沒有任何子彈腳本，則後備銷毀
                Destroy(bullet, bulletLifetime);
            }
        }

        // 為此炮塔播放槍口閃光
        if (turretIndex < muzzleFlashes.Count && muzzleFlashes[turretIndex] != null)
        {
            muzzleFlashes[turretIndex].Play();
        }

        Debug.Log($"從炮塔 {turretIndex} 射擊 - 縮放：{bulletScale:F2}");
    }

    private void PlayShootSound()
    {
        if (audioSource != null && shootSound != null)
        {
            // 根據炮塔數量稍微改變音調以增加音頻變化
            float pitchVariation = 1f + (allFirePoints.Count - 1) * 0.1f;
            audioSource.pitch = pitchVariation;
            audioSource.PlayOneShot(shootSound);
            audioSource.pitch = 1f; // 重置音調
        }
    }

    /// <summary>
    /// 強制刷新發射點（當坦克配置更改時調用）
    /// </summary>
    public void RefreshFirePoints()
    {
        InitializeFirePoints();
    }

    /// <summary>
    /// 獲取當前活躍炮塔數量
    /// </summary>
    public int GetTurretCount()
    {
        return allFirePoints.Count;
    }

    /// <summary>
    /// 設置子彈預製件（用於不同升級類型）
    /// </summary>
    public void SetBulletPrefab(GameObject newBulletPrefab)
    {
        bulletPrefab = newBulletPrefab;
    }

    /// <summary>
    /// 重置射擊冷卻
    /// </summary>
    public void ResetFireCooldown()
    {
        nextFireTime = 0f;
    }

    /// <summary>
    /// 切換自動射擊模式
    /// </summary>
    public void ToggleAutoFire()
    {
        enableAutoFire = !enableAutoFire;
        Debug.Log($"自動射擊：{(enableAutoFire ? "已啟用" : "已禁用")}");
    }

    #region Debug Methods

    [ContextMenu("測試單炮塔射擊")]
    public void TestSingleFire()
    {
        if (allFirePoints.Count > 0)
        {
            FireFromTurret(0);
        }
    }

    [ContextMenu("測試所有炮塔射擊")]
    public void TestAllTurretsFire()
    {
        FireAllTurrets();
    }

    [ContextMenu("打印炮塔資訊")]
    public void PrintTurretInfo()
    {
        Debug.Log($"=== 多炮塔射擊資訊 ===");
        Debug.Log($"活躍炮塔：{allFirePoints.Count}");
        Debug.Log($"射速：{baseFireRate:F2}");
        Debug.Log($"子彈速度：{baseBulletSpeed:F2}");
        Debug.Log($"自動射擊：{enableAutoFire}");
        Debug.Log($"全部同時射擊：{fireAllTurretsSimultaneously}");

        for (int i = 0; i < allFirePoints.Count; i++)
        {
            if (allFirePoints[i] != null)
            {
                Debug.Log($"  炮塔 {i}：{allFirePoints[i].name} 在 {allFirePoints[i].position}");
            }
        }
    }

    #endregion
}