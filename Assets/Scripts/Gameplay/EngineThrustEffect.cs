using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class EngineThrustEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem thrustParticles;
    [SerializeField] private ParticleSystem puffParticles;
    [SerializeField] public float Thrust = 20f;
    [SerializeField] public int Group = 1;
    [SerializeField] public int PhasePriority = 1;
    [SerializeField] private AudioClip thrusterFireSound;

    [Header("Empowered")]
    [SerializeField] private Color empoweredThrustColor = new Color(0.4f, 1f, 0.3f, 1f);
    [SerializeField] private float empoweredThrustSizeMultiplier = 1.4f;
    [SerializeField] private SpriteRenderer engineSprite;
    [SerializeField] private ParticleSystem empoweredAura;
    [SerializeField] private Color empoweredAuraColor = new Color(0.5f, 1f, 0.4f, 1f);
    [SerializeField] private float auraPulseDuration = 0.8f;

    public UnityEvent OnFiringStart;

    public UnityEvent OnFiringEnd;

    public bool Empowered { get; private set; }

    public bool IsFiring => isFiring;

    private bool isFiring;
    private Tween auraPulseTween;

    private void Awake()
    {
        thrustParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void SetFiring(bool firing)
    {
        var wasFiring = isFiring;
        isFiring = firing;

        if (firing)
        {
            thrustParticles.Play();
        }
        else
        {
            thrustParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (!wasFiring && firing)
        {
            PlayThrusterFireSound();
            OnFiringStart?.Invoke();
        }
        else if (wasFiring && !firing)
        {
            OnFiringEnd?.Invoke();
        }
    }

    public void Empower()
    {
        if (Empowered)
        {
            return;
        }

        Empowered = true;
        ApplyEmpoweredThrustAppearance();
        StartEmpoweredAura();
    }

    private void ApplyEmpoweredThrustAppearance()
    {
        var main = thrustParticles.main;
        main.startColor = empoweredThrustColor;
        main.startSizeMultiplier *= empoweredThrustSizeMultiplier;
    }

    private void StartEmpoweredAura()
    {
        if (empoweredAura != null)
        {
            empoweredAura.Play();
        }

        if (engineSprite != null)
        {
            auraPulseTween = engineSprite
                .DOColor(empoweredAuraColor, auraPulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void EmitPuff()
    {
        puffParticles.Play();
    }

    private void PlayThrusterFireSound()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlaySound(thrusterFireSound, transform);
    }

    private void OnDestroy()
    {
        auraPulseTween?.Kill();
    }
}
