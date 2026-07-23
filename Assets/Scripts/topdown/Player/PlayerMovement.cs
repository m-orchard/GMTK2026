using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in units per second.")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;

    [Tooltip("If true, movement input is read raw for snappy, instant direction changes. If false, input is smoothed.")]
    [SerializeField] private bool useRawInput = true;

    [Header("Aiming")]
    [Tooltip("Should the player rotate to face the mouse pointer?")]
    [SerializeField] private bool faceMouse = true;

    [Tooltip("The child graphics to rotate when aiming. The root is left unrotated so child UI stays upright.")]
    [SerializeField] private Transform graphics;

    [Tooltip("Degrees to offset the facing rotation. Use -90 when the sprite's 'forward' points up.")]
    [SerializeField] private float rotationOffset = -90f;

    private Rigidbody2D playerRigidbody;
    private Knockback knockback;
    private Camera mainCamera;
    private Vector2 movementInput;

    public Vector2 FacingDirection => graphics.up;
    public Vector2 MovementInput => movementInput;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        knockback = GetComponent<Knockback>();
        mainCamera = Camera.main;

        if (graphics == null)
        {
            graphics = transform;
        }
    }

    private void Update()
    {
        movementInput = ReadMovementInput();

        if (faceMouse && mainCamera != null)
        {
            AimAtMouse();
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private Vector2 ReadMovementInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            float horizontal = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
            float vertical = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
            return new Vector2(horizontal, vertical);
        }
#endif
        float horizontalAxis = useRawInput ? Input.GetAxisRaw("Horizontal") : Input.GetAxis("Horizontal");
        float verticalAxis = useRawInput ? Input.GetAxisRaw("Vertical") : Input.GetAxis("Vertical");
        return new Vector2(horizontalAxis, verticalAxis);
    }

    private void Move()
    {
        Vector2 direction = ClampToUnitLength(movementInput);
        playerRigidbody.linearVelocity = direction * moveSpeed + KnockbackVelocity();
    }

    private Vector2 KnockbackVelocity()
    {
        if (knockback == null)
        {
            return Vector2.zero;
        }

        Vector2 knockbackVelocity = knockback.Velocity;
        knockback.Decay(Time.fixedDeltaTime);
        return knockbackVelocity;
    }

    private Vector2 ClampToUnitLength(Vector2 vector)
    {
        return vector.sqrMagnitude > 1f ? vector.normalized : vector;
    }

    private void AimAtMouse()
    {
        Vector3 mouseScreenPosition = ReadMouseScreenPosition();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = transform.position.z;

        Vector3 directionToMouse = mouseWorldPosition - transform.position;
        if (directionToMouse.sqrMagnitude < 0.001f)
        {
            return;
        }

        float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg + rotationOffset;
        graphics.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private Vector3 ReadMouseScreenPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif
        return Input.mousePosition;
    }
}
