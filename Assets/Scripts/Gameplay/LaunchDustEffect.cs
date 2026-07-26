using UnityEngine;

public class LaunchDustEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustParticles;
    [SerializeField] private float dustFadeOutHeight = 6f;

    private ParticleSystem.EmissionModule emission;
    private float firingEmissionRate;

    private void Awake()
    {
        emission = dustParticles.emission;
        firingEmissionRate = emission.rateOverTime.constant;
        dustParticles.Play();
    }

    private void Update()
    {
        emission.rateOverTime = AnyEngineFiringNearGround() ? firingEmissionRate : 0f;
    }

    private bool AnyEngineFiringNearGround()
    {
        if (RocketAssembly.Instance == null)
        {
            return false;
        }

        float padHeight = RocketAssembly.Instance.PadY;

        foreach (var engineGroup in RocketAssembly.Instance.Rocket.EngineGroups)
        {
            foreach (EngineThrustEffect engine in engineGroup)
            {
                if (engine.IsFiring && engine.transform.position.y - padHeight < dustFadeOutHeight)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
