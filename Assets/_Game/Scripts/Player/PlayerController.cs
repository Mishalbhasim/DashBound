using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{



    [SerializeField] private Transform startPoint;


    [Header("Data")]
    [SerializeField] private PlayerData data;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundMask;

    // Components
    private Rigidbody2D rb;
    private PlayerStateMachine sm;
    private PlayerAnimator anim;
    private PlayerHealth health;
    private SpriteRenderer sr;

    
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<PlayerAnimator>();
        health = GetComponent<PlayerHealth>();
        sr = GetComponentInChildren<SpriteRenderer>();
        sm = GetComponent<PlayerStateMachine>();
        

        // Link state machine
        sm.Controller = this;
        sm.Rb = rb;
        sm.Anim = anim;
        sm.Data = data;

        // Physics setup
        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Start()
    {
        transform.position = LevelManager.Instance.GetSpawnPosition();
        sm.Initialize(sm.IdleState);
    }

    private void Update()
    {
        

        sm.IsGrounded = CheckGrounded();
        


        //reset coyoteTimer if grounded
        if (sm.IsGrounded)
        {
            sm.CoyoteTimer = data.coyoteTime;
        }

        HandleFlip();
    }

    //Movement  
    public void ApplyMovement(float direction)
    {
        float targetSpeed = direction * data.moveSpeed ;

        float accel = sm.IsGrounded ?
            (Mathf.Abs(direction) > 0.01f ? data.acceleration : data.deceleration) :
            data.airAcceleration;

        float newVelX = Mathf.MoveTowards(rb.velocity.x, targetSpeed, accel * Time.fixedDeltaTime);

        rb.velocity = new Vector2(newVelX, rb.velocity.y);
    }

    public void ApplyFallGravity(bool isFalling)
    {
        if (isFalling)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (data.fallGravityMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    public void ClampFallSpeed()
    {
        if (rb.velocity.y < -data.maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, -data.maxFallSpeed);
    }

    
    private bool CheckGrounded()
    {
        return Physics2D.OverlapCircle(groundCheckPoint.position, data.groundCheckRadius, groundMask);  
    }

    
    private void HandleFlip()
    {
        float input = Input.GetAxisRaw("Horizontal");
        if (input > 0 && !sm.IsFacingRight) Flip();
        if (input < 0 && sm.IsFacingRight) Flip();
    }

    private void Flip()
    {
        sm.IsFacingRight = !sm.IsFacingRight;
        transform.localScale = new Vector3(
            -transform.localScale.x,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    
    public void ApplyPowerUp(PowerUpData powerUp)
    {
        StopAllCoroutines();
       
    }

    

    //COLLISION
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Finish"))
        {
            GameManager.Instance.LevelComplete();
        }
        
    }

    public void RespawnAtStart()
    {
        if (startPoint != null)
        {
            transform.position = startPoint.position;
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, data.groundCheckRadius);
        }
    }

    //      GETTER
    public PlayerData GetData()
    {
        return data;
    }
}