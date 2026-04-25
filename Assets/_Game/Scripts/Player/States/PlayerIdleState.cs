using UnityEngine;
public class PlayerIdleState : IPlayerState
{
    public void Enter(PlayerStateMachine sm)
    {
        sm.Anim.Play("Idle");
        sm.Rb.velocity = new UnityEngine.Vector2(0, sm.Rb.velocity.y);

         
    }

    public void Update(PlayerStateMachine sm)
    {
        if (sm.JumpBufferTimer > 0 && sm.IsGrounded)
        {
            sm.ChangeState(sm.JumpState);
            return;
        }
        if (sm.JumpPressed && sm.CoyoteTimer > 0)
        {
            sm.ChangeState(sm.JumpState);
            return;
        }
        if (!sm.IsGrounded)
        {
            sm.ChangeState(sm.FallState);
            return;
        }
        if (sm.InputX != 0)
        {
            sm.ChangeState(sm.RunState);
        }
    }

    public void FixedUpdate(PlayerStateMachine sm) { }
    public void Exit(PlayerStateMachine sm) { }
}

