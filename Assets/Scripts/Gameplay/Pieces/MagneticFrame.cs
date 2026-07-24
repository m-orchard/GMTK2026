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

        if (activeController.gameObject == piece.gameObject)
        {
            // this magnetic frame is itself the falling piece - it should snap to
            // anything it touches instantly, skipping the normal wiggle/settle flow.
            if (activeController.CheckContacts() > 0)
            {
                Debug.Log("Magnetic Frame: Falling piece is magnetic; locking instantly");
                activeController.ForceLock();
            }
            return;
        }

        var activeCollider = activeController.GetComponent<Collider2D>();
        if (!Physics2D.Distance(collider, activeCollider).isOverlapped)
        {
            return;
        }

        Debug.Log(piece.IsLocked
            ? "[MagneticFrame]: Found collision with falling piece; locking"
            : "[MagneticFrame]: Found collision with locked piece; locking");
        activeController.Release();
        activeController.ForceLock();
    }
}
