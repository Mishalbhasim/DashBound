using UnityEngine;
public class PlayerFallState : IPlayerState
{
    public void Enter(PlayerStateMachine sm) => sm.Anim.Play("Fall");

    public void Update(PlayerStateMachine sm)
    {
        // Coyote jump
        if (sm.JumpPressed && sm.CoyoteTimer > 0)
        {
            sm.ChangeState(sm.JumpState);
            return;
        }
        if (sm.JumpPressed && sm.CanDoubleJump)
        {
            sm.ChangeState(sm.DoubleJumpState);
            return;
        }

        // Buffer jump
        if (sm.JumpPressed) sm.JumpBufferTimer = sm.Data.jumpBufferTime;

        if (sm.IsGrounded)
        {
            AudioManager.Instance.PlaySFX("Land");
            sm.ChangeState(sm.InputX != 0 ? (IPlayerState)sm.RunState : sm.IdleState);
        }
    }

    public void FixedUpdate(PlayerStateMachine sm)
    {
        sm.Controller.ApplyMovement(sm.InputX);
        sm.Controller.ApplyFallGravity(true);  
        sm.Controller.ClampFallSpeed();
    }

    public void Exit(PlayerStateMachine sm) { }
}