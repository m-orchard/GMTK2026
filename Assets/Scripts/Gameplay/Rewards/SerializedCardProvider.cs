using System.Collections.Generic;
using UnityEngine;

public class SerializedCardProvider : CardProvider
{
    [Tooltip("Piece prefab offered as a card when each level is completed, indexed by completed level (element 0 = level 1). Leave an element empty to offer no card for that level.")]
    [SerializeField] private List<GameObject> piecePrefabsByCompletedLevel = new();

    public override PieceCard GetOfferedCard(int completedLevel)
    {
        int index = completedLevel - 1;
        if (index < 0 || index >= piecePrefabsByCompletedLevel.Count)
            return null;

        return PieceCard.FromPrefab(piecePrefabsByCompletedLevel[index]);
    }
}
