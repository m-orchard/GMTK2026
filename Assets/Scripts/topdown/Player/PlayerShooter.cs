using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerShooter : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private Bullet bulletPrefab;

    [Tooltip("Where bullets spawn from. Falls back to this object's transform if unset.")]
    [SerializeField] private Transform firePoint;

    [Header("Firing")]
    [Tooltip("Shots fired per second.")]
    [SerializeField, Min(0f)] private float fireRate = 5f;

    [Tooltip("If true, holding fire keeps shooting. If false, each shot needs a fresh press.")]
    [SerializeField] private bool automatic = true;

    private float nextFireTime;

    private void Update()
    {
        if (IsFireInputActive() && CanFire())
        {
            Fire();
        }
    }

    private bool CanFire()
    {
        return bulletPrefab != null && Time.time >= nextFireTime;
    }

    private void Fire()
    {
        Transform spawnPoint = firePoint != null ? firePoint : transform;
        Bullet bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
        bullet.Launch(spawnPoint.up);
        nextFireTime = Time.time + SecondsBetweenShots();
    }

    private float SecondsBetweenShots()
    {
        return fireRate > 0f ? 1f / fireRate : 0f;
    }

    private bool IsFireInputActive()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return automatic
                ? Mouse.current.leftButton.isPressed
                : Mouse.current.leftButton.wasPressedThisFrame;
        }
#endif
        return automatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
    }
}
