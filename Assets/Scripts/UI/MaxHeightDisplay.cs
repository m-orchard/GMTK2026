using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaxHeightDisplay : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;

    [SerializeField] private Image dottedLine;
    [SerializeField] private TextMeshProUGUI heightLabel;
    [SerializeField] private GameObject aboveViewIndicator;

    private RectTransform bar;

    [SerializeField] private float topViewportLimit = 0.93f;
    [SerializeField] private float bottomViewportLimit = 0.05f;
    [SerializeField] private float dashLengthPixels = 18f;
    [SerializeField] private float gapLengthPixels = 12f;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private string heightFormat = "{0:0.0}m / {1:0}m";

    private float targetHeight;
    private bool showing;

    private void Awake()
    {
        bar = (RectTransform)transform;
        if (worldCamera == null) worldCamera = Camera.main;
        BuildDottedLine();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnLaunchStarted += Show;
        GameManager.Instance.OnBuildingStarted += Hide;
        GameManager.Instance.OnTargetHeightChanged += SetTargetHeight;
        Hide();
    }

    private void OnDisable()
    {
        GameManager.Instance.OnLaunchStarted -= Show;
        GameManager.Instance.OnBuildingStarted -= Hide;
        GameManager.Instance.OnTargetHeightChanged -= SetTargetHeight;
    }

    private void Show()
    {
        showing = true;
    }

    private void Hide()
    {
        showing = false;
        SetVisualsVisible(false);
    }

    private void SetVisualsVisible(bool visible)
    {
        dottedLine.gameObject.SetActive(visible);
        heightLabel.gameObject.SetActive(visible);
        if (!visible) aboveViewIndicator.SetActive(false);
    }

    private void SetTargetHeight(float newTargetHeight)
    {
        targetHeight = newTargetHeight;
    }

    private void LateUpdate()
    {
        if (!showing || worldCamera == null) return;

        float maxHeight = HeightTracker.Instance.ApexHeight;
        if (maxHeight < targetHeight)
        {
            SetVisualsVisible(false);
            return;
        }

        float worldY = RocketAssembly.Instance.PadY + maxHeight;
        float viewportY = worldCamera.WorldToViewportPoint(new Vector3(RocketAssembly.Instance.transform.position.x, worldY, 0f)).y;

        bool aboveView = viewportY > topViewportLimit;
        float anchoredViewportY = Mathf.Clamp(viewportY, bottomViewportLimit, topViewportLimit);

        SetVisualsVisible(true);
        bar.anchorMin = new Vector2(0f, anchoredViewportY);
        bar.anchorMax = new Vector2(1f, anchoredViewportY);
        bar.anchoredPosition = new Vector2(bar.anchoredPosition.x, 0f);

        aboveViewIndicator.SetActive(aboveView);
        heightLabel.text = string.Format(heightFormat, maxHeight, targetHeight);
    }

    private void BuildDottedLine()
    {
        int dashPixels = Mathf.Max(1, Mathf.RoundToInt(dashLengthPixels));
        int gapPixels = Mathf.Max(1, Mathf.RoundToInt(gapLengthPixels));
        int patternWidth = dashPixels + gapPixels;

        var texture = new Texture2D(patternWidth, 1);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Point;
        for (int column = 0; column < patternWidth; column++)
        {
            texture.SetPixel(column, 0, column < dashPixels ? Color.white : Color.clear);
        }
        texture.Apply();

        dottedLine.sprite = Sprite.Create(texture, new Rect(0f, 0f, patternWidth, 1f), new Vector2(0.5f, 0.5f), 1f);
        dottedLine.type = Image.Type.Tiled;
        dottedLine.color = lineColor;
    }
}
