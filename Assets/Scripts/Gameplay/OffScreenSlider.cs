using DG.Tweening;
using UnityEngine;

public class OffScreenSlider : MonoBehaviour
{
    [SerializeField] private float exitTargetX = 25f;
    [SerializeField] private Ease exitEase = Ease.InQuad;
    [SerializeField] private float minimumDistanceAboveRocketTop = 5f;
    [SerializeField] private float rocketFollowThreshold = 0.1f;
    [SerializeField] private float rocketFollowDuration = 0.35f;
    [SerializeField] private Ease rocketFollowEase = Ease.OutQuad;

    private Vector3 homePosition;
    private Tween exitTween;
    private Tween rocketFollowTween;
    private bool followingRocketTop;
    private float followTargetHeight;

    private void Awake()
    {
        homePosition = transform.position;
    }

    private void Update()
    {
        if (!followingRocketTop || RocketAssembly.Instance == null)
            return;

        float restingHeight = homePosition.y;
        float clearanceHeight = RocketAssembly.Instance.HighestPointY() + minimumDistanceAboveRocketTop;
        float desiredHeight = Mathf.Max(restingHeight, clearanceHeight);

        if (Mathf.Abs(desiredHeight - followTargetHeight) < rocketFollowThreshold)
            return;

        followTargetHeight = desiredHeight;
        rocketFollowTween?.Kill();
        rocketFollowTween = transform.DOMoveY(desiredHeight, rocketFollowDuration).SetEase(rocketFollowEase);
    }

    public void BeginFollowingRocketTop()
    {
        rocketFollowTween?.Kill();
        rocketFollowTween = null;
        followTargetHeight = homePosition.y;
        followingRocketTop = true;
    }

    public void StopFollowingRocketTop()
    {
        followingRocketTop = false;
        rocketFollowTween?.Kill();
        rocketFollowTween = null;
    }

    public void ExitOffScreen(float duration)
    {
        StopFollowingRocketTop();
        exitTween?.Kill();
        exitTween = transform.DOMoveX(exitTargetX, duration).SetEase(exitEase);
    }

    public void ResetPosition()
    {
        StopFollowingRocketTop();
        exitTween?.Kill();
        exitTween = null;
        transform.position = homePosition;
    }
}
