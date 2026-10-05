using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Horizontal movement speed in units per second.")]
    [SerializeField] private float moveSpeed = 6f;

    [Tooltip("Upward jump velocity/force.")]
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Detection")]
    [Tooltip("Transform position to check if player is touching the ground.")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("Radius around groundCheck to detect ground colliders.")]
    [SerializeField] private float groundCheckRadius = 0.25f;

    [Tooltip("Layer mask representing ground surfaces.")]
    [SerializeField] private LayerMask groundLayer = ~0;

    [Header("Component References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private PlayerPowerUps powerUps;
    private float horizontalInput;
    private bool jumpRequested;
    private bool isGrounded;

    // Animator Parameter Hashes
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int isGroundedLowerHash = Animator.StringToHash("isGrounded");

    private bool hasSpeedParam;
    private bool hasIsJumpingParam;
    private bool hasMoveXParam;
    private bool hasIsGroundedParam;
    private bool hasIsGroundedLowerParam;

    public bool IsGrounded => isGrounded;
    public float HorizontalInput => horizontalInput;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        powerUps = GetComponent<PlayerPowerUps>();

        CacheAnimatorParameters();
    }

    private void OnValidate()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        CacheAnimatorParameters();
    }

    private void Start()
    {
        // If groundCheck transform wasn't assigned in the inspector, find or create one at the feet
        if (groundCheck == null)
        {
            Transform existing = transform.Find("GroundCheck");
            if (existing != null)
            {
                groundCheck = existing;
            }
            else
            {
                GameObject checkObj = new GameObject("GroundCheck");
                checkObj.transform.SetParent(transform);
                checkObj.transform.localPosition = new Vector3(0f, -1.25f, 0f);
                groundCheck = checkObj.transform;
            }
        }

        CheckGroundedStatus();
    }

    public void CacheAnimatorParameters()
    {
        if (animator == null) return;

        hasSpeedParam = false;
        hasIsJumpingParam = false;
        hasMoveXParam = false;
        hasIsGroundedParam = false;
        hasIsGroundedLowerParam = false;

        foreach (var param in animator.parameters)
        {
            if (param.nameHash == SpeedHash) hasSpeedParam = true;
            if (param.nameHash == IsJumpingHash) hasIsJumpingParam = true;
            if (param.nameHash == MoveXHash) hasMoveXParam = true;
            if (param.nameHash == IsGroundedHash) hasIsGroundedParam = true;
            if (param.nameHash == isGroundedLowerHash) hasIsGroundedLowerParam = true;
        }
    }

    private void Update()
    {
        ReadMovementInput();
        UpdateFacingDirection();
        UpdateAnimatorParameters();
    }

    private void FixedUpdate()
    {
        CheckGroundedStatus();
        ApplyHorizontalMovement();
        ApplyJumpPhysics();
    }

    /// <summary>
    /// Reads horizontal input and jump input from both the New Input System and Legacy Input Manager.
    /// </summary>
    private void ReadMovementInput()
    {
        float input = 0f;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input += 1f;
            if (keyboard.spaceKey.wasPressedThisFrame) jumpRequested = true;
        }

        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            float stickX = gamepad.leftStick.x.ReadValue();
            if (Mathf.Abs(stickX) > 0.19f) input = stickX;
            if (gamepad.dpad.left.isPressed) input = -1f;
            if (gamepad.dpad.right.isPressed) input = 1f;
            if (gamepad.buttonSouth.wasPressedThisFrame) jumpRequested = true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        float legacyH = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(legacyH) > 0.01f) input = legacyH;
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space)) jumpRequested = true;
#endif

        horizontalInput = Mathf.Clamp(input, -1f, 1f);
    }

    /// <summary>
    /// Translates left/right input to horizontal velocity along the X-axis.
    /// Preserves existing vertical velocity (rb.linearVelocity.y) while zeroing out micro-jitter when settled on ground/crates.
    /// </summary>
    private void ApplyHorizontalMovement()
    {
        float speed = moveSpeed;
        if (powerUps == null) powerUps = GetComponent<PlayerPowerUps>();
        if (powerUps != null) speed += powerUps.GetMoveSpeedBonus();
        float targetVelocityX = horizontalInput * speed;
        float currentVy = rb.linearVelocity.y;

        // When grounded and not jumping, clamp micro vertical velocity jitter so player stands completely still on crates/ground
        if (isGrounded && !jumpRequested && Mathf.Abs(currentVy) < 0.25f)
        {
            currentVy = 0f;
        }

        rb.linearVelocity = new Vector2(targetVelocityX, currentVy);
    }

    /// <summary>
    /// Executes jump by applying upward velocity along the vertical axis (Vector2.up / Y-axis).
    /// </summary>
    private void ApplyJumpPhysics()
    {
        if (jumpRequested)
        {
            jumpRequested = false;

            if (isGrounded)
            {
                if (powerUps == null) powerUps = GetComponent<PlayerPowerUps>();
                float jump = jumpForce + (powerUps != null ? powerUps.GetJumpBonus() : 0f);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
                isGrounded = false;
                SetGroundedAnimatorParams(false);
            }
        }
    }

    /// <summary>
    /// Determines whether the player's feet are touching a surface, crate, or ground collider.
    /// Uses OverlapBoxAll and a downward BoxCast so the player's own collider never masks underlying surfaces.
    /// </summary>
    private void CheckGroundedStatus()
    {
        Vector2 checkPos = groundCheck != null 
            ? (Vector2)groundCheck.position 
            : (Vector2)transform.position + Vector2.down * 1.25f;

        // Use OverlapBoxAll so the player's own collider never masks underlying ground or crate colliders
        Collider2D[] hits = Physics2D.OverlapBoxAll(checkPos, new Vector2(0.7f, groundCheckRadius * 2f), 0f, groundLayer);
        bool groundDetected = false;
        foreach (var col in hits)
        {
            if (col != null && col.gameObject != gameObject && !col.isTrigger)
            {
                groundDetected = true;
                break;
            }
        }

        // Secondary check: downward box cast to catch surface directly beneath feet
        if (!groundDetected)
        {
            RaycastHit2D castHit = Physics2D.BoxCast(transform.position, new Vector2(0.7f, 0.1f), 0f, Vector2.down, 1.25f, groundLayer);
            if (castHit.collider != null && castHit.collider.gameObject != gameObject && !castHit.collider.isTrigger)
            {
                groundDetected = true;
            }
        }

        if (groundDetected)
        {
            if (rb.linearVelocity.y <= 0.5f)
            {
                isGrounded = true;
                if (Mathf.Abs(rb.linearVelocity.y) < 0.2f && !jumpRequested)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                }
            }
        }
        else
        {
            if (rb.linearVelocity.y < -0.1f || rb.linearVelocity.y > 0.5f)
            {
                isGrounded = false;
            }
        }

        SetGroundedAnimatorParams(isGrounded);
    }

    private void SetGroundedAnimatorParams(bool grounded)
    {
        if (animator == null) return;
        if (hasIsGroundedParam) animator.SetBool(IsGroundedHash, grounded);
        if (hasIsGroundedLowerParam) animator.SetBool(isGroundedLowerHash, grounded);
        if (hasIsJumpingParam) animator.SetBool(IsJumpingHash, !grounded);
    }

    /// <summary>
    /// Updates sprite facing direction so the character faces where they are traveling.
    /// </summary>
    private void UpdateFacingDirection()
    {
        if (horizontalInput > 0.05f)
        {
            // Moving Right -> face right
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
            }
            else
            {
                Vector3 scale = transform.localScale;
                transform.localScale = new Vector3(Mathf.Abs(scale.x), scale.y, scale.z);
            }
        }
        else if (horizontalInput < -0.05f)
        {
            // Moving Left -> flip to face left
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                Vector3 scale = transform.localScale;
                transform.localScale = new Vector3(-Mathf.Abs(scale.x), scale.y, scale.z);
            }
        }
    }

    /// <summary>
    /// Updates the Animator parameters. Sets Speed to the horizontal input / velocity magnitude,
    /// triggering the Walk/Run animation state.
    /// </summary>
    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        if (!hasSpeedParam && !hasIsJumpingParam && !hasIsGroundedLowerParam)
        {
            CacheAnimatorParameters();
        }

        float speedValue = Mathf.Max(Mathf.Abs(horizontalInput), Mathf.Abs(rb.linearVelocity.x));
        if (hasSpeedParam)
        {
            animator.SetFloat(SpeedHash, speedValue);
        }
        if (hasMoveXParam)
        {
            animator.SetFloat(MoveXHash, horizontalInput);
        }
        if (hasIsGroundedParam)
        {
            animator.SetBool(IsGroundedHash, isGrounded);
        }
        if (hasIsGroundedLowerParam)
        {
            animator.SetBool(isGroundedLowerHash, isGrounded);
        }
        if (hasIsJumpingParam)
        {
            animator.SetBool(IsJumpingHash, !isGrounded);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Upward normal indicates contact with ground or crate top from below player feet
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                if (!jumpRequested && rb.linearVelocity.y < 0f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                }
                SetGroundedAnimatorParams(true);
                break;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                if (!jumpRequested && Mathf.Abs(rb.linearVelocity.y) < 0.15f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                }
                SetGroundedAnimatorParams(true);
                break;
            }
        }
    }

    // Programmatic test hooks
    public void SetInput(float horizontal, bool jump)
    {
        horizontalInput = Mathf.Clamp(horizontal, -1f, 1f);
        if (jump) jumpRequested = true;
    }

    public void ManualUpdate()
    {
        UpdateFacingDirection();
        UpdateAnimatorParameters();
    }

    public void ManualFixedUpdate()
    {
        CheckGroundedStatus();
        ApplyHorizontalMovement();
        ApplyJumpPhysics();
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 checkPos = groundCheck != null 
            ? (Vector2)groundCheck.position 
            : (Vector2)transform.position + Vector2.down * 1.25f;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(checkPos, groundCheckRadius);
    }
}
