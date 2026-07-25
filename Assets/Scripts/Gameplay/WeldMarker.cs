using DG.Tweening;
using UnityEngine;

public class WeldMarker : MonoBehaviour
{
    [SerializeField] private ParticleSystem sparkEmitter;
    [SerializeField] private float growDuration = 0.25f;
    [SerializeField] private float startScaleMultiplier = 0.1f;
    [SerializeField] private float overshoot = 3f;

    private Vector3 targetScale;
    private Tween growTween;

    private void Awake()
    {
        targetScale = transform.localScale;
    }

    private void Start()
    {
        transform.localScale = targetScale * startScaleMultiplier;
        growTween = transform
            .DOScale(targetScale, growDuration)
            .SetEase(Ease.OutBack, overshoot);

        if (sparkEmitter != null) sparkEmitter.Play();
    }

    private void OnDestroy()
    {
        growTween?.Kill();
    }
}
