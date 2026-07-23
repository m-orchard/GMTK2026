using UnityEngine;

[DisallowMultipleComponent]
public class Knockback : MonoBehaviour, IKnockbackable
{
    [Tooltip("How quickly the push fades. Higher recovers to normal movement faster.")]
    [SerializeField, Min(0f)] private float recoverySpeed = 20f;

    private Rigidbody2D body;
    private Vector2 velocity;

    public Vector2 Velocity => velocity;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        velocity += direction.normalized * force;
    }

    public void Decay(float deltaTime)
    {
        velocity = Vector2.MoveTowards(velocity, Vector2.zero, recoverySpeed * deltaTime);
    }

    private void LateUpdate()
    {
        if (body != null || velocity.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.position += (Vector3)(velocity * Time.deltaTime);
        Decay(Time.deltaTime);
    }
}
