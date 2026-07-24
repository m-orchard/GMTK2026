using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Piece : MonoBehaviour
{
    public bool IsLocked { get; private set; }
    public bool IsConnected { get; private set; }
    public Rigidbody2D Body2D { get; private set; }

    private void Awake()
    {
        Body2D = GetComponent<Rigidbody2D>();
    }

    public void Lock()
    {
        IsLocked = true;
    }

    public void MarkConnected()
    {
        IsConnected = true;
    }
}
