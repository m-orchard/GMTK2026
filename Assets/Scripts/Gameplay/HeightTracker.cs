using UnityEngine;

public class HeightTracker : Singleton<HeightTracker>
{
    [SerializeField] private RocketAssembly rocket;
    [SerializeField] private float delayAfterApex = 1f;
    [SerializeField] private float liftoffHeightThreshold = 0.5f;
    [SerializeField] private float maxTrackingDuration = 30f;
    [Tooltip("Failsafe for a rocket that never lifts off (for example cargo resting on the floor). If the apex stops rising for this long, tracking ends and the round resolves even though the liftoff threshold was never reached.")]
    [SerializeField] private float maxStallDuration = 5f;

    public float ApexHeight { get; private set; }
    public bool IsTracking { get; private set; }

    private float timeSinceApex;
    private float timeSinceApexRose;
    private float elapsedTracking;

    public void BeginTracking()
    {
        ApexHeight = 0f;
        timeSinceApex = 0f;
        timeSinceApexRose = 0f;
        elapsedTracking = 0f;
        IsTracking = true;
    }

    public void StopTracking()
    {
        IsTracking = false;
    }

    private void FixedUpdate()
    {
        if (!IsTracking) return;

        if (rocket.CargoPiece == null)
        {
            StopTracking();
            return;
        }

        elapsedTracking += Time.fixedDeltaTime;
        if (elapsedTracking >= maxTrackingDuration)
        {
            StopTracking();
            return;
        }

        float current = rocket.CargoPiece.transform.position.y - rocket.PadY;
        if (current > ApexHeight)
        {
            ApexHeight = current;
            timeSinceApex = 0f;
            timeSinceApexRose = 0f;
            return;
        }

        timeSinceApexRose += Time.fixedDeltaTime;
        if (timeSinceApexRose >= maxStallDuration)
        {
            StopTracking();
            return;
        }

        if (ApexHeight < liftoffHeightThreshold) return;

        timeSinceApex += Time.fixedDeltaTime;
        if (timeSinceApex >= delayAfterApex) StopTracking();
    }
}
