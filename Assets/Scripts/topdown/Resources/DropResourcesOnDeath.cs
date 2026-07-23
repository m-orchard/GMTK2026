using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ResourceCarrier))]
public class DropResourcesOnDeath : MonoBehaviour
{
    [SerializeField] private ResourcePickup pickupPrefab;

    [Tooltip("How far the dropped pickups scatter from the death point.")]
    [SerializeField, Min(0f)] private float scatterRadius = 0.75f;

    private Health health;
    private ResourceCarrier resourceCarrier;

    private void Awake()
    {
        health = GetComponent<Health>();
        resourceCarrier = GetComponent<ResourceCarrier>();
    }

    private void OnEnable()
    {
        health.OnDied += DropResources;
    }

    private void OnDisable()
    {
        health.OnDied -= DropResources;
    }

    private void DropResources()
    {
        if (pickupPrefab == null)
        {
            return;
        }

        for (int i = 0; i < resourceCarrier.CarriedAmount; i++)
        {
            SpawnPickup();
        }
    }

    private void SpawnPickup()
    {
        ResourcePickup pickup = Instantiate(pickupPrefab, ScatteredDropPosition(), Quaternion.identity);
        pickup.SetAmount(1);
    }

    private Vector3 ScatteredDropPosition()
    {
        Vector2 offset = Random.insideUnitCircle * scatterRadius;
        return transform.position + new Vector3(offset.x, offset.y, 0f);
    }
}
