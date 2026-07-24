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

        if (piece.IsLocked)
        {
            var activeCollider = activeController.GetComponent<Collider2D>();
            if (piece.IsLocked && collider.IsTouching(activeCollider))
            {
                Debug.Log($"Magnetic Frame: Found collision with falling piece; locking");
                activeController.Release();
                activeController.ForceLock();
            }
        } else if (activeController.CheckContacts() > 0)
        {
            Debug.Log($"Magnetic Frame: Found collision with locked piece; locking");
            activeController.Release();
            activeController.ForceLock();
        }
    }
}
