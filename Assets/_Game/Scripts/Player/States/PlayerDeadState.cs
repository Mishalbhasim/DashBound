using UnityEngine;
using System.Collections;

public class PlayerDeadState : IPlayerState
{
    private float deathTimer = 1.5f;
    private bool calledGameOver = false;

    public void Enter(PlayerStateMachine sm)
    {
        sm.Anim.Play("Death");
        sm.Rb.velocity = Vector2.zero;
        sm.Rb.gravityScale = 0;

        // ── REMOVED: sm.enabled = false ──────────────────────────────
        // This was stopping Update() from running so TriggerGameOver()
        // was never called. The state machine MUST stay enabled.
        // ─────────────────────────────────────────────────────────────

        AudioManager.Instance?.PlaySFX("Die");
        deathTimer = 1.5f;
        calledGameOver = false;

        // Disable player input and collider instead
        var col = sm.Controller.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public void Update(PlayerStateMachine sm)
    {
        // This now RUNS correctly because sm.enabled is no longer false
        deathTimer -= Time.unscaledDeltaTime;

        if (deathTimer <= 0 && !calledGameOver)
        {
            calledGameOver = true;
            Debug.Log("[PlayerDeadState] Triggering Game Over");
            GameManager.Instance?.TriggerGameOver();
        }
    }

    public void FixedUpdate(PlayerStateMachine sm) { }

    public void Exit(PlayerStateMachine sm)
    {
        sm.Rb.gravityScale = 1;

        // Re-enable collider
        var col = sm.Controller.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }
}