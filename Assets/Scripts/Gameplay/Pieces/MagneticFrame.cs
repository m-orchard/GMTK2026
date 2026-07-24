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
        if (!piece.IsLocked)
        {
            return;
        }

        var activeController = PieceSpawner.Instance.Active;
        if (activeController == null)
        {
            return;
        }

        var activeCollider = activeController.GetComponent<Collider2D>();
        if (collider.IsTouching(activeCollider))
        {
            Debug.Log($"Magnetic Frame: Found collision with falling piece; locking");
            activeController.Release();
            activeController.ForceLock();
        }
    }
}
