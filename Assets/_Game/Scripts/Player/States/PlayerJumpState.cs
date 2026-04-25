using UnityEngine;
public class PlayerJumpState : IPlayerState
{
    public void Enter(PlayerStateMachine sm)
    {
        sm.Anim.Play("Jump");
        sm.JumpBufferTimer = 0;

        // Apply jump force
        sm.Rb.velocity = new Vector2(sm.Rb.velocity.x, sm.Data.jumpForce);
        sm.CanDoubleJump = true;
        AudioManager.Instance.PlaySFX("Jump");
    }

    public void Update(PlayerStateMachine sm)
    {
        

        if (sm.JumpPressed && sm.CanDoubleJump)
        {
            sm.ChangeState(sm.DoubleJumpState);
            return;
        }

        if (sm.Rb.velocity.y < 0)
        {
            sm.ChangeState(sm.FallState);
        }
    }

    public void FixedUpdate(PlayerStateMachine sm)
    {
        sm.Controller.ApplyMovement(sm.InputX);
        sm.Controller.ApplyFallGravity(false);
    }

    public void Exit(PlayerStateMachine sm) { }
}