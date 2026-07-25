using UnityEngine;

public class LandingGuide : MonoBehaviour
{
    [SerializeField] private Color guideColor = new Color(1f, 1f, 1f, 0.12f);
    [SerializeField] private int sortingOrder = -1;
    [SerializeField] private float maximumDropDistance = 100f;

    private SpriteRenderer guideRenderer;
    private Transform guideTransform;
    private readonly RaycastHit2D[] castResults = new RaycastHit2D[8];

    private void Awake()
    {
        CreateGuideBox();
    }

    private void CreateGuideBox()
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        var sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);

        var box = new GameObject("LandingGuideBox");
        guideTransform = box.transform;
        guideRenderer = box.AddComponent<SpriteRenderer>();
        guideRenderer.sprite = sprite;
        guideRenderer.color = guideColor;
        guideRenderer.sortingOrder = sortingOrder;
        guideRenderer.enabled = false;
    }

    private void LateUpdate()
    {
        FallingPieceController activePiece = PieceSpawner.Instance != null ? PieceSpawner.Instance.Active : null;

        if (activePiece == null || activePiece.Released)
        {
            guideRenderer.enabled = false;
            return;
        }

        DrawGuideBeneath(activePiece);
    }

    private void DrawGuideBeneath(FallingPieceController activePiece)
    {
        Collider2D pieceCollider = activePiece.LandingCollider;
        Bounds pieceBounds = pieceCollider.bounds;
        float dropDistance = MeasureDropDistance(pieceCollider, activePiece.LandingMask);

        float pieceBottomY = pieceBounds.min.y;
        float landingY = pieceBottomY - dropDistance;

        guideTransform.position = new Vector3(pieceBounds.center.x, (pieceBottomY + landingY) * 0.5f, guideTransform.position.z);
        guideTransform.rotation = Quaternion.identity;
        guideTransform.localScale = new Vector3(pieceBounds.size.x, dropDistance, 1f);
        guideRenderer.enabled = true;
    }

    private float MeasureDropDistance(Collider2D pieceCollider, LayerMask landingMask)
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(landingMask);
        filter.useTriggers = false;

        int hitCount = pieceCollider.Cast(Vector2.down, filter, castResults, maximumDropDistance);

        float nearestDistance = maximumDropDistance;
        for (int i = 0; i < hitCount; i++)
        {
            if (castResults[i].distance < nearestDistance) nearestDistance = castResults[i].distance;
        }
        return nearestDistance;
    }
}
