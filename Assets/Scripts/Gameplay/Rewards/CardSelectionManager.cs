using System.Collections.Generic;
using UnityEngine;

public class CardSelectionManager : MonoBehaviour
{
    [SerializeField] private CardProvider cardProvider;
    [SerializeField] private RewardIntroBanner bannerPrefab;
    [SerializeField] private Transform bannerContainer;
    [SerializeField] private PieceCardView cardPrefab;
    [SerializeField] private Transform cardContainer;

    private RewardIntroBanner activeBanner;
    private readonly List<PieceCardView> activeCards = new();
    private List<PieceCard> offeredCards;
    private int pendingCardDismissals;

    private void OnEnable()
    {
        GameManager.Instance.OnRoundResult += HandleRoundResult;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRoundResult -= HandleRoundResult;

        DestroyActiveCards();
        DespawnBanner();
    }

    private void HandleRoundResult(float apex, float targetHeight, bool success)
    {
        if (!success)
            return;

        offeredCards = cardProvider.GetOfferedCards();
        if (offeredCards == null || offeredCards.Count == 0)
        {
            Debug.LogWarning($"[CardSelectionManager] No card offered for completed level {LevelManager.Instance.CurrentLevel}; advancing without a card.");
            GameManager.Instance.GoToNextLevel();
            return;
        }

        activeBanner = Instantiate(bannerPrefab, bannerContainer);
        activeBanner.IntroCompleted += ShowCards;
    }

    private void ShowCards()
    {
        if (activeBanner != null)
            activeBanner.IntroCompleted -= ShowCards;

        foreach (PieceCard offeredCard in offeredCards)
        {
            PieceCard cardToAcquire = offeredCard;

            PieceCardView cardView = Instantiate(cardPrefab, cardContainer);
            cardView.Populate(cardToAcquire);
            cardView.Clicked += () => HandleCardPicked(cardToAcquire, cardView);
            activeCards.Add(cardView);
        }
    }

    private void HandleCardPicked(PieceCard pickedCard, PieceCardView pickedCardView)
    {
        if (offeredCards == null)
            return;

        LevelManager.Instance.AcquirePiece(pickedCard.PiecePrefab);
        offeredCards = null;

        DismissUnpickedCards(pickedCardView);
        DismissBanner();
        GameManager.Instance.GoToNextLevel();
    }

    private void DismissUnpickedCards(PieceCardView pickedCardView)
    {
        pendingCardDismissals = 0;

        foreach (PieceCardView cardView in activeCards)
        {
            if (cardView == null || cardView == pickedCardView)
                continue;

            cardView.DismissCompleted += HandleCardDismissCompleted;
            if (cardView.Dismiss())
                pendingCardDismissals++;
            else
                cardView.DismissCompleted -= HandleCardDismissCompleted;
        }

        if (pendingCardDismissals == 0)
            DestroyActiveCards();
    }

    private void HandleCardDismissCompleted()
    {
        pendingCardDismissals--;
        if (pendingCardDismissals <= 0)
            DestroyActiveCards();
    }

    private void DestroyActiveCards()
    {
        foreach (PieceCardView cardView in activeCards)
        {
            if (cardView == null)
                continue;

            cardView.DismissCompleted -= HandleCardDismissCompleted;
            Destroy(cardView.gameObject);
        }

        activeCards.Clear();
    }

    private void DismissBanner()
    {
        if (activeBanner == null)
            return;

        activeBanner.IntroCompleted -= ShowCards;
        activeBanner.Dismiss();
        activeBanner = null;
    }

    private void DespawnBanner()
    {
        if (activeBanner == null)
            return;

        activeBanner.IntroCompleted -= ShowCards;
        Destroy(activeBanner.gameObject);
        activeBanner = null;
    }
}
