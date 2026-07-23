using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class ShipModule : MonoBehaviour
{
    private Health health;

    public event Action OnBroken;
    public event Action OnRestored;

    public Health Health => health;
    public bool IsBroken => health.IsDead;
    public bool IsOperational => !health.IsDead;
    public bool IsFullyRepaired => health.IsFull;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.OnDied += HandleBroken;
        health.OnRevived += HandleRestored;
    }

    private void OnDisable()
    {
        health.OnDied -= HandleBroken;
        health.OnRevived -= HandleRestored;
    }

    public void Repair(float amount)
    {
        health.Heal(amount);
    }

    private void HandleBroken()
    {
        OnBroken?.Invoke();
    }

    private void HandleRestored()
    {
        OnRestored?.Invoke();
    }
}
