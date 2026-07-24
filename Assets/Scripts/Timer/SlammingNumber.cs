using DG.Tweening;
using TMPro;
using UnityEngine;

public class SlammingNumber : MonoBehaviour
{
    [SerializeField] private RectTransform slammingTransform;
    [SerializeField] private TextMeshProUGUI numberLabel;
    [SerializeField] private AudioClip slamSound;

    private AudioClip countdownVoice;
    private Color designatedTopColor;
    private Color designatedBottomColor;
    private Tween colorFlash;
    private Tween brightnessPulse;

    [SerializeField] private float slamStartScale = 3f;
    [SerializeField] private float slamDuration = 0.1f;
    [SerializeField] private float impactSettleScale = 1.05f;
    [SerializeField] private float impactSettleDuration = 0.1f;
    [SerializeField] private float hangDuration = 0.6f;
    [SerializeField] private float hangScaleBonus = 0.05f;
    [SerializeField] private float exitDuration = 0.2f;
    [SerializeField] private float exitFallDistance = 1600f;
    [SerializeField] private float exitRotation = 20f;
    [SerializeField] private float recoilForce = 0.5f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashFadeDuration = 0.12f;
    [SerializeField] private float pulseBrightnessBonus = 0.08f;
    [SerializeField] private float pulseCycleDuration = 0.7f;

    public void Play(int numberToDisplay, AudioClip countdownVoiceClip, Color topColor, Color bottomColor)
    {
        numberLabel.text = numberToDisplay.ToString();
        countdownVoice = countdownVoiceClip;
        designatedTopColor = topColor;
        designatedBottomColor = bottomColor;
        BeginSlam();
    }

    private void BeginSlam()
    {
        slammingTransform.localScale = Vector3.one * slamStartScale;
        slammingTransform.anchoredPosition = Vector2.zero;
        slammingTransform.localRotation = Quaternion.identity;

        FlashThenSettleToDesignatedColor();
        PlaySlamSound();
        PlayCountdownVoice();

        float halfSettleDuration = impactSettleDuration * 0.5f;

        Sequence slamSequence = DOTween.Sequence();
        slamSequence.SetUpdate(true);
        slamSequence.Append(slammingTransform.DOScale(Vector3.one, slamDuration).SetEase(Ease.InQuad));
        slamSequence.AppendCallback(ApplyRecoil);
        slamSequence.AppendCallback(PulseChromaticAberration);
        slamSequence.Append(slammingTransform.DOScale(Vector3.one * impactSettleScale, halfSettleDuration).SetEase(Ease.OutQuad));
        slamSequence.Append(slammingTransform.DOScale(Vector3.one, halfSettleDuration).SetEase(Ease.InOutQuad));
        slamSequence.Append(slammingTransform.DOScale(Vector3.one * (1f + hangScaleBonus), hangDuration).SetEase(Ease.OutSine));
        slamSequence.Append(slammingTransform.DOAnchorPosY(-exitFallDistance, exitDuration).SetEase(Ease.InQuad));
        slamSequence.Join(slammingTransform.DOLocalRotate(new Vector3(0f, 0f, -exitRotation), exitDuration).SetEase(Ease.InQuad));
        slamSequence.OnComplete(DestroySelf);
    }

    private void FlashThenSettleToDesignatedColor()
    {
        ApplyGradient(flashColor, flashColor);

        colorFlash = DOTween.To(
                () => 0f,
                flashProgress => ApplyGradient(
                    Color.Lerp(flashColor, designatedTopColor, flashProgress),
                    Color.Lerp(flashColor, designatedBottomColor, flashProgress)),
                1f,
                flashFadeDuration)
            .SetUpdate(true)
            .SetEase(Ease.OutQuad)
            .OnComplete(StartBrightnessPulse);
    }

    private void StartBrightnessPulse()
    {
        Color brightenedTopColor = Brighten(designatedTopColor);
        Color brightenedBottomColor = Brighten(designatedBottomColor);

        brightnessPulse = DOTween.To(
                () => 0f,
                pulseAmount => ApplyGradient(
                    Color.Lerp(designatedTopColor, brightenedTopColor, pulseAmount),
                    Color.Lerp(designatedBottomColor, brightenedBottomColor, pulseAmount)),
                1f,
                pulseCycleDuration)
            .SetUpdate(true)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void ApplyGradient(Color topColor, Color bottomColor)
    {
        numberLabel.colorGradient = new VertexGradient(topColor, topColor, bottomColor, bottomColor);
    }

    private Color Brighten(Color color)
    {
        return new Color(
            color.r + pulseBrightnessBonus,
            color.g + pulseBrightnessBonus,
            color.b + pulseBrightnessBonus,
            color.a);
    }

    private void PlaySlamSound()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlaySound(slamSound);
    }

    private void PlayCountdownVoice()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlaySound(countdownVoice);
    }

    private void ApplyRecoil()
    {
        if (ScreenShake.Instance == null)
        {
            return;
        }

        ScreenShake.Instance.Recoil(recoilForce);
    }

    private void PulseChromaticAberration()
    {
        if (ChromaticAberrationPulse.Instance == null)
        {
            return;
        }

        ChromaticAberrationPulse.Instance.Pulse();
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (colorFlash != null)
        {
            colorFlash.Kill();
        }

        if (brightnessPulse != null)
        {
            brightnessPulse.Kill();
        }
    }
}
