
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

    //Component References (set by PlayerController)
    public PlayerController Controller { get; set; }
    public Rigidbody2D Rb { get; set; }
    public PlayerAnimator Anim { get; set; }
    public PlayerData Data { get; set; }

    //Input Cache (updated every frame)
    public float InputX { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpReleased { get; private set; }

    //Physics State
    public bool IsGrounded { get; set; }
    public bool IsFacingRight { get; set; } = true;
    public bool CanDoubleJump { get; set; }

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
        if (newState == null)
        {
            Debug.LogWarning("[PlayerStateMachine] Tried to change to null state");
            return;
        }

        CurrentState?.Exit(this);
        CurrentState = newState;
        CurrentState.Enter(this);
    }

    
    private void Update()
    {
        CacheInput();
        CurrentState?.Update(this);
        TickTimers();
    }

    private void FixedUpdate() => CurrentState?.FixedUpdate(this);

    private void CacheInput()
    {
        InputX = Input.GetAxisRaw("Horizontal");
        JumpPressed = Input.GetButtonDown("Jump");
        JumpHeld = Input.GetButton("Jump");
        JumpReleased = Input.GetButtonUp("Jump");
    }

    private void TickTimers()
    {
        if (CoyoteTimer > 0) CoyoteTimer -= Time.deltaTime;
        if (JumpBufferTimer > 0) JumpBufferTimer -= Time.deltaTime;
    }

    //Debug
    public string GetCurrentStateName()
        => CurrentState?.GetType().Name ?? "None";
}