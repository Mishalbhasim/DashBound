using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{

    


    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 18f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMult = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Coyote + Buffer")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;


    [Header("Wall Slide")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.3f;
    private static readonly int HashWallSlide = Animator.StringToHash("IsWallSliding");

    [Header("Double Jump")]
    [SerializeField] private bool canDoubleJump = true;
    private bool _hasDoubleJump;
    private static readonly int HashDoubleJump = Animator.StringToHash("DoubleJump");


    private bool _isWallSliding;
    // ── Components ────────────────────────────────────────────────────
    private Rigidbody2D _rb;
    private Animator _anim;

    // ── State ─────────────────────────────────────────────────────────
    private float _inputX;
    private bool _isGrounded;
    private bool _facingRight = true;
    private float _coyoteTimer;
    private float _jumpBuffer;

    // ── Animator Hashes ───────────────────────────────────────────────
    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashVelY = Animator.StringToHash("VelocityY");
    private static readonly int HashGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int HashJump = Animator.StringToHash("Jump");

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        _rb.velocity = Vector2.zero;
    }

    private void Update()
    {
        GatherInput();
        UpdateTimers();
        HandleJump();
        HandleWallSlide();
        FlipSprite();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        CheckGround();
        ApplyMovement();
        ApplyBetterGravity();
    }

    // ── Input ─────────────────────────────────────────────────────────
    private void GatherInput()
    {
        // Keyboard only — no joystick drift
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            _inputX = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            _inputX = 1f;
        else
            _inputX = 0f;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
            _jumpBuffer = jumpBufferTime;
    }

    // ── Timers ────────────────────────────────────────────────────────
    private void UpdateTimers()
    {
        _coyoteTimer -= Time.deltaTime;
        _jumpBuffer -= Time.deltaTime;
    }

    // ── Ground Check ──────────────────────────────────────────────────
    private void CheckGround()
    {
        _isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer);

        if (_isGrounded)
            _coyoteTimer = coyoteTime;
    }

    // ── Movement ──────────────────────────────────────────────────────
    private void ApplyMovement()
    {
        _rb.velocity = new Vector2(
            _inputX * moveSpeed,
            _rb.velocity.y);
    }

    // ── Jump ──────────────────────────────────────────────────────────
    private void HandleJump()
    {
        // Reset double jump when grounded
        if (_isGrounded)
            _hasDoubleJump = canDoubleJump;

        if (_jumpBuffer > 0f)
        {
            // Normal jump
            if (_coyoteTimer > 0f)
            {
                PerformJump();
                _coyoteTimer = 0f;
            }
            // Double jump
            else if (_hasDoubleJump)
            {
                PerformDoubleJump();
            }
        }
    }

    private void PerformJump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
        _jumpBuffer = 0f;
        _anim.SetTrigger(HashJump);
    }

    private void PerformDoubleJump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
        _jumpBuffer = 0f;
        _hasDoubleJump = false;
        _anim.SetTrigger(HashDoubleJump);
    }

    // wall slide

    private void HandleWallSlide()
    {
        bool touchingWall = Physics2D.Raycast(
            wallCheck.position,
            _facingRight ? Vector2.right : Vector2.left,
            wallCheckDistance,
            groundLayer);

        _isWallSliding = touchingWall && !_isGrounded && _inputX != 0;

        if (_isWallSliding)
        {
            _rb.velocity = new Vector2(
                _rb.velocity.x,
                Mathf.Clamp(_rb.velocity.y, -wallSlideSpeed, float.MaxValue));
        }

        _anim.SetBool(HashWallSlide, _isWallSliding);
    }

    // ── Better Gravity ────────────────────────────────────────────────
    private void ApplyBetterGravity()
    {
        if (_rb.velocity.y < 0)
        {
            _rb.velocity += Vector2.up * Physics2D.gravity.y
                * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (_rb.velocity.y > 0 &&
                 !Input.GetKey(KeyCode.Space) &&
                 !Input.GetKey(KeyCode.W))
        {
            _rb.velocity += Vector2.up * Physics2D.gravity.y
                * (lowJumpMult - 1) * Time.fixedDeltaTime;
        }
    }

    // ── Flip ──────────────────────────────────────────────────────────
    private void FlipSprite()
    {
        if (_inputX > 0 && !_facingRight) Flip();
        else if (_inputX < 0 && _facingRight) Flip();
    }

    private void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ── Animator ──────────────────────────────────────────────────────
    private void UpdateAnimator()
    {
        _anim.SetFloat(HashSpeed, Mathf.Abs(_rb.velocity.x));
        _anim.SetFloat(HashVelY, _rb.velocity.y);
        _anim.SetBool(HashGrounded, _isGrounded);
    }

    // ── Gizmos ────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius);
    }
}