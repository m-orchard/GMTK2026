using System.Collections.Generic;
using UnityEngine;

public abstract class CardProvider : MonoBehaviour
{
    public abstract List<PieceCard> GetOfferedCards();
}
