using UnityEngine;
public class PlayerRunState : IPlayerState
{
    public void Enter(PlayerStateMachine sm)
    {
        sm.Anim.Play("Run");
        AudioManager.Instance.PlaySFX("Run");

       
    }

    public void Update(PlayerStateMachine sm)
    {
        if (sm.JumpPressed || sm.JumpBufferTimer > 0)
        {
            sm.ChangeState(sm.JumpState);
            return;
        }
        if (!sm.IsGrounded)
        {
            sm.CoyoteTimer = sm.Data.coyoteTime;
            sm.ChangeState(sm.FallState);
            return;
        }
        if (sm.InputX == 0)
        {
            sm.ChangeState(sm.IdleState);
        }
    }

    public void FixedUpdate(PlayerStateMachine sm)
    {
        sm.Controller.ApplyMovement(sm.InputX);
    }

    public void Exit(PlayerStateMachine sm)
    {
        
    }
}