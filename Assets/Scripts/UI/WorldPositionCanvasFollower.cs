using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class RocketCanvasFollower : MonoBehaviour {
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RocketAssembly rocketAssembly;
    [SerializeField] private Vector3 worldOffset = Vector3.zero;
    [SerializeField] private bool debugLogging = true;
    [SerializeField] private GameObject fuelText;
    [SerializeField] private bool isFollowRocket = false;

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
            canvasRectTransform = canvas.GetComponent<RectTransform>();

        HandleBuildStart();
    }

    private void OnEnable() {
        GameManager.Instance.OnLaunchStarted += HandleLaunchStarted;
        GameManager.Instance.OnBuildingStarted += HandleBuildStart;
    }

    private void OnDisable() {
        if (!GameManager.Instance)
            return;

        GameManager.Instance.OnLaunchStarted -= HandleLaunchStarted;
        GameManager.Instance.OnBuildingStarted -= HandleBuildStart;
    }

    private void HandleLaunchStarted() {
        if (fuelText) {
            fuelText.gameObject.SetActive(true);
        }
    }

    private void HandleBuildStart() {
        if (fuelText) {
            fuelText.gameObject.SetActive(false);
        }
    }

    private void LateUpdate() {
        if (rectTransform == null || canvasRectTransform == null || worldCamera == null || !isFollowRocket)
            return;

        RocketAssembly assembly = rocketAssembly != null ? rocketAssembly : RocketAssembly.Instance;
        if (assembly == null) {
            if (debugLogging)
                Debug.LogWarning("[RocketCanvasFollower] No RocketAssembly available.");
            return;
        }

        Vector3 worldPosition = GetWorldPosition(assembly);
        Vector3 viewportPoint = worldCamera.WorldToViewportPoint(worldPosition);
        Vector2 screenPoint = new Vector2(viewportPoint.x * Screen.width, viewportPoint.y * Screen.height);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, null, out var localPoint)) {
            rectTransform.anchoredPosition = localPoint + new Vector2(0f, 0f);
        }

        if (debugLogging) {
            Debug.Log($"[RocketCanvasFollower] assembly={assembly.name} world={worldPosition} viewport={viewportPoint} screen={screenPoint}");
        }
    }

    private Vector3 GetWorldPosition(RocketAssembly assembly) {
        float rocketX = assembly.PointX();
        float rocketBottomY = assembly.LowestPointY();
        Vector3 basePoint = new Vector3(rocketX, rocketBottomY, 0f);
        return basePoint + worldOffset;
    }

    private void OnDrawGizmos() {
        if (!Application.isPlaying || rocketAssembly == null)
            return;

        Vector3 worldPosition = GetWorldPosition(rocketAssembly);
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(worldPosition, 0.1f);
    }
}