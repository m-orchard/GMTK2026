using UnityEngine;

public class ResourceCollector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out ResourcePickup pickup))
        {
            Collect(pickup);
        }
    }

    private void Collect(ResourcePickup pickup)
    {
        if (ResourceBank.Instance != null)
        {
            ResourceBank.Instance.Add(pickup.Amount);
        }

        Destroy(pickup.gameObject);
    }
}
