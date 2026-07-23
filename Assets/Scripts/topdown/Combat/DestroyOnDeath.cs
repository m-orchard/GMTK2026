using UnityEngine;

[RequireComponent(typeof(Health))]
public class DestroyOnDeath : MonoBehaviour
{
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        health.OnDied -= HandleDeath;
    }

    private void HandleDeath()
    {
        Destroy(gameObject);
    }
}
