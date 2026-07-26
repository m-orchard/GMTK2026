using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class RocketCanvasFollower : MonoBehaviour {
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RocketAssembly rocketAssembly;
    [SerializeField] private Vector3 worldOffset = Vector3.zero;
    [SerializeField] private bool debugLogging = true;
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private bool isFollowRocket = false;

    private int? currentBurnPhase = null;

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;
    private float TotalFuel = 0f;
    private float RemainingFuel = 0f;
    private float BurnDuration = 0f;
    private float RemainingBurnDuration = 0f;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
            canvasRectTransform = canvas.GetComponent<RectTransform>();

        LaunchController.Instance.OnBurnStart += OnBurnStart;
        LaunchController.Instance.OnBurnEnd += OnBurnEnd;

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

    private void OnBurnStart(int phase)
    {
        currentBurnPhase = phase;
    }

    private void OnBurnEnd(int phase)
    {
        currentBurnPhase = null;
        if (phase == rocketAssembly.Rocket.EngineGroups.Count - 1)
        {
            RemainingBurnDuration = 0;
            RemainingFuel = 0;
            UpdateFuelText();
        }
    }

    private void HandleLaunchStarted() {
        var rocket = rocketAssembly.Rocket;
        var engineGroups = rocket.EngineGroups;
        TotalFuel = rocket.AvailableFuel;
        RemainingFuel = TotalFuel;
        BurnDuration = rocket.BurnDuration;
        RemainingBurnDuration = BurnDuration;

        if (fuelText) {
            fuelText.gameObject.SetActive(true);
            UpdateFuelText();
        }
    }

    private void UpdateFuelText()
    {
        if (fuelText) {
            fuelText.text = $"{RemainingFuel:N2} gallons";
        }
    }

    private void HandleBuildStart() {
        TotalFuel = 0;
        RemainingFuel = 0;
        BurnDuration = 0;
        RemainingBurnDuration = 0;
        currentBurnPhase = null;

        if (fuelText) {
            fuelText.gameObject.SetActive(false);
        }

        UpdateFuelText();
    }

    void Update()
    {
        if (currentBurnPhase == null) return;

        if (RemainingBurnDuration <= 0f) return;

        float delta = Time.deltaTime;

        // Rate of value-loss per second, scaled to whatever's left of duration
        float rate = TotalFuel / BurnDuration;

        RemainingBurnDuration = Mathf.Max(0f, BurnDuration - delta);
        RemainingFuel = Mathf.Max(0f, RemainingFuel - rate * delta);

        if (RemainingBurnDuration <= 0f)
        {
            RemainingBurnDuration = 0f;
            RemainingFuel = 0f;
        }

        UpdateFuelText();
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