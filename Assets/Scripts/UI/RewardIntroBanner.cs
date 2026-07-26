using System;
using DG.Tweening;
using UnityEngine;

public class RewardIntroBanner : MonoBehaviour
{
    [Serializable]
    private class BannerLine
    {
        public RectTransform lineTransform;
        public CanvasGroup lineGroup;
        public AudioClip popInSound;
    }

    [SerializeField] private BannerLine congratulationsLine;
    [SerializeField] private BannerLine instructionLine;

    [Header("Pop In")]
    [SerializeField] private float startScale = 0f;
    [SerializeField] private float peakScale = 1.15f;
    [SerializeField] private float riseDuration = 0.32f;
    [SerializeField] private float riseOvershoot = 3.2f;
    [SerializeField] private float settleDuration = 0.18f;
    [SerializeField] private float fadeDuration = 0.16f;

    [Header("Sequence")]
    [SerializeField] private float initialDelay = 0.1f;
    [SerializeField] private float gapBetweenLines = 0.28f;
    [SerializeField] private float holdBeforeCard = 0.2f;

    [Header("Exit")]
    [SerializeField] private float exitScale = 0f;
    [SerializeField] private float exitDuration = 0.24f;

    public event Action IntroCompleted;

    private Sequence introSequence;
    private Sequence exitSequence;

    private void Awake()
    {
        HideLine(congratulationsLine);
        HideLine(instructionLine);
    }

    private void Start()
    {
        PlayIntro();
    }

    private void OnDestroy()
    {
        introSequence?.Kill();
        exitSequence?.Kill();
    }

    private void PlayIntro()
    {
        introSequence = DOTween.Sequence();
        introSequence.SetUpdate(true);

        introSequence.AppendInterval(initialDelay);
        AppendLinePopIn(introSequence, congratulationsLine);
        introSequence.AppendInterval(gapBetweenLines);
        AppendLinePopIn(introSequence, instructionLine);
        introSequence.AppendInterval(holdBeforeCard);
        introSequence.OnComplete(() => IntroCompleted?.Invoke());
    }

    private void AppendLinePopIn(Sequence sequence, BannerLine line)
    {
        sequence.AppendCallback(() => PlaySound(line.popInSound));
        sequence.Append(line.lineTransform.DOScale(Vector3.one * peakScale, riseDuration)
            .SetEase(Ease.OutBack, riseOvershoot));
        sequence.Join(line.lineGroup.DOFade(1f, fadeDuration)
            .SetEase(Ease.OutQuad));
        sequence.Append(line.lineTransform.DOScale(Vector3.one, settleDuration)
            .SetEase(Ease.InOutQuad));
    }

    public void Dismiss()
    {
        introSequence?.Kill();

        exitSequence = DOTween.Sequence();
        exitSequence.SetUpdate(true);

        AppendLineExit(exitSequence, congratulationsLine);
        AppendLineExit(exitSequence, instructionLine);

        exitSequence.OnComplete(() => Destroy(gameObject));
    }

    private void AppendLineExit(Sequence sequence, BannerLine line)
    {
        sequence.Join(line.lineTransform.DOScale(Vector3.one * exitScale, exitDuration)
            .SetEase(Ease.InBack));
        sequence.Join(line.lineGroup.DOFade(0f, exitDuration)
            .SetEase(Ease.InQuad));
    }

    private void HideLine(BannerLine line)
    {
        if (line.lineTransform != null)
            line.lineTransform.localScale = Vector3.one * startScale;
        if (line.lineGroup != null)
            line.lineGroup.alpha = 0f;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlaySound(clip);
    }
}
