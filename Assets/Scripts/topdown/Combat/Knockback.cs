using UnityEngine;

public class Knockback : MonoBehaviour, IKnockbackable
{
    [Tooltip("How quickly the push fades. Higher recovers to normal movement faster.")]
    [SerializeField, Min(0f)] private float recoverySpeed = 20f;

    private Vector2 velocity;

    public void ApplyKnockback(Vector2 direction, float force)
    {
        velocity += direction.normalized * force;
    }

    private void LateUpdate()
    {
        if (velocity.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.position += (Vector3)(velocity * Time.deltaTime);
        velocity = Vector2.MoveTowards(velocity, Vector2.zero, recoverySpeed * Time.deltaTime);
    }
}
