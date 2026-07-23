using UnityEngine;

[RequireComponent(typeof(ResourcePickup))]
public class ResourcePickupMagnet : MonoBehaviour
{
    [Tooltip("The player must be within this range for the pickup to start homing in.")]
    [SerializeField, Min(0f)] private float attractRadius = 3f;

    [Tooltip("Top speed the pickup flies toward the player at.")]
    [SerializeField, Min(0f)] private float attractSpeed = 8f;

    [Tooltip("How quickly the pickup ramps up to its attract speed.")]
    [SerializeField, Min(0f)] private float acceleration = 20f;

    private Transform target;
    private float currentSpeed;

    private void Start()
    {
        ResourceCollector collector = FindFirstObjectByType<ResourceCollector>();
        if (collector != null)
        {
            target = collector.transform;
        }
    }

    private void Update()
    {
        if (target == null || !PlayerInRange())
        {
            currentSpeed = 0f;
            return;
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, attractSpeed, acceleration * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, target.position, currentSpeed * Time.deltaTime);
    }

    private bool PlayerInRange()
    {
        return Vector2.Distance(transform.position, target.position) <= attractRadius;
    }
}
