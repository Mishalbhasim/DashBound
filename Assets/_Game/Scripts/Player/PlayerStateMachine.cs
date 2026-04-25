using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    //States
    public PlayerIdleState IdleState = new PlayerIdleState();
    public PlayerRunState RunState = new PlayerRunState();
    public PlayerJumpState JumpState = new PlayerJumpState();
    public PlayerDoubleJumpState DoubleJumpState = new PlayerDoubleJumpState();
    public PlayerFallState FallState = new PlayerFallState();
    public PlayerHurtState HurtState = new PlayerHurtState();
    public PlayerDeadState DeadState = new PlayerDeadState();

    //Current State
    public IPlayerState CurrentState { get; private set; }

    //Component Reference
    public PlayerController Controller { get; set; }
    public Rigidbody2D Rb { get; set; }
    public PlayerAnimator Anim { get; set; }
    public PlayerData Data { get; set; }

    //Input Cache
    public float InputX { get; set; }
    public bool JumpPressed { get; set; }
    public bool JumpHeld { get; set; }
    public bool JumpReleased { get; set; }

    //Physics State
    public bool IsGrounded { get; set; }
    public bool IsFacingRight { get; set; } = true;
    public bool CanDoubleJump { get; set; }

    public float DamageSourceX { get; set; }

    //Timers
    public float CoyoteTimer { get; set; }
    public float JumpBufferTimer { get; set; }

    public void Initialize(IPlayerState startState)
    {
        CurrentState = startState;
        CurrentState.Enter(this);
    }

    public void ChangeState(IPlayerState newState)
    {
        CurrentState?.Exit(this);
        CurrentState = newState;
        CurrentState.Enter(this);
    }

    private void Update()
    {
        CacheInput();
        CurrentState?.Update(this);
        UpdateTimers();
    }

    private void FixedUpdate() => CurrentState?.FixedUpdate(this);

    private void CacheInput()
    {
        InputX = Input.GetAxisRaw("Horizontal");
        JumpPressed = Input.GetButtonDown("Jump");
        JumpHeld = Input.GetButton("Jump");
        JumpReleased = Input.GetButtonUp("Jump");

        
    }

    private void UpdateTimers()
    {
        if (CoyoteTimer > 0) CoyoteTimer -= Time.deltaTime;
        if (JumpBufferTimer > 0) JumpBufferTimer -= Time.deltaTime;
    }
}