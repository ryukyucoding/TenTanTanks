using UnityEngine;

public class Mine : MonoBehaviour
{
    [Header("Mine Settings")]
    [SerializeField] private float explodeRadius = 2f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float activationDelay = 0.5f;
    [SerializeField] private LayerMask targetLayers = -1;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float warningPulseSpeed = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    // Mine state
    private bool isActivated = false;
    private bool isExploded = false;
    private float activationTimer = 0f;
    private GameObject owner;
    private int team = 0;
    
    // Visual components
    private Renderer mineRenderer;
    private Color originalColor;
    
    void Awake()
    {
        mineRenderer = GetComponent<Renderer>();
        if (mineRenderer != null)
        {
            originalColor = mineRenderer.material.color;
        }
    }
    
    void Start()
    {
        // 設置激活延遲
        activationTimer = activationDelay;
    }
    
    void Update()
    {
        if (isExploded) return;
        
        // 檢查激活延遲
        if (!isActivated)
        {
            activationTimer -= Time.deltaTime;
            if (activationTimer <= 0f)
            {
                isActivated = true;
            }
        }
        
        // 視覺警告效果
        if (isActivated && mineRenderer != null)
        {
            float pulse = Mathf.Sin(Time.time * warningPulseSpeed) * 0.5f + 0.5f;
            Color currentColor = Color.Lerp(originalColor, warningColor, pulse);
            mineRenderer.material.color = currentColor;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isExploded || !isActivated) return;
        
        // 檢查是否為目標
        if (IsValidTarget(other))
        {
            // 檢查是否為地雷擁有者
            if (other.gameObject == owner) return;
            
            // 檢查團隊（如果有的話）
            if (team != 0 && other.GetComponent<TeamComponent>() != null)
            {
                TeamComponent targetTeam = other.GetComponent<TeamComponent>();
                if (targetTeam.team == team) return; // 同隊不爆炸
            }
            
            // 激活爆炸
            Explode();
        }
    }
    
    private bool IsValidTarget(Collider target)
    {
        // 檢查Layer
        if (((1 << target.gameObject.layer) & targetLayers) == 0) return false;
        
        // 檢查距離
        float distance = Vector3.Distance(transform.position, target.transform.position);
        return distance <= explodeRadius;
    }
    
    private void Explode()
    {
        if (isExploded) return;
        
        isExploded = true;
        
        // 播放爆炸音效
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }
        
        // 創建爆炸特效
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        // 對範圍內的所有目標造成傷害
        Collider[] targets = Physics.OverlapSphere(transform.position, explodeRadius, targetLayers);
        
        foreach (Collider target in targets)
        {
            if (target.gameObject == owner) continue; // 不傷害擁有者
            
            // 檢查團隊
            if (team != 0 && target.GetComponent<TeamComponent>() != null)
            {
                TeamComponent targetTeam = target.GetComponent<TeamComponent>();
                if (targetTeam.team == team) continue; // 不傷害同隊
            }
            
            // 造成傷害
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, transform.position, owner);
            }
        }
        
        // 銷毀地雷
        Destroy(gameObject, 0.1f);
    }
    
    public void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
    }
    
    public void SetTeam(int newTeam)
    {
        team = newTeam;
    }
    
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    public void SetExplodeRadius(float newRadius)
    {
        explodeRadius = newRadius;
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // 繪製爆炸範圍
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRadius);
        
        // 繪製激活狀態
        Gizmos.color = isActivated ? Color.yellow : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    
    void OnDrawGizmosSelected()
    {
        // 繪製詳細信息
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
