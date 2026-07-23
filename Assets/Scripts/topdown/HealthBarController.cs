using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [Tooltip("The health to display. Can also be assigned at runtime via Initialize.")]
    [SerializeField] private Health health;

    [SerializeField] private Image healthBar;
    [SerializeField] private Image shieldBar;
    [SerializeField] private Image damageBar;
    [SerializeField] private Image healableHealthBar;

    private const float DamageBarShrinkDuration = 0.5f;

    private float damageBarShrinkTimer;

    public void Initialize(Health healthToTrack)
    {
        Unsubscribe();
        health = healthToTrack;
        Subscribe();
        Refresh();
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        if (damageBar == null)
        {
            return;
        }

        if (damageBarShrinkTimer < 0f)
        {
            UpdateBarToValue(damageBar, healthBar.fillAmount);
        }
        else
        {
            damageBarShrinkTimer -= Time.deltaTime;
        }
    }

    public void SetHealthColor(Color color)
    {
        healthBar.color = color;
    }

    private void Subscribe()
    {
        if (health != null)
        {
            health.OnChanged += Refresh;
        }
    }

    private void Unsubscribe()
    {
        if (health != null)
        {
            health.OnChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        if (health != null)
        {
            SetValues(health.CurrentHealth, health.MaxHealth);
        }
    }

    private void SetValues(float current, float max)
    {
        float shield = CurrentShield;
        float healableHealth = max;
        float totalCapacity = max + shield;
        if (totalCapacity <= 0f)
        {
            return;
        }

        healthBar.fillAmount = current / totalCapacity;
        SetFill(shieldBar, (current + shield) / totalCapacity);
        SetFill(healableHealthBar, healableHealth / totalCapacity);

        damageBarShrinkTimer = DamageBarShrinkDuration;
    }

    // Shields are not implemented yet. Returning 0 keeps the bar maths valid so shields can be re-enabled here later.
    private float CurrentShield => 0f;

    private static void SetFill(Image bar, float amount)
    {
        if (bar != null)
        {
            bar.fillAmount = amount;
        }
    }

    private static void UpdateBarToValue(Image bar, float targetAmount)
    {
        if (Mathf.Approximately(targetAmount, bar.fillAmount))
        {
            return;
        }

        bar.fillAmount = Mathf.Lerp(bar.fillAmount, targetAmount, 5f * Time.deltaTime);

        float distance = Mathf.Abs(targetAmount - bar.fillAmount);
        if (distance < 0.005f)
        {
            bar.fillAmount = targetAmount;
        }
    }
}
