
using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    //Components
    private PlayerData data;
    private PlayerStateMachine sm;
    private PlayerController playerController;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    //State
    public int CurrentHealth { get; private set; }
    private bool isInvincible = false;
    private bool hasShield = false;
    private bool isRespawning = false;

    //Particles (optional)
    [Header("Particles (optional)")]
    [SerializeField] private ParticleSystem hurtParticle;
    [SerializeField] private ParticleSystem deathParticle;
    [SerializeField] private ParticleSystem shieldBreakParticle;


    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        data = playerController.GetData();
        sm = GetComponent<PlayerStateMachine>();
        sr = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        CurrentHealth = data.maxHealth;
        isRespawning = false;
        UIEvents.OnHealthChanged?.Invoke(1f);
    }

    //TAKE DAMAGE
    public void TakeDamage(int amount)
    {
        if (isInvincible) return;
        if (isRespawning) return;

        // Shield absorbs one hit
        if (hasShield)
        {
            hasShield = false;
            shieldBreakParticle?.Play();
            AudioManager.Instance?.PlaySFX("ShieldBreak");
            StartCoroutine(ShieldBreakFlash());
            return;
        }

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        UIEvents.OnHealthChanged?.Invoke((float)CurrentHealth / data.maxHealth);

        if (CurrentHealth <= 0)
            Kill();
        else
            StartCoroutine(HurtAndRespawn());
    }

    // ── Hurt — lose a heart, respawn at checkpoint
    private IEnumerator HurtAndRespawn()
    {
        isRespawning = true;

        hurtParticle?.Play();
        sm.ChangeState(sm.HurtState);
        AudioManager.Instance?.PlaySFX("Hurt");

        yield return new WaitForSecondsRealtime(0.5f);

        // Move to last checkpoint or level start
        Vector3 spawnPos = LevelManager.Instance != null
            ? LevelManager.Instance.GetSpawnPosition()
            : Vector3.zero;

        transform.position = spawnPos;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 1f;

        yield return StartCoroutine(InvincibilityFrames());

        isRespawning = false;
        sm.ChangeState(sm.IdleState);
    }

    // Kill — all hearts gone, trigger game over
    public void Kill()
    {
        if (isRespawning && CurrentHealth > 0) return; // prevent double-kill

        CurrentHealth = 0;
        UIEvents.OnHealthChanged?.Invoke(0f);

        deathParticle?.Play();
        sm.ChangeState(sm.DeadState);
    }

    //HEAL
    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, data.maxHealth);
        UIEvents.OnHealthChanged?.Invoke((float)CurrentHealth / data.maxHealth);
    }

    //Reset health fully (called on new level)
    public void ResetHealth()
    {
        CurrentHealth = data.maxHealth;
        isRespawning = false;
        isInvincible = false;
        if (sr != null) sr.enabled = true;
        UIEvents.OnHealthChanged?.Invoke(1f);
    }

    //INVINCIBILITY FRAMES
    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        float elapsed = 0f;
        while (elapsed < data.invincibilityDuration)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (sr != null) sr.enabled = true;
        isInvincible = false;
    }

    private IEnumerator ShieldBreakFlash()
    {
        if (sr == null) yield break;
        Color original = sr.color;

        for (int i = 0; i < 3; i++)
        {
            sr.color = Color.cyan;
            yield return new WaitForSeconds(0.08f);
            sr.color = original;
            yield return new WaitForSeconds(0.08f);
        }
    }

    //SETTERS
    public void SetInvincible(bool value) => isInvincible = value;
    public void ActivateShield() => hasShield = true;
    public bool IsInvincible => isInvincible;
}