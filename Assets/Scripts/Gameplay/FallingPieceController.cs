using DG.Tweening;
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
    [SerializeField] private float rotateTweenDuration = 0.12f;
    [SerializeField] private float rotateOvershoot = 2.2f;
    [SerializeField] private LayerMask landingMask;
    [SerializeField] private string lockedLayerName = "Locked";
    [SerializeField] private float settleDuration = 1f;
    [SerializeField] private float settleLinearSpeedThreshold = 0.3f;
    [SerializeField] private float settleAngularSpeedThreshold = 15f;
    [SerializeField] private float settleDecayRate = 0.5f;
    [SerializeField] private Color lockedTint = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private float wiggleGraceDuration = 0.3f;
    [SerializeField] private float pieceFriction = 0.6f;

    private Rigidbody2D body2D;
    private Collider2D collider2D;
    private SpriteRenderer spriteRenderer;
    private Piece piece;
    private float minX = float.NegativeInfinity;
    private float maxX = float.PositiveInfinity;
    private float lockCeilingY = float.PositiveInfinity;
    private float nextRotateTime;
    private float currentRotationZ;
    private float targetRotationZ;
    private Tween rotateTween;
    private bool released;
    private float settleTimer;
    private float wiggleTimer;
    private static readonly Collider2D[] OverlapBuffer = new Collider2D[8];

    public System.Action OnReleased;
    public System.Action OnLocked;

    private void Awake()
    {
        body2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        piece = GetComponent<Piece>();
        body2D.bodyType = RigidbodyType2D.Kinematic;
        body2D.useFullKinematicContacts = true;
        collider2D.sharedMaterial = new PhysicsMaterial2D("PieceFriction") { friction = pieceFriction };
        currentRotationZ = transform.eulerAngles.z;
        targetRotationZ = currentRotationZ;
    }

    public void SetBounds(float min, float max)
    {
        minX = min;
        maxX = max;
    }

    public void SetLockCeiling(float y)
    {
        lockCeilingY = y;
    }

    private void Update()
    {
        if (released) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (Time.time >= nextRotateTime &&
            (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame))
        {
            RotateStep();
            nextRotateTime = Time.time + rotateCooldown;
        }
    }

    private void RotateStep()
    {
        targetRotationZ += rotateStepDegrees;
        rotateTween?.Kill();
        rotateTween = DOTween
            .To(() => currentRotationZ, ApplyRotationZ, targetRotationZ, rotateTweenDuration)
            .SetEase(Ease.OutBack, rotateOvershoot);
    }

    private void ApplyRotationZ(float rotationZ)
    {
        currentRotationZ = rotationZ;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    private void FixedUpdate()
    {
        if (released)
        {
            TickSettle();
            return;
        }

        bool touchingNow = body2D.position.y < lockCeilingY && CheckContacts() > 0;

        if (touchingNow)
        {
            wiggleTimer += Time.fixedDeltaTime;
            if (wiggleTimer >= wiggleGraceDuration)
            {
                Release();
                return;
            }
        }
        else
        {
            wiggleTimer = 0f;
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

        float fallSpeed = touchingNow ? 0f : (softDrop ? softDropSpeed : dropSpeed);
        Vector2 delta = new Vector2(horizontal * horizontalSpeed, -fallSpeed) * Time.fixedDeltaTime;
        Vector2 target = body2D.position + delta;
        target.x = Mathf.Clamp(target.x, minX, maxX);

        body2D.MovePosition(target);
    }

    public void Release()
    {
        released = true;
        rotateTween?.Kill();
        body2D.bodyType = RigidbodyType2D.Dynamic;
        settleTimer = 0f;

        int lockedLayer = LayerMask.NameToLayer(lockedLayerName);
        if (lockedLayer >= 0) gameObject.layer = lockedLayer;

        OnReleased?.Invoke();
    }

    private void TickSettle()
    {
        if (piece.IsLocked) return;

        bool settled = collider2D.IsTouchingLayers(landingMask)
                     && body2D.linearVelocity.sqrMagnitude <= settleLinearSpeedThreshold * settleLinearSpeedThreshold
                     && Mathf.Abs(body2D.angularVelocity) <= settleAngularSpeedThreshold;

        settleTimer = settled
            ? Mathf.Min(settleDuration, settleTimer + Time.fixedDeltaTime)
            : Mathf.Max(0f, settleTimer - Time.fixedDeltaTime * settleDecayRate);

        if (settleTimer >= settleDuration) FinalizeLock();
    }

    public void ForceLock()
    {
        if (piece.IsLocked) return;
        if (!released) body2D.bodyType = RigidbodyType2D.Dynamic;
        FinalizeLock();
    }

    public void LockIfAtRest()
    {
        if (piece.IsLocked || !released) return;

        bool atRest = collider2D.IsTouchingLayers(landingMask)
                    && body2D.linearVelocity.sqrMagnitude <= settleLinearSpeedThreshold * settleLinearSpeedThreshold
                    && Mathf.Abs(body2D.angularVelocity) <= settleAngularSpeedThreshold;

        if (atRest) FinalizeLock();
    }

    private void FinalizeLock()
    {
        if (piece.IsLocked) return;
        piece.Lock();
        WeldToContacts();
        enabled = false;
        if (spriteRenderer != null) spriteRenderer.color *= lockedTint;
        OnLocked?.Invoke();
    }

    private void OnDestroy()
    {
        rotateTween?.Kill();
    }

    public int CheckContacts()
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(landingMask);
        filter.useTriggers = false;

        return Physics2D.OverlapCollider(collider2D, filter, OverlapBuffer);

    }

    private void WeldToContacts()
    {
        int contactCount = CheckContacts();
        if (contactCount == 0) return; // never touched anything - stray piece, stays unconnected

        piece.MarkConnected();

        var welded = new System.Collections.Generic.HashSet<Rigidbody2D>();

        for (int i = 0; i < contactCount; i++)
        {
            Rigidbody2D other = OverlapBuffer[i].attachedRigidbody;
            if (other == null || other == body2D) continue; // bare scenery (ground/walls) - rest on it, nothing to weld to
            if (!welded.Add(other)) continue;
            if (other.TryGetComponent<Piece>(out var otherPiece)) piece.WeldTo(otherPiece);
        }
    }
}
