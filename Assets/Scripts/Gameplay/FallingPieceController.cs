using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Piece))]
public class FallingPieceController : MonoBehaviour
{
    [SerializeField] private float horizontalSpeed = 4f;
    [SerializeField] private float dropSpeed = 2f;
    [SerializeField] private float softDropSpeed = 8f;
    [SerializeField] private float rotateStepDegrees = 90f;
    [SerializeField] private float rotateCooldown = 0.15f;
    [SerializeField] private LayerMask landingMask;
    [SerializeField] private string lockedLayerName = "Locked";

    private Rigidbody2D body2D;
    private Collider2D collider2D;
    private Piece piece;
    private RocketAssembly rocket;
    private float minX = float.NegativeInfinity;
    private float maxX = float.PositiveInfinity;
    private float nextRotateTime;
    private static readonly Collider2D[] OverlapBuffer = new Collider2D[8];

    public System.Action OnLocked;

    private void Awake()
    {
        body2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
        piece = GetComponent<Piece>();
        body2D.bodyType = RigidbodyType2D.Kinematic;
        body2D.useFullKinematicContacts = true;
    }

    public void SetBounds(float min, float max)
    {
        minX = min;
        maxX = max;
    }

    public void SetRocket(RocketAssembly rocketAssembly)
    {
        rocket = rocketAssembly;
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (Time.time >= nextRotateTime &&
            (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame))
        {
            transform.Rotate(0f, 0f, rotateStepDegrees);
            nextRotateTime = Time.time + rotateCooldown;
        }
    }

    private void FixedUpdate()
    {
        if (collider2D.IsTouchingLayers(landingMask))
        {
            LockPiece();
            return;
        }

        var keyboard = Keyboard.current;
        float horizontal = 0f;
        bool softDrop = false;

        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) horizontal -= 1f;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) horizontal += 1f;
            softDrop = keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed;
        }

        float fallSpeed = softDrop ? softDropSpeed : dropSpeed;
        Vector2 delta = new Vector2(horizontal * horizontalSpeed, -fallSpeed) * Time.fixedDeltaTime;
        Vector2 target = body2D.position + delta;
        target.x = Mathf.Clamp(target.x, minX, maxX);

        body2D.MovePosition(target);
    }

    public void ForceLock()
    {
        if (piece.IsLocked) return;
        piece.Lock();
        WeldToContacts();
        body2D.bodyType = RigidbodyType2D.Dynamic;
        int lockedLayer = LayerMask.NameToLayer(lockedLayerName);
        if (lockedLayer >= 0) gameObject.layer = lockedLayer;
        enabled = false;
    }

    private void WeldToContacts()
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(landingMask);
        filter.useTriggers = false;

        int count = Physics2D.OverlapCollider(collider2D, filter, OverlapBuffer);
        if (count == 0) return; // never touched anything - stray piece, stays unconnected

        piece.MarkConnected();

        bool touchedGround = false;
        var welded = new System.Collections.Generic.HashSet<Rigidbody2D>();

        for (int i = 0; i < count; i++)
        {
            Rigidbody2D other = OverlapBuffer[i].attachedRigidbody;
            if (other == null)
            {
                touchedGround = true;
                continue; // resting on static ground - plain collision is enough, no joint
            }
            if (other == body2D) continue;
            if (!welded.Add(other)) continue;
            WeldTo(other);
        }

        if (touchedGround && rocket != null && rocket.PadPiece == null)
        {
            var padJoint = WeldTo(null);
            rocket.SetPadPiece(piece, padJoint);
        }
    }

    private FixedJoint2D WeldTo(Rigidbody2D other)
    {
        var weld = gameObject.AddComponent<FixedJoint2D>();
        weld.autoConfigureConnectedAnchor = true;
        weld.connectedBody = other;
        weld.frequency = 0f;
        return weld;
    }

    private void LockPiece()
    {
        ForceLock();
        OnLocked?.Invoke();
    }
}
