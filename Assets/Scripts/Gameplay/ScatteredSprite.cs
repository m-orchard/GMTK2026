using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(SpriteRenderer))]
public class ScatteredSprite : MonoBehaviour
{
    [FormerlySerializedAs("cloudSprites")]
    [SerializeField] private List<Sprite> sprites = new List<Sprite>();
    [SerializeField] private float minScale = 0.75f;
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private bool randomiseFacing = true;

    [Header("Drift")]
    [SerializeField] private bool drifts = true;
    [SerializeField] private float minDriftDistance = 2f;
    [SerializeField] private float maxDriftDistance = 6f;
    [SerializeField] private float minDriftDuration = 25f;
    [SerializeField] private float maxDriftDuration = 45f;
    [SerializeField] private Ease driftEase = Ease.InOutSine;

    private SpriteRenderer spriteRenderer;
    private Tween driftTween;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Randomise()
    {
        AssignRandomSprite();
        AssignRandomScale();

        if (randomiseFacing)
        {
            AssignRandomFacing();
        }

        if (drifts)
        {
            StartDrifting();
        }
    }

    private void AssignRandomSprite()
    {
        if (sprites.Count == 0)
        {
            return;
        }

        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Count)];
    }

    private void AssignRandomScale()
    {
        float scale = Random.Range(minScale, maxScale);
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void AssignRandomFacing()
    {
        spriteRenderer.flipX = Random.value > 0.5f;
    }

    private void StartDrifting()
    {
        driftTween?.Kill();

        float driftDirection = Random.value > 0.5f ? 1f : -1f;
        float driftDistance = Random.Range(minDriftDistance, maxDriftDistance) * driftDirection;
        float driftDuration = Random.Range(minDriftDuration, maxDriftDuration);

        driftTween = transform
            .DOMoveX(transform.position.x + driftDistance, driftDuration)
            .SetEase(driftEase)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        driftTween?.Kill();
    }
}
