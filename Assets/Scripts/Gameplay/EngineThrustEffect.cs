using UnityEngine;
using UnityEngine.Events;

public class EngineThrustEffect : MonoBehaviour
{
    [SerializeField] private Color flameColor = new Color(1f, 0.55f, 0.1f);
    [SerializeField] private float particleLifetime = 0.35f;
    [SerializeField] private float particleSpeed = 4f;
    [SerializeField] private float particleSize = 0.2f;
    [SerializeField] public float Thrust = 20f;
    [SerializeField] private AudioClip thrusterFireSound;
    [SerializeField] private Color poweredColor = new Color(0.2f, 1f, 0.3f, 1f);
    [SerializeField] private Color unpoweredColor = new Color(0.6f, 0.1f, 0.1f, 1f);

    public UnityEvent OnFiringStart;

    public UnityEvent OnFiringEnd;

    private ParticleSystem.EmissionModule emission;
    private SpriteRenderer powerIndicator;
    private bool? powered;

    private void Awake()
    {
        var system = BuildParticleSystem();
        emission = system.emission;
        emission.enabled = false;
        system.Play();

        powerIndicator = BuildPowerIndicator();
        SetPowered(false);
    }

    public void SetPowered(bool isPowered)
    {
        if (powered == isPowered) return;
        powered = isPowered;
        if (powerIndicator != null) powerIndicator.color = isPowered ? poweredColor : unpoweredColor;
    }

    private SpriteRenderer BuildPowerIndicator()
    {
        var indicatorObject = new GameObject("PowerIndicator");
        indicatorObject.transform.SetParent(transform, false);
        indicatorObject.transform.localPosition = new Vector3(0f, 0.35f, -0.1f);
        indicatorObject.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        var sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);

        var renderer = indicatorObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 11;
        return renderer;
    }

    public void SetFiring(bool firing)
    {
        var wasFiring = emission.enabled;
        emission.enabled = firing;

        if (!wasFiring && firing)
        {
            PlayThrusterFireSound();
            OnFiringStart?.Invoke();
        } else if (wasFiring && !firing)
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

    private static Material sharedMaterial;

    private ParticleSystem BuildParticleSystem()
    {
        var system = gameObject.AddComponent<ParticleSystem>();

        var renderer = system.GetComponent<ParticleSystemRenderer>();
        renderer.material = GetParticleMaterial();
        renderer.sortingOrder = 10;

        var main = system.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = particleLifetime;
        main.startSpeed = 0f;
        main.startSize = particleSize;
        main.startColor = flameColor;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.gravityModifier = 0f;

        var shape = system.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.05f;

        var velocityOverLifetime = system.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-particleSpeed * 0.7f, -particleSpeed);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var colorOverLifetime = system.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(flameColor, 0f), new GradientColorKey(flameColor, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;

        var emissionModule = system.emission;
        emissionModule.rateOverTime = 40f;

        return system;
    }

    private static Material GetParticleMaterial()
    {
        if (sharedMaterial != null) return sharedMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                     ?? Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Sprites/Default")
                     ?? Shader.Find("Particles/Standard Unlit");

        if (shader == null)
        {
            Debug.LogError("[EngineThrustEffect] No compatible particle shader found - particles will not render.");
            return null;
        }

        Debug.Log($"[EngineThrustEffect] Using shader: {shader.name}");
        sharedMaterial = new Material(shader);
        return sharedMaterial;
    }
}
