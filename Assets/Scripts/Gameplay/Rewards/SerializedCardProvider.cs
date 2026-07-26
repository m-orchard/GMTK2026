using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class SerializedCardProvider : CardProvider
{
    [Tooltip("Every piece prefab that can be offered as a card. A prefab already selected in a previous level is never offered again.")]
    [FormerlySerializedAs("piecePrefabsByCompletedLevel")]
    [SerializeField] private List<GameObject> availablePiecePrefabs = new();

    [Tooltip("When enabled, offered cards are drawn randomly from the remaining pool instead of in list order.")]
    [SerializeField] private bool selectRandomly = false;

    [Tooltip("How many cards to offer as options each time. Fewer are shown when fewer prefabs remain available.")]
    [Range(1, 5)]
    [SerializeField] private int cardsToOffer = 1;

    public override List<PieceCard> GetOfferedCards()
    {
        List<GameObject> availablePool = BuildAvailablePool();
        int offerCount = Mathf.Min(cardsToOffer, availablePool.Count);

        List<PieceCard> offeredCards = new();
        for (int offered = 0; offered < offerCount; offered++)
        {
            int poolIndex = selectRandomly ? Random.Range(0, availablePool.Count) : 0;
            offeredCards.Add(PieceCard.FromPrefab(availablePool[poolIndex]));
            availablePool.RemoveAt(poolIndex);
        }

        return offeredCards;
    }

    private List<GameObject> BuildAvailablePool()
    {
        IReadOnlyList<GameObject> acquiredPieces = LevelManager.Instance.AcquiredPieces;

        List<GameObject> availablePool = new();
        foreach (GameObject piecePrefab in availablePiecePrefabs)
        {
            if (piecePrefab == null)
                continue;
            if (acquiredPieces.Contains(piecePrefab))
                continue;

            availablePool.Add(piecePrefab);
        }

        return availablePool;
    }
}
