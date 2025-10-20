using UnityEngine;

public class TankShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;     // ?????
    [SerializeField] private float bulletSpeed = 20f;     // ????
    [SerializeField] private float fireRate = 1f;         // ??????????
    [SerializeField] private float bulletLifetime = 5f;   // ??????

    [Header("Audio & Effects")]
    [SerializeField] private AudioClip shootSound;        // ????
    [SerializeField] private ParticleSystem muzzleFlash;  // ??????

    // ????
    private TankController tankController;
    private AudioSource audioSource;

    // ????
    private float nextFireTime = 0f;

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
        HandleShooting();
    }

    private void HandleShooting()
    {
        // ????????
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