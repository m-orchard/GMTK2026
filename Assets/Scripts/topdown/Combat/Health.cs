using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1f)] private float maxHealth = 3f;

    private float currentHealth;

    public event Action<float> OnDamaged;
    public event Action<float> OnHealed;
    public event Action OnChanged;
    public event Action OnDied;
    public event Action OnRevived;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;
    public bool IsFull => currentHealth >= maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnDamaged?.Invoke(amount);
        OnChanged?.Invoke();

        if (IsDead)
        {
            OnDied?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (IsFull || amount <= 0f)
        {
            return;
        }

        bool wasDead = IsDead;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealed?.Invoke(amount);
        OnChanged?.Invoke();

        if (wasDead)
        {
            OnRevived?.Invoke();
        }
    }
}
