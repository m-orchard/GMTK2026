using UnityEngine;

public class CardSelectionManager : MonoBehaviour
{
    [SerializeField] private CardProvider cardProvider;
    [SerializeField] private PieceCardView cardPrefab;
    [SerializeField] private Transform cardContainer;

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

        DespawnActiveCard();
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

        activeCard = Instantiate(cardPrefab, cardContainer);
        activeCard.Populate(offeredCard);
        activeCard.Clicked += HandleCardPicked;
    }

    private void HandleCardPicked()
    {
        LevelManager.Instance.AcquirePiece(offeredCard.PiecePrefab);
        offeredCard = null;

        DespawnActiveCard();
        GameManager.Instance.GoToNextLevel();
    }

    private void DespawnActiveCard()
    {
        if (activeCard == null)
            return;

        activeCard.Clicked -= HandleCardPicked;
        Destroy(activeCard.gameObject);
        activeCard = null;
    }
}
