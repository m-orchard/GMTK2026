using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Piece : MonoBehaviour
{
    public enum PieceType { Body, Engine, Cargo }

    [SerializeField] private PieceType pieceType = PieceType.Body;
    [SerializeField] private float thrust = 20f;

    public PieceType Type => pieceType;
    public float Thrust => thrust;
    public bool IsLocked { get; private set; }
    public bool IsConnected { get; private set; }
    public Rigidbody2D Body2D { get; private set; }
    public EngineThrustEffect ThrustEffect { get; private set; }

    private void Awake()
    {
        Body2D = GetComponent<Rigidbody2D>();
        if (pieceType == PieceType.Engine) ThrustEffect = gameObject.AddComponent<EngineThrustEffect>();
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
