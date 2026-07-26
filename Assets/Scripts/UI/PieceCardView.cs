using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PieceCardView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI descriptionLabel;
    [SerializeField] private CardButton cardButton;
    [SerializeField] private RectTransform animatedTransform;

    [Header("Sound")]
    [SerializeField] private AudioClip entranceSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip exitSound;

    [Header("Entrance")]
    [SerializeField] private float entranceStartScale = 0f;
    [SerializeField] private float entrancePeakScale = 1.18f;
    [SerializeField] private float entranceRiseToPeakDuration = 0.38f;
    [SerializeField] private float entranceOvershoot = 3.4f;
    [SerializeField] private float entranceSettleDuration = 0.22f;
    [SerializeField] private float entranceStartRotation = -12f;
    [SerializeField] private float entranceFadeDuration = 0.18f;

    [Header("Hover")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float hoverTilt = 3f;
    [SerializeField] private float hoverDuration = 0.28f;
    [SerializeField] private float hoverOvershoot = 4f;

    [Header("Press")]
    [SerializeField] private float pressScale = 0.92f;
    [SerializeField] private float pressDuration = 0.09f;

    [Header("Click")]
    [SerializeField] private float clickPunchScale = 1.28f;
    [SerializeField] private float clickPunchDuration = 0.14f;
    [SerializeField] private float clickPunchOvershoot = 6f;

    [Header("Exit")]
    [SerializeField] private float exitScale = 0f;
    [SerializeField] private float exitDuration = 0.3f;
    [SerializeField] private float exitSpinRotation = 22f;

    public event Action Clicked;

    private CanvasGroup canvasGroup;
    private Sequence entranceSequence;
    private Sequence interactionSequence;
    private Sequence dismissSequence;

    private bool isReady;
    private bool isDismissing;
    private bool isHovered;
    private bool isPressed;

    private void Awake()
    {
        if (!TryGetComponent(out canvasGroup))
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (animatedTransform == null)
            animatedTransform = (RectTransform)transform;

        SubscribeToButton();
        ApplyHiddenState();
    }

    private void Start()
    {
        PlayEntrance();
    }

    private void OnDestroy()
    {
        UnsubscribeFromButton();
        KillAllTweens();
    }

    public void Populate(PieceCard card)
    {
        if (icon != null)
            icon.sprite = card.Icon;
        if (nameLabel != null)
            nameLabel.text = card.DisplayName;
        if (descriptionLabel != null)
            descriptionLabel.text = card.HowItWorks;
    }

    private void SubscribeToButton()
    {
        if (cardButton == null)
            return;

        cardButton.Clicked += HandleCardButtonClicked;
        cardButton.PointerEntered += HandlePointerEntered;
        cardButton.PointerExited += HandlePointerExited;
        cardButton.PointerPressed += HandlePointerPressed;
        cardButton.PointerReleased += HandlePointerReleased;
    }

    private void UnsubscribeFromButton()
    {
        if (cardButton == null)
            return;

        cardButton.Clicked -= HandleCardButtonClicked;
        cardButton.PointerEntered -= HandlePointerEntered;
        cardButton.PointerExited -= HandlePointerExited;
        cardButton.PointerPressed -= HandlePointerPressed;
        cardButton.PointerReleased -= HandlePointerReleased;
    }

    private void ApplyHiddenState()
    {
        animatedTransform.localScale = Vector3.one * entranceStartScale;
        animatedTransform.localRotation = Quaternion.Euler(0f, 0f, entranceStartRotation);
        canvasGroup.alpha = 0f;
    }

    private void PlayEntrance()
    {
        PlaySound(entranceSound);

        entranceSequence = DOTween.Sequence();
        entranceSequence.SetUpdate(true);

        entranceSequence.Append(animatedTransform.DOScale(Vector3.one * entrancePeakScale, entranceRiseToPeakDuration)
            .SetEase(Ease.OutBack, entranceOvershoot));
        entranceSequence.Join(animatedTransform.DOLocalRotate(Vector3.zero, entranceRiseToPeakDuration)
            .SetEase(Ease.OutBack, entranceOvershoot));
        entranceSequence.Join(canvasGroup.DOFade(1f, entranceFadeDuration)
            .SetEase(Ease.OutQuad));

        entranceSequence.Append(animatedTransform.DOScale(Vector3.one, entranceSettleDuration)
            .SetEase(Ease.InOutQuad));

        entranceSequence.OnComplete(HandleEntranceComplete);
    }

    private void HandleEntranceComplete()
    {
        isReady = true;
        RefreshInteractionState();
    }

    private void HandlePointerEntered()
    {
        isHovered = true;

        if (isReady && !isDismissing)
            PlaySound(hoverSound);

        RefreshInteractionState();
    }

    private void HandlePointerExited()
    {
        isHovered = false;
        isPressed = false;
        RefreshInteractionState();
    }

    private void HandlePointerPressed()
    {
        isPressed = true;
        RefreshInteractionState();
    }

    private void HandlePointerReleased()
    {
        isPressed = false;
        RefreshInteractionState();
    }

    private void RefreshInteractionState()
    {
        if (!isReady || isDismissing)
            return;

        float targetScale = isPressed ? pressScale : (isHovered ? hoverScale : 1f);
        float targetTilt = isHovered ? hoverTilt : 0f;
        float duration = isPressed ? pressDuration : hoverDuration;
        Ease ease = isPressed ? Ease.OutQuad : Ease.OutBack;

        interactionSequence?.Kill();
        interactionSequence = DOTween.Sequence();
        interactionSequence.SetUpdate(true);

        Tweener scaleTween = animatedTransform.DOScale(Vector3.one * targetScale, duration).SetEase(ease);
        if (ease == Ease.OutBack)
            scaleTween.SetEase(Ease.OutBack, hoverOvershoot);

        interactionSequence.Join(scaleTween);
        interactionSequence.Join(animatedTransform.DOLocalRotate(new Vector3(0f, 0f, targetTilt), duration).SetEase(ease));
    }

    public void Dismiss()
    {
        if (isDismissing)
            return;

        isDismissing = true;
        PlaySound(exitSound);
        PlayExit(DestroySelf);
    }

    private void HandleCardButtonClicked()
    {
        if (!isReady || isDismissing)
            return;

        isDismissing = true;
        PlaySound(clickSound);
        PlayPickedDismiss();
    }

    private void PlayPickedDismiss()
    {
        interactionSequence?.Kill();
        entranceSequence?.Kill();

        dismissSequence = DOTween.Sequence();
        dismissSequence.SetUpdate(true);

        dismissSequence.Append(animatedTransform.DOScale(Vector3.one * clickPunchScale, clickPunchDuration)
            .SetEase(Ease.OutBack, clickPunchOvershoot));

        AppendExitTweens(dismissSequence);

        dismissSequence.OnComplete(HandlePickedDismissComplete);
    }

    private void PlayExit(TweenCallback onComplete)
    {
        interactionSequence?.Kill();
        entranceSequence?.Kill();

        dismissSequence = DOTween.Sequence();
        dismissSequence.SetUpdate(true);

        AppendExitTweens(dismissSequence);

        dismissSequence.OnComplete(onComplete);
    }

    private void AppendExitTweens(Sequence sequence)
    {
        sequence.Append(animatedTransform.DOScale(Vector3.one * exitScale, exitDuration)
            .SetEase(Ease.InBack));
        sequence.Join(animatedTransform.DOLocalRotate(new Vector3(0f, 0f, exitSpinRotation), exitDuration)
            .SetEase(Ease.InQuad));
        sequence.Join(canvasGroup.DOFade(0f, exitDuration)
            .SetEase(Ease.InQuad));
    }

    private void HandlePickedDismissComplete()
    {
        Clicked?.Invoke();
        DestroySelf();
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlaySound(clip);
    }

    private void KillAllTweens()
    {
        entranceSequence?.Kill();
        interactionSequence?.Kill();
        dismissSequence?.Kill();
    }
}
