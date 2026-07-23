using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private RocketAssembly rocket;
    [SerializeField] private Camera cam;
    [SerializeField] private float positionLerp = 5f;
    [SerializeField] private float zoomLerp = 4f;
    [SerializeField] private float zoomPadding = 2f;
    [SerializeField] private float buildFramingY = 0f;

    private bool following;
    private float buildFramingX;
    private float buildFramingOrthoSize;

    private void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
        buildFramingX = transform.position.x;
        buildFramingOrthoSize = cam != null ? cam.orthographicSize : 5f;
    }

    public void StartFollowing()
    {
        following = true;
    }

    public void ResetToBuildFraming()
    {
        following = false;
        Vector3 pos = transform.position;
        pos.x = buildFramingX;
        pos.y = buildFramingY;
        transform.position = pos;

        if (cam != null) cam.orthographicSize = buildFramingOrthoSize;
    }

    private void LateUpdate()
    {
        if (!following) return;

        Bounds bounds = rocket.GetBounds(rocket.GetConnectedPieces());
        bounds.Encapsulate(new Vector3(bounds.center.x, rocket.PadY, 0f));
        bounds.Encapsulate(new Vector3(bounds.center.x, buildFramingY, 0f));

        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, bounds.center.x, positionLerp * Time.deltaTime);
        pos.y = Mathf.Lerp(pos.y, bounds.center.y, positionLerp * Time.deltaTime);
        transform.position = pos;

        if (cam != null)
        {
            float requiredSize = Mathf.Max(bounds.extents.y, bounds.extents.x / cam.aspect) + zoomPadding;
            requiredSize = Mathf.Max(requiredSize, buildFramingOrthoSize);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, requiredSize, zoomLerp * Time.deltaTime);
        }
    }
}
