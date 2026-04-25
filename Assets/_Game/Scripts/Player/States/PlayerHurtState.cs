using UnityEngine;
using System.Collections;
public class PlayerHurtState : IPlayerState
{
    private float hurtTimer;
    private const float HURT_DURATION = 0.4f;

    public void Enter(PlayerStateMachine sm)
    {
        sm.Anim.Play("Hit");
        hurtTimer = HURT_DURATION;
        AudioManager.Instance.PlaySFX("Hurt");

        // Knockback
        float dir = sm.transform.position.x < sm.DamageSourceX ? -1f : 1f;
        sm.Rb.velocity = new Vector2(dir * sm.Data.knockbackForce, sm.Data.knockbackForce * 0.5f);
    }

    public void Update(PlayerStateMachine sm)
    {
        hurtTimer -= Time.deltaTime;
        if (hurtTimer <= 0)
            sm.ChangeState(sm.IsGrounded ? (IPlayerState)sm.IdleState : sm.FallState);
    }

    public void FixedUpdate(PlayerStateMachine sm) { }
    public void Exit(PlayerStateMachine sm) { }
}