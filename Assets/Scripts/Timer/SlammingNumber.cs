using DG.Tweening;
using TMPro;
using UnityEngine;

public class SlammingNumber : MonoBehaviour
{
    [SerializeField] private RectTransform slammingTransform;
    [SerializeField] private TextMeshProUGUI numberLabel;
    [SerializeField] private AudioClip slamSound;

    private AudioClip countdownVoice;

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

    public void Play(int numberToDisplay, AudioClip countdownVoiceClip)
    {
        numberLabel.text = numberToDisplay.ToString();
        countdownVoice = countdownVoiceClip;
        BeginSlam();
    }

    private void BeginSlam()
    {
        slammingTransform.localScale = Vector3.one * slamStartScale;
        slammingTransform.anchoredPosition = Vector2.zero;
        slammingTransform.localRotation = Quaternion.identity;

        PlaySlamSound();
        PlayCountdownVoice();

        float halfSettleDuration = impactSettleDuration * 0.5f;

        Sequence slamSequence = DOTween.Sequence();
        slamSequence.SetUpdate(true);
        slamSequence.Append(slammingTransform.DOScale(Vector3.one, slamDuration).SetEase(Ease.InQuad));
        slamSequence.AppendCallback(ApplyRecoil);
        slamSequence.Append(slammingTransform.DOScale(Vector3.one * impactSettleScale, halfSettleDuration).SetEase(Ease.OutQuad));
        slamSequence.Append(slammingTransform.DOScale(Vector3.one, halfSettleDuration).SetEase(Ease.InOutQuad));
        slamSequence.Append(slammingTransform.DOScale(Vector3.one * (1f + hangScaleBonus), hangDuration).SetEase(Ease.OutSine));
        slamSequence.Append(slammingTransform.DOAnchorPosY(-exitFallDistance, exitDuration).SetEase(Ease.InQuad));
        slamSequence.Join(slammingTransform.DOLocalRotate(new Vector3(0f, 0f, -exitRotation), exitDuration).SetEase(Ease.InQuad));
        slamSequence.OnComplete(DestroySelf);
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

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
