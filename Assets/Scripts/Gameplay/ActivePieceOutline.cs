using System.Collections.Generic;
using UnityEngine;

public class ActivePieceOutline : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;
    [ColorUsage(true, true)]
    [SerializeField] private Color outlineColor = new Color(0.5f, 2.5f, 3f, 1f);
    [Min(0f)]
    [SerializeField] private float outlineWidthInWorldUnits = 0.1f;
    [SerializeField] private int sortingOrderOffset = -1;

    private static readonly int OutlineColorPropertyId = Shader.PropertyToID("_OutlineColor");

    private FallingPieceController trackedPiece;
    private readonly List<PieceOutlineRenderer> pieceOutlineRenderers = new();
    private MaterialPropertyBlock reusablePropertyBlock;

    private void LateUpdate()
    {
        FallingPieceController activePiece = ResolveActivePiece();

        if (activePiece != trackedPiece)
        {
            RebuildOutlineFor(activePiece);
        }

        SyncOutlineRenderers();
        ApplyOutlineProperties();
    }

    private FallingPieceController ResolveActivePiece()
    {
        if (PieceSpawner.Instance == null) return null;

        FallingPieceController activePiece = PieceSpawner.Instance.Active;
        if (activePiece == null || activePiece.Released) return null;

        return activePiece;
    }

    private void RebuildOutlineFor(FallingPieceController activePiece)
    {
        ClearOutlineRenderers();
        trackedPiece = activePiece;

        if (activePiece == null) return;

        SpriteRenderer[] sourceRenderers = activePiece.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sourceRenderer in sourceRenderers)
        {
            pieceOutlineRenderers.Add(CreateOutlineRenderer(sourceRenderer));
        }
    }

    private PieceOutlineRenderer CreateOutlineRenderer(SpriteRenderer sourceRenderer)
    {
        var outlineObject = new GameObject("ActivePieceOutline");
        outlineObject.transform.SetParent(sourceRenderer.transform, worldPositionStays: false);

        var outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
        outlineRenderer.sharedMaterial = outlineMaterial;

        return new PieceOutlineRenderer(sourceRenderer, outlineRenderer, sortingOrderOffset);
    }

    private void SyncOutlineRenderers()
    {
        for (int index = pieceOutlineRenderers.Count - 1; index >= 0; index--)
        {
            if (!pieceOutlineRenderers[index].TrySync(outlineWidthInWorldUnits))
            {
                DestroyOutlineRenderer(pieceOutlineRenderers[index]);
                pieceOutlineRenderers.RemoveAt(index);
            }
        }
    }

    private void ApplyOutlineProperties()
    {
        reusablePropertyBlock ??= new MaterialPropertyBlock();

        foreach (PieceOutlineRenderer pieceOutlineRenderer in pieceOutlineRenderers)
        {
            pieceOutlineRenderer.ApplyProperties(OutlineColorPropertyId, outlineColor, reusablePropertyBlock);
        }
    }

    private void ClearOutlineRenderers()
    {
        foreach (PieceOutlineRenderer pieceOutlineRenderer in pieceOutlineRenderers)
        {
            DestroyOutlineRenderer(pieceOutlineRenderer);
        }
        pieceOutlineRenderers.Clear();
    }

    private void DestroyOutlineRenderer(PieceOutlineRenderer pieceOutlineRenderer)
    {
        if (pieceOutlineRenderer.OutlineObject != null) Destroy(pieceOutlineRenderer.OutlineObject);
    }

    private sealed class PieceOutlineRenderer
    {
        private readonly SpriteRenderer sourceRenderer;
        private readonly SpriteRenderer outlineRenderer;
        private readonly int sortingOrderOffset;

        public PieceOutlineRenderer(SpriteRenderer sourceRenderer, SpriteRenderer outlineRenderer, int sortingOrderOffset)
        {
            this.sourceRenderer = sourceRenderer;
            this.outlineRenderer = outlineRenderer;
            this.sortingOrderOffset = sortingOrderOffset;
        }

        public GameObject OutlineObject => outlineRenderer != null ? outlineRenderer.gameObject : null;

        public bool TrySync(float outlineWidthInWorldUnits)
        {
            if (sourceRenderer == null || outlineRenderer == null) return false;

            Sprite sprite = sourceRenderer.sprite;

            outlineRenderer.sprite = sprite;
            outlineRenderer.flipX = sourceRenderer.flipX;
            outlineRenderer.flipY = sourceRenderer.flipY;
            outlineRenderer.enabled = sourceRenderer.enabled && sprite != null;
            outlineRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            outlineRenderer.sortingOrder = sourceRenderer.sortingOrder + sortingOrderOffset;

            if (sprite != null)
                EnlargeAroundSpriteCenter(sprite, outlineWidthInWorldUnits);

            return true;
        }

        private void EnlargeAroundSpriteCenter(Sprite sprite, float outlineWidthInWorldUnits)
        {
            Vector2 spriteSize = sprite.bounds.size;
            Vector3 spriteCenter = sprite.bounds.center;

            float scaleX = spriteSize.x > 0f ? (spriteSize.x + 2f * outlineWidthInWorldUnits) / spriteSize.x : 1f;
            float scaleY = spriteSize.y > 0f ? (spriteSize.y + 2f * outlineWidthInWorldUnits) / spriteSize.y : 1f;

            Transform outlineTransform = outlineRenderer.transform;
            outlineTransform.localScale = new Vector3(scaleX, scaleY, 1f);
            outlineTransform.localPosition = new Vector3(spriteCenter.x * (1f - scaleX), spriteCenter.y * (1f - scaleY), 0f);
        }

        public void ApplyProperties(int colorPropertyId, Color color, MaterialPropertyBlock reusablePropertyBlock)
        {
            if (outlineRenderer == null) return;

            outlineRenderer.GetPropertyBlock(reusablePropertyBlock);
            reusablePropertyBlock.SetColor(colorPropertyId, color);
            outlineRenderer.SetPropertyBlock(reusablePropertyBlock);
        }
    }
}
