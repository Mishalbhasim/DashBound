using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    private PlayerData data;
    private PlayerStateMachine sm;
    private PlayerController playerController;
    private Rigidbody2D rb;                // ── ADDED

    public int CurrentHealth { get; private set; }
    private bool isInvincible = false;
    private bool hasShield = false;
    private bool isRespawning = false;            // ── ADDED: blocks double hits
    private SpriteRenderer sr;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        data = playerController.GetData();
        sm = GetComponent<PlayerStateMachine>();
        sr = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();   // ── ADDED
    }

    private void Start()
    {
        CurrentHealth = data.maxHealth;
        isRespawning = false;                            // ── ADDED
        UIEvents.OnHealthChanged?.Invoke((float)CurrentHealth / data.maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;
        if (isRespawning) return;                        // ── ADDED: no damage during respawn

        if (hasShield)
        {
            hasShield = false;
            AudioManager.Instance.PlaySFX("ShieldBreak");
            return;
        }

        CurrentHealth -= amount;
        if (CurrentHealth < 0)
            CurrentHealth = 0;

        UIEvents.OnHealthChanged?.Invoke((float)CurrentHealth / data.maxHealth);

        if (CurrentHealth <= 0)
        {
            // ── CHANGED: all hearts gone = Game Over
            Kill();
        }
        else
        {
            // ── CHANGED: lost a heart = respawn at checkpoint, no scene reload
            StartCoroutine(HurtAndRespawn());
        }
    }

    // ── ADDED: hurt effect then teleport to last checkpoint ──────────
    private IEnumerator HurtAndRespawn()
    {
        isRespawning = true;

        // Play hurt state
        sm.ChangeState(sm.HurtState);
        AudioManager.Instance?.PlaySFX("Hurt");

        // Wait for hurt animation
        yield return new WaitForSecondsRealtime(0.5f);

        // Teleport to last checkpoint (or level start if none touched)
        Vector3 spawnPos = LevelManager.Instance.GetSpawnPosition();
        transform.position = spawnPos;
        rb.velocity = Vector2.zero;

        

        // Invincibility frames after respawn
        yield return StartCoroutine(InvincibilityFrames());

        isRespawning = false;

        // Back to idle
        sm.ChangeState(sm.IdleState);
    }

    // ── UNCHANGED: Kill = Game Over (called by DeathZone) ────────────
    public void Kill()
    {
        CurrentHealth = 0;
        UIEvents.OnHealthChanged?.Invoke(0f);
        sm.ChangeState(sm.DeadState);
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, data.maxHealth);
        UIEvents.OnHealthChanged?.Invoke((float)CurrentHealth / data.maxHealth);
    }

    public void SetInvincible(bool value) => isInvincible = value;
    public void ActivateShield() => hasShield = true;

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        for (float t = 0; t < data.invincibilityDuration; t += 0.1f)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
        }

        if (sr != null) sr.enabled = true;
        isInvincible = false;
    }
}