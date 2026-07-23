using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 3f;

    [Tooltip("The child graphics to rotate when facing. The root is left unrotated so child UI stays upright.")]
    [SerializeField] private Transform graphics;

    [Tooltip("Degrees to offset the facing rotation. Use -90 when the sprite's 'forward' points up.")]
    [SerializeField] private float rotationOffset = -90f;

    private void Awake()
    {
        if (graphics == null)
        {
            graphics = transform;
        }
    }

    public void MoveTowards(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        FaceDirection(direction);
    }

    public float DistanceTo(Vector3 target)
    {
        return Vector2.Distance(transform.position, target);
    }

    private void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
        graphics.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
