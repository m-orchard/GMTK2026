using UnityEngine;

public abstract class CardProvider : MonoBehaviour
{
    public abstract PieceCard GetOfferedCard(int completedLevel);
}
