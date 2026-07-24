using UnityEngine;

public class HeightTracker : Singleton<HeightTracker>
{
    [SerializeField] private RocketAssembly rocket;
    [SerializeField] private float delayAfterApex = 1f;
    [SerializeField] private float liftoffHeightThreshold = 0.5f;
    [SerializeField] private float maxTrackingDuration = 30f;

    public float ApexHeight { get; private set; }
    public bool IsTracking { get; private set; }

    private float timeSinceApex;
    private float elapsedTracking;

    public void BeginTracking()
    {
        ApexHeight = 0f;
        timeSinceApex = 0f;
        elapsedTracking = 0f;
        IsTracking = true;
    }

    public void StopTracking()
    {
        IsTracking = false;
    }

    private void FixedUpdate()
    {
        if (!IsTracking || rocket.CargoPiece == null) return;

        elapsedTracking += Time.fixedDeltaTime;
        if (elapsedTracking >= maxTrackingDuration)
        {
            StopTracking();
            return;
        }

        float current = rocket.CargoPiece.transform.position.y - rocket.PadY;
        if (current >= ApexHeight)
        {
            ApexHeight = current;
            timeSinceApex = 0f;
            return;
        }

        if (ApexHeight < liftoffHeightThreshold) return;

        timeSinceApex += Time.fixedDeltaTime;
        if (timeSinceApex >= delayAfterApex) StopTracking();
    }
}
