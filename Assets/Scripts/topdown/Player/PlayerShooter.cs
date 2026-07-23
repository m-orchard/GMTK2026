using UnityEngine;

#if ENABLE_INPUT_SYSTEM

using UnityEngine.InputSystem;

#endif

public class PlayerShooter : MonoBehaviour {

    [Header("Bullet")]
    [SerializeField] private Bullet bulletPrefab;

    [Tooltip("Where bullets spawn from. Falls back to this object's transform if unset.")]
    [SerializeField] private Transform firePoint;

    [Header("Firing")]
    [Tooltip("Shots fired per second.")]
    [SerializeField, Min(0f)] private float fireRate = 5f;

    [Tooltip("If true, holding fire keeps shooting. If false, each shot needs a fresh press.")]
    [SerializeField] private bool automatic = true;

    [Header("Recoil")]
    [Tooltip("How hard each shot shoves the player backwards. Eventually driven by the equipped gun.")]
    [SerializeField, Min(0f)] private float recoilForce = 1.5f;

    [SerializeField] private AudioClip fireSfx;

    private float nextFireTime;
    private Knockback recoil;

    private void Awake() {
        recoil = GetComponentInParent<Knockback>();
    }

    private void Update() {
        if (IsFireInputActive() && CanFire()) {
            Fire();
        }
    }

    private bool CanFire() {
        return bulletPrefab != null && Time.time >= nextFireTime;
    }

    private void Fire() {
        Transform spawnPoint = firePoint != null ? firePoint : transform;
        Vector2 fireDirection = spawnPoint.up;
        Bullet bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
        bullet.Launch(fireDirection);
        ApplyRecoil(fireDirection);
        nextFireTime = Time.time + SecondsBetweenShots();
        AudioManager.Instance.PlaySound(fireSfx);
    }

    private void ApplyRecoil(Vector2 fireDirection) {
        if (recoil == null || recoilForce <= 0f) {
            return;
        }

        recoil.ApplyKnockback(-fireDirection, recoilForce);
    }

    private float SecondsBetweenShots() {
        return fireRate > 0f ? 1f / fireRate : 0f;
    }

    private bool IsFireInputActive() {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) {
            return automatic
                ? Mouse.current.leftButton.isPressed
                : Mouse.current.leftButton.wasPressedThisFrame;
        }
#endif
        return automatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
    }
}