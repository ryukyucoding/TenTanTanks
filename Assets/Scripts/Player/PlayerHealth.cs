using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;         // �̤j��q�I��
    [SerializeField] private int currentHealth;         // ��e��q�I��

    [Header("Damage Effects")]
    [SerializeField] private float invulnerabilityTime = 1f; // �L�Įɶ�
    [SerializeField] private GameObject damageEffect;        // ���˯S��
    [SerializeField] private AudioClip damageSound;          // ���˭���
    [SerializeField] private AudioClip deathSound;           // ���`����

    [Header("Visual Feedback")]
    [SerializeField] private Renderer[] tankRenderers;      // �Z�J����V���]�Ω�{�{�ĪG�^
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    // �ե�ޥ�
    private AudioSource audioSource;

    // ���A�ܼ�
    private bool isInvulnerable = false;
    private float lastDamageTime;
    private Color[] originalColors;

    // �ƥ�
    public System.Action<int, int> OnHealthChanged; // ��e��q, �̤j��q
    public System.Action OnPlayerDeath;

    // ���@�ݩ�
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
        // �q��UI��l��q
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
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

        Debug.Log($"Player took 1 damage. Health: {currentHealth}/{maxHealth}");

        // Ĳ�o��ı�M����
        PlayDamageEffects(hitPoint);

        // �q���t�Φ�q�ܤ�
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // �q��GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDamaged(currentHealth, maxHealth);
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
        Debug.Log("Player died!");

        // ���񦺤`����
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Ĳ�o���`�ƥ�
        OnPlayerDeath?.Invoke();

        // �T�α���
        TankController controller = GetComponent<TankController>();
        if (controller != null)
            controller.enabled = false;

        TankShooting shooting = GetComponent<TankShooting>();
        if (shooting != null)
            shooting.enabled = false;

        // �i�H�b�o�̲K�[���`�ʵe�ίS��

        // ����@�q�ɶ���q��GameManager
        Invoke(nameof(NotifyGameManager), 1f);
    }

    private void NotifyGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver("���a�Q���ѡI");
        }
    }

    // ���@��k�G�v��
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

    // ���@��k�G�]�m�̤j��q
    public void SetMaxHealth(int newMaxHealth)
    {
        float healthPercentage = HealthPercentage;
        maxHealth = newMaxHealth;
        currentHealth = Mathf.RoundToInt(maxHealth * healthPercentage);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ���@��k�G�����v��
    public void FullHeal()
    {
        Heal(maxHealth);
    }

    // �����Ϊ�Gizmos
    void OnDrawGizmosSelected()
    {
        // ��ܦ�q��
        Vector3 healthBarPos = transform.position + Vector3.up * 3f;
        Vector3 healthBarSize = new Vector3(2f, 0.2f, 0f);

        // �I���]����^
        Gizmos.color = Color.red;
        Gizmos.DrawCube(healthBarPos, healthBarSize);

        // ��q�]���^
        if (IsAlive)
        {
            Gizmos.color = Color.green;
            Vector3 healthSize = healthBarSize;
            healthSize.x *= HealthPercentage;
            Vector3 healthPos = healthBarPos;
            healthPos.x -= (healthBarSize.x - healthSize.x) * 0.5f;
            Gizmos.DrawCube(healthPos, healthSize);
        }

        // ��ܦ�q�I�Ƥ�r
        UnityEditor.Handles.Label(healthBarPos + Vector3.up * 0.5f, $"HP: {currentHealth}/{maxHealth}");
    }
}
