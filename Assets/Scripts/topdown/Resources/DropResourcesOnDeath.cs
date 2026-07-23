using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ResourceCarrier))]
public class DropResourcesOnDeath : MonoBehaviour
{
    [SerializeField] private ResourcePickup pickupPrefab;

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
        if (pickupPrefab == null || !resourceCarrier.HasResources)
        {
            return;
        }

        ResourcePickup pickup = Instantiate(pickupPrefab, transform.position, Quaternion.identity);
        pickup.SetAmount(resourceCarrier.CarriedAmount);
    }
}
