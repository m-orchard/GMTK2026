using UnityEngine;

public class CardSelectionManager : MonoBehaviour
{
    [SerializeField] private CardProvider cardProvider;
    [SerializeField] private RewardIntroBanner bannerPrefab;
    [SerializeField] private Transform bannerContainer;
    [SerializeField] private PieceCardView cardPrefab;
    [SerializeField] private Transform cardContainer;

    private RewardIntroBanner activeBanner;
    private PieceCardView activeCard;
    private PieceCard offeredCard;

    private void OnEnable()
    {
        GameManager.Instance.OnRoundResult += HandleRoundResult;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRoundResult -= HandleRoundResult;

        DespawnCard();
        DespawnBanner();
    }

    private void HandleRoundResult(float apex, float targetHeight, bool success)
    {
        if (!success)
            return;

        offeredCard = cardProvider.GetOfferedCard(LevelManager.Instance.CurrentLevel);
        if (offeredCard == null)
        {
            Debug.LogWarning($"[CardSelectionManager] No card offered for completed level {LevelManager.Instance.CurrentLevel}; advancing without a card.");
            GameManager.Instance.GoToNextLevel();
            return;
        }

        activeBanner = Instantiate(bannerPrefab, bannerContainer);
        activeBanner.IntroCompleted += ShowCard;
    }

    private void ShowCard()
    {
        if (activeBanner != null)
            activeBanner.IntroCompleted -= ShowCard;

        activeCard = Instantiate(cardPrefab, cardContainer);
        activeCard.Populate(offeredCard);
        activeCard.Clicked += HandleCardPicked;
    }

    private void HandleCardPicked()
    {
        LevelManager.Instance.AcquirePiece(offeredCard.PiecePrefab);
        offeredCard = null;

        DespawnCard();
        DismissBanner();
        GameManager.Instance.GoToNextLevel();
    }

    private void DespawnCard()
    {
        if (activeCard == null)
            return;

        activeCard.Clicked -= HandleCardPicked;
        Destroy(activeCard.gameObject);
        activeCard = null;
    }

    private void DismissBanner()
    {
        if (activeBanner == null)
            return;

        activeBanner.IntroCompleted -= ShowCard;
        activeBanner.Dismiss();
        activeBanner = null;
    }

    private void DespawnBanner()
    {
        if (activeBanner == null)
            return;

        activeBanner.IntroCompleted -= ShowCard;
        Destroy(activeBanner.gameObject);
        activeBanner = null;
    }
}
