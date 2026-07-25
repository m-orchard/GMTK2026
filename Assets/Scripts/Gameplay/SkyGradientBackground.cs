using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class SkyGradientBackground : MonoBehaviour
{
    [SerializeField] private Gradient skyGradient;
    [SerializeField] private int gradientResolution = 256;
    [SerializeField] private float bottomWorldY = -10f;
    [SerializeField] private float topWorldY = 120f;
    [SerializeField] private Camera trackingCamera;
    [SerializeField] private float horizontalPadding = 2f;
    [SerializeField] private float backgroundDepth = 20f;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = -1000;
    [SerializeField] private bool matchCameraBackgroundToGradientTop = true;

    private SpriteRenderer spriteRenderer;
    private Texture2D generatedTexture;
    private Color gradientTopColor;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        RebuildGradientSprite();
    }

    private void OnDisable()
    {
        DestroyGeneratedTexture();
    }

    private void LateUpdate()
    {
        FitToVisibleArea();
    }

    [ContextMenu("Rebuild Sky Gradient")]
    private void RebuildGradientSprite()
    {
        if (skyGradient == null || gradientResolution < 2)
        {
            return;
        }

        DestroyGeneratedTexture();

        generatedTexture = new Texture2D(1, gradientResolution)
        {
            name = "SkyGradient",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Color[] gradientPixels = new Color[gradientResolution];
        for (int pixelRow = 0; pixelRow < gradientResolution; pixelRow++)
        {
            float verticalFraction = (float)pixelRow / (gradientResolution - 1);
            gradientPixels[pixelRow] = skyGradient.Evaluate(verticalFraction);
        }

        generatedTexture.SetPixels(gradientPixels);
        generatedTexture.Apply();

        gradientTopColor = skyGradient.Evaluate(1f);

        spriteRenderer.sprite = Sprite.Create(
            generatedTexture,
            new Rect(0f, 0f, 1f, gradientResolution),
            new Vector2(0.5f, 0f),
            1f);

        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder;
    }

    private void FitToVisibleArea()
    {
        Camera activeCamera = trackingCamera != null ? trackingCamera : Camera.main;
        if (activeCamera == null || !activeCamera.orthographic)
        {
            return;
        }

        float visibleWorldHeight = activeCamera.orthographicSize * 2f;
        float visibleWorldWidth = visibleWorldHeight * activeCamera.aspect;
        float spriteWorldWidth = visibleWorldWidth + horizontalPadding * 2f;
        float spriteWorldHeight = topWorldY - bottomWorldY;

        transform.position = new Vector3(activeCamera.transform.position.x, bottomWorldY, backgroundDepth);
        transform.localScale = new Vector3(spriteWorldWidth, spriteWorldHeight / gradientResolution, 1f);

        if (matchCameraBackgroundToGradientTop)
        {
            activeCamera.clearFlags = CameraClearFlags.SolidColor;
            activeCamera.backgroundColor = gradientTopColor;
        }
    }

    private void DestroyGeneratedTexture()
    {
        if (generatedTexture == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(generatedTexture);
        }
        else
        {
            DestroyImmediate(generatedTexture);
        }
    }
}
