using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float speed = 15f;

    [Header("Lifetime")]
    [Tooltip("Seconds before the bullet despawns if it hasn't hit anything.")]
    [SerializeField, Min(0f)] private float maxLifetime = 3f;

    [Header("Damage")]
    [SerializeField, Min(0f)] private float damage = 1f;

    [Tooltip("Which layers this bullet is allowed to hit.")]
    [SerializeField] private LayerMask hittableLayers;

    [Header("Knockback")]
    [Tooltip("How hard the bullet shoves what it hits, along its travel direction.")]
    [SerializeField, Min(0f)] private float knockbackForce = 3f;

    private Rigidbody2D bulletRigidbody;
    private float despawnTime;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody2D>();
        despawnTime = Time.time + maxLifetime;
    }

    public void Launch(Vector2 direction)
    {
        bulletRigidbody.linearVelocity = direction.normalized * speed;
        despawnTime = Time.time + maxLifetime;
    }

    private void Update()
    {
        if (Time.time >= despawnTime)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsHittable(other))
        {
            return;
        }

        ApplyDamageIfPossible(other);
        ApplyKnockbackIfPossible(other);
        Despawn();
    }

    private bool IsHittable(Collider2D other)
    {
        return (hittableLayers.value & (1 << other.gameObject.layer)) != 0;
    }

    private void ApplyDamageIfPossible(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
        }
    }

    private void ApplyKnockbackIfPossible(Collider2D other)
    {
        if (knockbackForce <= 0f)
        {
            return;
        }

        if (other.TryGetComponent(out IKnockbackable knockbackable))
        {
            knockbackable.ApplyKnockback(bulletRigidbody.linearVelocity, knockbackForce);
        }
    }

    private void Despawn()
    {
        Destroy(gameObject);
    }
}
