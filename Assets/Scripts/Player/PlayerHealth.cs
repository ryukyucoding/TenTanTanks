using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int initialHealth = 3;     // 初始生命值
    private int currentHealth;                          // 當前生命值

    [Header("Damage Effects")]
    [SerializeField] private float invulnerabilityTime = 1f; // �L�Įɶ�
    [SerializeField] private GameObject damageEffect;        // ���˯S��
    [SerializeField] private AudioClip damageSound;          // ���˭���
    [SerializeField] private AudioClip deathSound;           // ���`����

    [Header("Visual Feedback")]
    [SerializeField] private Renderer[] tankRenderers;      // �Z�J����V���]�Ω�{�{�ĪG�^
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Death Effects")]
    [SerializeField] private GameObject explosionEffect;  // Explosion particle prefab
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float explosionDuration = 2f; // How long effect lasts

    // �ե�ޥ�
    private AudioSource audioSource;

    // ���A�ܼ�
    private bool isInvulnerable = false;
    private float lastDamageTime;
    private Color[] originalColors;
    private bool isDead = false;


    // �ƥ�
    public System.Action<int, int> OnHealthChanged; // ���e��q, �̤j��q
    public System.Action OnPlayerDeath;

    // 公開屬性
    public int CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 先設置為初始值
        currentHealth = initialHealth;

        // �O�s��l�C��
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
        // 嘗試從 PlayerDataManager 載入生命值
        if (PlayerDataManager.Instance != null)
        {
            bool loaded = PlayerDataManager.Instance.LoadPlayerHealth(this);
            if (!loaded)
            {
                // 如果沒有保存的數據，初始化為 initialHealth
                currentHealth = initialHealth;
                // 保存初始生命值
                PlayerDataManager.Instance.SavePlayerHealth(currentHealth);
            }
        }
        else
        {
            currentHealth = initialHealth;
            Debug.LogWarning("[PlayerHealth] PlayerDataManager 不存在，使用預設生命值");
        }

        // 通知UI初始生命
        OnHealthChanged?.Invoke(currentHealth, currentHealth);
    }

    void OnDestroy()
    {
        // 註：不在這裡保存生命值，因為場景切換時會觸發 OnDestroy
        // 生命值的保存在 GameManager.Victory() 中處理
    }

    void Update()
    {
        // �ˬd�L�Įɶ�
        if (isInvulnerable && Time.time - lastDamageTime >= invulnerabilityTime)
        {
            isInvulnerable = false;
            RestoreOriginalColors();
        }
    }

    // ��{IDamageable����
    public void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker)
    {
        // �ˬd�O�_�B��L�Ī��A
        if (isInvulnerable || !IsAlive) return;

        // ��֦�q�]�C��������1�I��^
        currentHealth = Mathf.Max(0, currentHealth - 1);
        lastDamageTime = Time.time;
        isInvulnerable = true;

        Debug.Log($"Player took 1 damage. Health: {currentHealth}");

        // 播放受傷特效和音效
        PlayDamageEffects(hitPoint);

        // 通知系統生命變化
        OnHealthChanged?.Invoke(currentHealth, currentHealth);

        // 通知GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDamaged(currentHealth, currentHealth);
        }

        // �ˬd�O�_���`
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void PlayDamageEffects(Vector3 hitPoint)
    {
        // ������˭���
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        // �Ыب��˯S��
        if (damageEffect != null)
        {
            GameObject effect = Instantiate(damageEffect, hitPoint, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // �{�{�ĪG
        StartFlashEffect();
    }

    private void StartFlashEffect()
    {
        if (tankRenderers == null) return;

        // �����C�⬰�����C��
        for (int i = 0; i < tankRenderers.Length; i++)
        {
            if (tankRenderers[i] != null && tankRenderers[i].material != null)
            {
                tankRenderers[i].material.color = damageFlashColor;
            }
        }

        // �u�ɶ����_���
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
        if (isDead) return;
        isDead = true;

        Debug.Log("Player tank destroyed!");

        // Play death/explosion sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        // Create explosion visual effect
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);

            // Auto-destroy the explosion effect after duration
            if (explosionDuration > 0)
            {
                Destroy(explosion, explosionDuration);
            }
        }

        // Disable player controls
        var tankController = GetComponent<TankController>();
        if (tankController != null)
            tankController.enabled = false;

        var tankShooting = GetComponent<TankShooting>();
        if (tankShooting != null)
            tankShooting.enabled = false;

        // Stop movement
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Hide the tank immediately (so explosion shows without tank)
        if (tankRenderers != null)
        {
            foreach (Renderer renderer in tankRenderers)
            {
                if (renderer != null)
                    renderer.enabled = false;
            }
        }

        // Destroy the player tank immediately
        Destroy(gameObject);
    }

    private void NotifyGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver("���a�Q���ѡI");
        }
    }

    // 公開方法：治療
    public void Heal(int healAmount = 1)
    {
        if (!IsAlive) return;

        int oldHealth = currentHealth;
        currentHealth += healAmount;

        if (currentHealth != oldHealth)
        {
            Debug.Log($"Player healed for {currentHealth - oldHealth}. Health: {currentHealth}");
            OnHealthChanged?.Invoke(currentHealth, currentHealth);
        }
    }

    /// <summary>
    /// 直接設置生命值（不觸發事件，用於載入保存的數據）
    /// </summary>
    public void SetHealthDirect(int health)
    {
        currentHealth = Mathf.Max(0, health);
        Debug.Log($"直接設置生命值: {currentHealth}");
    }

    // �����Ϊ�Gizmos
    void OnDrawGizmosSelected()
    {
        // 顯示生命條
        Vector3 healthBarPos = transform.position + Vector3.up * 3f;

        // 顯示生命數字
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(healthBarPos + Vector3.up * 0.5f, $"HP: {currentHealth}");
        #endif
    }
}
