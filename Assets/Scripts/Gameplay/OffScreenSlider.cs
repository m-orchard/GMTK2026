using DG.Tweening;
using UnityEngine;

public class OffScreenSlider : MonoBehaviour
{
    [SerializeField] private float exitTargetX = 25f;
    [SerializeField] private Ease exitEase = Ease.InQuad;

    private Vector3 homePosition;
    private Tween exitTween;

    private void Awake()
    {
        homePosition = transform.position;
    }

    public void ExitOffScreen(float duration)
    {
        exitTween?.Kill();
        exitTween = transform.DOMoveX(exitTargetX, duration).SetEase(exitEase);
    }

    public void ResetPosition()
    {
        exitTween?.Kill();
        exitTween = null;
        transform.position = homePosition;
    }
}
