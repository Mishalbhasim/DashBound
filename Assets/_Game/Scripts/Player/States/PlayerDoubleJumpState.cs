using UnityEngine;
public class PlayerDoubleJumpState : IPlayerState
{
    public void Enter(PlayerStateMachine sm)
    {
        sm.Anim.Play("DoubleJump");
        sm.Rb.velocity = new Vector2(sm.Rb.velocity.x, sm.Data.doubleJumpForce);
        sm.CanDoubleJump = false;
        AudioManager.Instance.PlaySFX("DoubleJump");
    }

    public void Update(PlayerStateMachine sm)
    {
        
        if (sm.Rb.velocity.y < 0) sm.ChangeState(sm.FallState);
        if (sm.IsGrounded) sm.ChangeState(sm.IdleState);
    }

    public void FixedUpdate(PlayerStateMachine sm)
    {
        sm.Controller.ApplyMovement(sm.InputX);
    }

    public void Exit(PlayerStateMachine sm) { }
}