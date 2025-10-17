using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float damage = 1f;           // �T�w�ˮ`�ȡ]1�I��^
    [SerializeField] private float lifetime = 5f;         // �s���ɶ�
    [SerializeField] private LayerMask hitLayers = -1;    // �i�H�������h��

    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;        // �����S��
    [SerializeField] private AudioClip hitSound;          // ��������
    [SerializeField] private float hitEffectLifetime = 2f; // �S�Ħs���ɶ�

    // �ե�ޥ�
    private Rigidbody rb;
    private Collider bulletCollider;

    // �l�u���A
    private bool hasHit = false;
    private float spawnTime;

    // �l�u�o�g�̡]�קK�۶ˡ^
    private GameObject shooter;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bulletCollider = GetComponent<Collider>();
        spawnTime = Time.time;

        // �p�G�S��Rigidbody�A�K�[�@��
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false; // �l�u�������O�v�T
        }

        // �p�G�S��Collider�A�K�[�@�Ӳy�θI����
        if (bulletCollider == null)
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 0.05f;
            sphereCollider.isTrigger = true;
        }
    }

    void Start()
    {
        // �]�m�P���ɶ�
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // �ˬd�O�_�W�L�s���ɶ�
        if (Time.time - spawnTime >= lifetime)
        {
            DestroyBullet();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // �קK����Ĳ�o
        if (hasHit) return;

        // �����o�g��
        if (other.gameObject == shooter) return;

        // �ˬd�O�_�b�i�������h�Ť�
        if (((1 << other.gameObject.layer) & hitLayers) == 0) return;

        // �B�z����
        HandleHit(other);
    }

    private void HandleHit(Collider hitTarget)
    {
        hasHit = true;

        // ���������m
        Vector3 hitPoint = transform.position;
        Vector3 hitNormal = -transform.forward; // ���]�l�u�«e����

        // ���չ�ؼгy���ˮ`
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

        // �Ы������S��
        CreateHitEffect(hitPoint, hitNormal);

        // ������������
        PlayHitSound(hitPoint);

        // �P���l�u
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
            // �b������m����3D����
            AudioSource.PlayClipAtPoint(hitSound, position);
        }
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }

    // ���@��k�G�]�m�l�u�Ѽ�
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

    // �����Ϊ�Gizmos
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

// �ˮ`�����]��L�}���i�H��{�o�Ӥ����ӱ����ˮ`�^
public interface IDamageable
{
    void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker);
}