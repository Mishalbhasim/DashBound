
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerData data;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundMask;

    //[Header("Particles (optional)")]
    //[SerializeField] private ParticleSystem jumpParticle;
    //[SerializeField] private ParticleSystem landParticle;
    //[SerializeField] private ParticleSystem runDustParticle;

    //Components
    private Rigidbody2D rb;
    private PlayerStateMachine sm;
    private PlayerAnimator anim;
    private PlayerHealth health;
    private SpriteRenderer sr;

    // Power-Up State
    private float speedMultiplier = 1f;
    private bool hasDoubleJump = false;
    private Coroutine powerUpRoutine = null;

    // Ground State (cached to avoid double land events)
    private bool wasGrounded = false;

  
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<PlayerAnimator>();
        health = GetComponent<PlayerHealth>();
        sr = GetComponentInChildren<SpriteRenderer>();
        sm = GetComponent<PlayerStateMachine>();

        // Link state machine references
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
        if (LevelManager.Instance != null)
            transform.position = LevelManager.Instance.GetSpawnPosition();

        sm.Initialize(sm.IdleState);

        // Fire initial UI events so HUD shows correct values
        UIEvents.OnLivesChanged?.Invoke(
            GameManager.Instance != null ? GameManager.Instance.Lives : 3
        );
    }

    private void Update()
    {
        sm.IsGrounded = CheckGrounded();

        // Reset coyote timer while grounded
        if (sm.IsGrounded)
            sm.CoyoteTimer = data.coyoteTime;

        // Land particle — fires once on landing
        //if (sm.IsGrounded && !wasGrounded)
        //    OnLanded();

        wasGrounded = sm.IsGrounded;

        HandleFlip();
        //HandleRunDust();
    }

    //MOVEMENT
    public void ApplyMovement(float direction)
    {
        float targetSpeed = direction * data.moveSpeed * speedMultiplier;

        float accel = sm.IsGrounded
            ? (Mathf.Abs(direction) > 0.01f ? data.acceleration : data.deceleration)
            : data.airAcceleration;

        float newVelX = Mathf.MoveTowards(
            rb.velocity.x,
            targetSpeed,
            accel * Time.fixedDeltaTime
        );

        rb.velocity = new Vector2(newVelX, rb.velocity.y);
    }

    public void ApplyFallGravity(bool isFalling)
    {
        if (!isFalling) return;
        rb.velocity += Vector2.up
            * Physics2D.gravity.y
            * (data.fallGravityMultiplier - 1f)
            * Time.fixedDeltaTime;
    }

    public void ClampFallSpeed()
    {
        if (rb.velocity.y < -data.maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, -data.maxFallSpeed);
    }

    //GROUND CHECK
    private bool CheckGrounded()
    {
        if (groundCheckPoint == null) return false;
        return Physics2D.OverlapCircle(
            groundCheckPoint.position,
            data.groundCheckRadius,
            groundMask
        );
    }

    //FLIP
    private void HandleFlip()
    {
        float input = Input.GetAxisRaw("Horizontal");
        if (input > 0 && !sm.IsFacingRight) Flip();
        else if (input < 0 && sm.IsFacingRight) Flip();
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

    //PARTICLES
    //public void PlayJumpParticle()
    //{
    //    jumpParticle?.Play();
    //}

    //private void OnLanded()
    //{
    //    landParticle?.Play();
    //}

    //private void HandleRunDust()
    //{
    //    if (runDustParticle == null) return;

    //    bool shouldPlay = sm.IsGrounded
    //        && Mathf.Abs(rb.velocity.x) > 0.5f
    //        && sm.CurrentState == sm.RunState;

    //    if (shouldPlay && !runDustParticle.isPlaying)
    //        runDustParticle.Play();
    //    else if (!shouldPlay && runDustParticle.isPlaying)
    //        runDustParticle.Stop();
    //}

    //POWER-UPS
    public void ApplyPowerUp(PowerUpData powerUp)
    {
        if (powerUp == null) return;

        // Stop any existing power-up
        if (powerUpRoutine != null)
            StopCoroutine(powerUpRoutine);

        powerUpRoutine = StartCoroutine(PowerUpRoutine(powerUp));
        AudioManager.Instance?.PlaySFX("CollectPowerUp");
        UIEvents.OnPowerUpCollected?.Invoke(powerUp);
    }

    private IEnumerator PowerUpRoutine(PowerUpData powerUp)
    {
        // Apply power-up effect
        switch (powerUp.type)
        {
            case PowerUpType.SpeedBoost:
                speedMultiplier = powerUp.multiplier;
                break;
            case PowerUpType.DoubleJump:
                hasDoubleJump = true;
                sm.CanDoubleJump = true;
                break;
            case PowerUpType.Invincibility:
                health?.SetInvincible(true);
                break;
            case PowerUpType.Shield:
                health?.ActivateShield();
                yield break;     // shield has no duration — just apply and done
            case PowerUpType.ExtraLife:
                GameManager.Instance?.GainLife();
                yield break;     // instant effect
        }

        // Wait for duration
        yield return new WaitForSeconds(powerUp.duration);

        // Remove effect
        switch (powerUp.type)
        {
            case PowerUpType.SpeedBoost:
                speedMultiplier = 1f;
                break;
            case PowerUpType.DoubleJump:
                hasDoubleJump = false;
                break;
            case PowerUpType.Invincibility:
                health?.SetInvincible(false);
                break;
        }

        powerUpRoutine = null;
        UIEvents.OnPowerUpCollected?.Invoke(null); // clear HUD icon
    }

    //COLLISION
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Finish"))
            GameManager.Instance?.LevelComplete();
    }

    //GIZMOS
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null || data == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckPoint.position, data.groundCheckRadius);
    }

    // GETTER
    public PlayerData GetData() => data;
}