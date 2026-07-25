using UnityEngine;
using UnityEngine.Events;

public class EngineThrustEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem thrustParticles;
    [SerializeField] public float Thrust = 20f;
    [SerializeField] public int Group = 1;
    [SerializeField] public int PhasePriority = 1;
    [SerializeField] private AudioClip thrusterFireSound;

    public UnityEvent OnFiringStart;

    public UnityEvent OnFiringEnd;

    private bool isFiring;

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

    private void PlayThrusterFireSound()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlaySound(thrusterFireSound, transform);
    }
}
