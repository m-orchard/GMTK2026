using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Piece))]
public class FallingPieceController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float horizontalSpeed = 4f;

    [Header("Rotation")]
    [SerializeField] private float rotateStepDegrees = 90f;
    [SerializeField] private float rotateCooldown = 0.15f;
    [SerializeField] private float rotateTweenDuration = 0.12f;
    [SerializeField] private float rotateOvershoot = 2.2f;

    [Header("Landing & Locking")]
    [SerializeField] private LayerMask landingMask;
    [SerializeField] private string lockedLayerName = "Locked";
    [SerializeField] private Color lockedTint = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private float wiggleGraceDuration = 0.3f;
    [SerializeField] private float pieceFriction = 0.6f;

    [Header("Settling")]
    [SerializeField] private float settleDuration = 1f;
    [SerializeField] private float settleLinearSpeedThreshold = 0.3f;
    [SerializeField] private float settleAngularSpeedThreshold = 15f;
    [SerializeField] private float settleDecayRate = 0.5f;

    [Header("Experimental Controls")]
    [SerializeField] private bool tapToMoveHorizontally = false;
    [SerializeField] private float blockSize = 1f;
    [SerializeField] private float tapMoveStepInBlocks = 0.5f;
    [SerializeField] private float stepMoveTweenDuration = 0.05f;

    private float TapMoveStepDistance => blockSize * tapMoveStepInBlocks;

    [Header("Drop Speed")]
    [SerializeField] private bool acceleratingSoftDrop = false;
    [FormerlySerializedAs("dropSpeed")]
    [SerializeField] private float normalDropSpeed = 2f;
    [FormerlySerializedAs("softDropSpeed")]
    [SerializeField] private float maxDropSpeed = 8f;
    [SerializeField] private float dropAcceleration = 20f;
    [SerializeField] private float dropDeceleration = 20f;

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
    private float currentPositionX;
    private float targetPositionX;
    private Tween horizontalTween;
    private float currentDropSpeed;
    private static readonly Collider2D[] OverlapBuffer = new Collider2D[8];

    public System.Action OnReleased;
    public System.Action OnLocked;

    public bool Released => released;
    public Collider2D LandingCollider => collider2D;
    public LayerMask LandingMask => landingMask;

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
        currentDropSpeed = normalDropSpeed;
        currentPositionX = transform.position.x;
        targetPositionX = currentPositionX;
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

    public void SnapToMovementStep()
    {
        if (!tapToMoveHorizontally) return;

        float step = TapMoveStepDistance;
        if (step <= 0f) return;

        currentPositionX = body2D.position.x;
        TweenHorizontalTo(SnapToStepGrid(body2D.position.x, step));
    }

    private void TweenHorizontalTo(float destinationX)
    {
        targetPositionX = Mathf.Clamp(destinationX, minX, maxX);
        horizontalTween?.Kill();
        horizontalTween = DOTween
            .To(() => currentPositionX, positionX => currentPositionX = positionX, targetPositionX, stepMoveTweenDuration)
            .SetEase(Ease.OutQuad);
    }

    private float SnapToStepGrid(float value, float step)
    {
        float gridOrigin = 0f;
        if (!float.IsInfinity(minX) && !float.IsInfinity(maxX)) gridOrigin = (minX + maxX) * 0.5f;

        return gridOrigin + Mathf.Round((value - gridOrigin) / step) * step;
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

        if (tapToMoveHorizontally)
        {
            int stepDirection = 0;
            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) stepDirection -= 1;
            if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) stepDirection += 1;
            if (stepDirection != 0) TweenHorizontalTo(targetPositionX + stepDirection * TapMoveStepDistance);
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
        bool softDrop = keyboard != null && (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed);

        float fallSpeed = ResolveDropSpeed(softDrop, touchingNow);

        Vector2 target = body2D.position;
        target.y -= fallSpeed * Time.fixedDeltaTime;
        target.x = tapToMoveHorizontally
            ? currentPositionX
            : target.x + ReadContinuousHorizontalDelta(keyboard);
        target.x = Mathf.Clamp(target.x, minX, maxX);

        body2D.MovePosition(target);
    }

    private float ResolveDropSpeed(bool softDropHeld, bool touchingNow)
    {
        if (touchingNow) return 0f;

        float targetDropSpeed = softDropHeld ? maxDropSpeed : normalDropSpeed;

        if (!acceleratingSoftDrop)
        {
            currentDropSpeed = targetDropSpeed;
            return currentDropSpeed;
        }

        float rate = targetDropSpeed > currentDropSpeed ? dropAcceleration : dropDeceleration;
        currentDropSpeed = Mathf.MoveTowards(currentDropSpeed, targetDropSpeed, rate * Time.fixedDeltaTime);
        return currentDropSpeed;
    }

    private float ReadContinuousHorizontalDelta(Keyboard keyboard)
    {
        if (keyboard == null) return 0f;

        float direction = 0f;
        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) direction -= 1f;
        if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) direction += 1f;
        return direction * horizontalSpeed * Time.fixedDeltaTime;
    }

    public void Release()
    {
        released = true;
        rotateTween?.Kill();
        horizontalTween?.Kill();
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
        horizontalTween?.Kill();
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
