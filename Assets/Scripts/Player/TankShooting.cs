using UnityEngine;

public class TankShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;     // �l�u�w�s��
    [SerializeField] private float bulletSpeed = 20f;     // �l�u�t��
    [SerializeField] private float fireRate = 1f;         // �g���W�v�]�C���X�o�^
    [SerializeField] private float bulletLifetime = 5f;   // �l�u�s���ɶ�

    [Header("Audio & Effects")]
    [SerializeField] private AudioClip shootSound;        // �g������
    [SerializeField] private ParticleSystem muzzleFlash;  // �j�f���K�S��

    // �ե�ޥ�
    private TankController tankController;
    private AudioSource audioSource;

    // �g������
    private float nextFireTime = 0f;

    void Awake()
    {
        tankController = GetComponent<TankController>();
        audioSource = GetComponent<AudioSource>();

        // �p�G�S��AudioSource�ե�A�K�[�@��
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        HandleShooting();
    }

    private void HandleShooting()
    {
        // �ˬd�O�_�i�H�g��
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
        // �]�m�U���g���ɶ�
        nextFireTime = Time.time + (1f / fireRate);

        // ����o�g��m�M��V
        Vector3 firePosition = tankController.GetFirePointPosition();
        Vector3 fireDirection = tankController.GetFireDirection();

        // �Ыؤl�u
        GameObject bullet = Instantiate(bulletPrefab, firePosition, Quaternion.LookRotation(fireDirection));

        // �]�m�l�u�t��
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = fireDirection * bulletSpeed;
        }

        // �]�m�l�u�s���ɶ�
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetLifetime(bulletLifetime);
        }
        else
        {
            // �p�G�S��Bullet�}���A�ϥ�Destroy�@���Ʈ�
            Destroy(bullet, bulletLifetime);
        }

        // ���񭵮�
        PlayShootSound();

        // ����j�f���K�S��
        PlayMuzzleFlash();

        // ������X
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

    // ���@��k�G�]�m�l�u�w�s��]�Ω󤣦P�������Z���^
    public void SetBulletPrefab(GameObject newBulletPrefab)
    {
        bulletPrefab = newBulletPrefab;
    }

    // ���@��k�G�]�m�g���W�v
    public void SetFireRate(float newFireRate)
    {
        fireRate = newFireRate;
    }

    // ���@��k�G�j��m�g���N�o
    public void ResetFireCooldown()
    {
        nextFireTime = 0f;
    }
}