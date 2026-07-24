using UnityEngine;

public class HeightTracker : MonoBehaviour
{
    [SerializeField] private RocketAssembly rocket;

    public float ApexHeight { get; private set; }
    private bool tracking;

    public void BeginTracking()
    {
        ApexHeight = 0f;
        tracking = true;
    }

    public void StopTracking()
    {
        tracking = false;
    }

    private void FixedUpdate()
    {
        if (!tracking || rocket.CargoPiece == null) return;

        float current = rocket.CargoPiece.transform.position.y - rocket.PadY;
        if (current > ApexHeight) ApexHeight = current;
    }
}
