using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;         // 最大血量點數
    [SerializeField] private int currentHealth;         // 當前血量點數

    [Header("Damage Effects")]
    [SerializeField] private float invulnerabilityTime = 1f; // 無敵時間
    [SerializeField] private GameObject damageEffect;        // 受傷特效
    [SerializeField] private AudioClip damageSound;          // 受傷音效
    [SerializeField] private AudioClip deathSound;           // 死亡音效

    [Header("Visual Feedback")]
    [SerializeField] private Renderer[] tankRenderers;      // 坦克的渲染器（用於閃爍效果）
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    // 組件引用
    private AudioSource audioSource;

    // 狀態變數
    private bool isInvulnerable = false;
    private float lastDamageTime;
    private Color[] originalColors;

    // 事件
    public System.Action<int, int> OnHealthChanged; // 當前血量, 最大血量
    public System.Action OnPlayerDeath;

    // 公共屬性
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;
    public float HealthPercentage => (float)currentHealth / maxHealth;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        currentHealth = maxHealth;

        // 保存原始顏色
        if (tankRenderers != null && tankRenderers.Length > 0)
        {
            originalColors = new Color[tankRenderers.Length];
            for (int i = 0; i < tankRenderers.Length; i++)
            {
                if (tankRenderers[i] != null && tankRenderers[i].material != null)
                {
                    originalColors[i] = tankRenderers[i].material.color;
                }
            }
        }
    }

    void Start()
    {
        // 通知UI初始血量
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        // 檢查無敵時間
        if (isInvulnerable && Time.time - lastDamageTime >= invulnerabilityTime)
        {
            isInvulnerable = false;
            RestoreOriginalColors();
        }
    }

    // 實現IDamageable介面
    public void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker)
    {
        // 檢查是否處於無敵狀態
        if (isInvulnerable || !IsAlive) return;

        // 減少血量（每次攻擊扣1點血）
        currentHealth = Mathf.Max(0, currentHealth - 1);
        lastDamageTime = Time.time;
        isInvulnerable = true;

        Debug.Log($"Player took 1 damage. Health: {currentHealth}/{maxHealth}");

        // 觸發視覺和音效
        PlayDamageEffects(hitPoint);

        // 通知系統血量變化
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // 通知GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDamaged(currentHealth, maxHealth);
        }

        // 檢查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void PlayDamageEffects(Vector3 hitPoint)
    {
        // 播放受傷音效
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        // 創建受傷特效
        if (damageEffect != null)
        {
            GameObject effect = Instantiate(damageEffect, hitPoint, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // 閃爍效果
        StartFlashEffect();
    }

    private void StartFlashEffect()
    {
        if (tankRenderers == null) return;

        // 改變顏色為受傷顏色
        for (int i = 0; i < tankRenderers.Length; i++)
        {
            if (tankRenderers[i] != null && tankRenderers[i].material != null)
            {
                tankRenderers[i].material.color = damageFlashColor;
            }
        }

        // 短時間後恢復原色
        Invoke(nameof(RestoreOriginalColors), flashDuration);
    }

    private void RestoreOriginalColors()
    {
        if (tankRenderers == null || originalColors == null) return;

        for (int i = 0; i < tankRenderers.Length && i < originalColors.Length; i++)
        {
            if (tankRenderers[i] != null && tankRenderers[i].material != null)
            {
                tankRenderers[i].material.color = originalColors[i];
            }
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");

        // 播放死亡音效
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // 觸發死亡事件
        OnPlayerDeath?.Invoke();

        // 禁用控制
        TankController controller = GetComponent<TankController>();
        if (controller != null)
            controller.enabled = false;

        TankShooting shooting = GetComponent<TankShooting>();
        if (shooting != null)
            shooting.enabled = false;

        // 可以在這裡添加死亡動畫或特效

        // 延遲一段時間後通知GameManager
        Invoke(nameof(NotifyGameManager), 1f);
    }

    private void NotifyGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver("玩家被擊敗！");
        }
    }

    // 公共方法：治療
    public void Heal(int healAmount = 1)
    {
        if (!IsAlive) return;

        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);

        if (currentHealth != oldHealth)
        {
            Debug.Log($"Player healed for {currentHealth - oldHealth}. Health: {currentHealth}/{maxHealth}");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    // 公共方法：設置最大血量
    public void SetMaxHealth(int newMaxHealth)
    {
        float healthPercentage = HealthPercentage;
        maxHealth = newMaxHealth;
        currentHealth = Mathf.RoundToInt(maxHealth * healthPercentage);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // 公共方法：完全治療
    public void FullHeal()
    {
        Heal(maxHealth);
    }

    // 除錯用的Gizmos
    void OnDrawGizmosSelected()
    {
        // 顯示血量條
        Vector3 healthBarPos = transform.position + Vector3.up * 3f;
        Vector3 healthBarSize = new Vector3(2f, 0.2f, 0f);

        // 背景（紅色）
        Gizmos.color = Color.red;
        Gizmos.DrawCube(healthBarPos, healthBarSize);

        // 血量（綠色）
        if (IsAlive)
        {
            Gizmos.color = Color.green;
            Vector3 healthSize = healthBarSize;
            healthSize.x *= HealthPercentage;
            Vector3 healthPos = healthBarPos;
            healthPos.x -= (healthBarSize.x - healthSize.x) * 0.5f;
            Gizmos.DrawCube(healthPos, healthSize);
        }

        // 顯示血量點數文字
        UnityEditor.Handles.Label(healthBarPos + Vector3.up * 0.5f, $"HP: {currentHealth}/{maxHealth}");
    }
}
