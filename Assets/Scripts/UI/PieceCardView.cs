using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PieceCardView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI descriptionLabel;
    [SerializeField] private CardButton cardButton;

    public event Action Clicked;

    private void Awake()
    {
        if (cardButton != null)
            cardButton.Clicked += HandleCardButtonClicked;
    }

    private void OnDestroy()
    {
        if (cardButton != null)
            cardButton.Clicked -= HandleCardButtonClicked;
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

    private void HandleCardButtonClicked()
    {
        Clicked?.Invoke();
    }
}
