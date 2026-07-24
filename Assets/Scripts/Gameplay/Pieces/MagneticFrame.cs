using UnityEngine;

[RequireComponent(typeof(Piece))]
[RequireComponent(typeof(Collider2D))]
public class MagneticFrame : MonoBehaviour
{
    private Piece piece;
    private Collider2D collider;

    void Awake()
    {
        piece = GetComponent<Piece>();
        collider = GetComponentInChildren<Collider2D>();
    }

    void Update()
    {
        var activeController = PieceSpawner.Instance.Active;
        if (activeController == null)
        {
            return;
        }

        var activeCollider = activeController.GetComponent<Collider2D>();
        if (!Physics2D.Distance(collider, activeCollider).isOverlapped)
        {
            return;
        }

        Debug.Log(piece.IsLocked
            ? "Magnetic Frame: Found collision with falling piece; locking"
            : "Magnetic Frame: Found collision with locked piece; locking");
        activeController.Release();
        activeController.ForceLock();
    }
}
