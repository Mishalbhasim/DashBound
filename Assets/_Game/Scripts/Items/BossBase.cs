
using UnityEngine;
using System.Collections;

public enum BossPhase { Phase1, Phase2, Phase3 }

public abstract class BossBase : MonoBehaviour
{
    [Header("Boss Config")]
    [SerializeField] protected int maxHealth = 20;
    [SerializeField] protected string bossName;

    [Header("Health Bar")]
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 2f, 0);

    protected int currentHealth;
    protected BossPhase currentPhase = BossPhase.Phase1;
    protected bool isDead = false;
    protected bool playerArrived = false;  
    protected Animator anim;
    protected Rigidbody2D rb;

    private BossWorldHealthBar worldHealthBar;

    public static System.Action<float> OnBossHealthChanged;
    public static System.Action OnBossDead;
    public static System.Action<BossPhase> OnPhaseChanged;

   
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        SpawnHealthBar();

        if (AudioManager.Instance != null && LevelManager.Instance != null)
            AudioManager.Instance.PlayMusic(
                LevelManager.Instance.CurrentLevelData?.bossMusic,
                forceRestart: true
            );

        StartCoroutine(BossIntroSequence());
    }

    // called by BossArenaActivator 
    public void NotifyPlayerArrived()
    {
        playerArrived = true;
        
    }

    
    private void SpawnHealthBar()
    {
        if (healthBarPrefab == null) return;

        GameObject bar = Instantiate(
            healthBarPrefab,
            transform.position + healthBarOffset,
            Quaternion.identity
        );

        worldHealthBar = bar.GetComponent<BossWorldHealthBar>();
        if (worldHealthBar != null)
            worldHealthBar.Initialize(transform, bossName, maxHealth);
    }

    
    protected virtual IEnumerator BossIntroSequence()
    {
        TrySetTrigger("Intro");
        yield return new WaitForSeconds(1.5f);
        AudioManager.Instance?.PlaySFX("BossRoar");
        yield return new WaitForSeconds(1f);
        StartBossFight();
    }

    
    protected abstract void StartBossFight();
    protected abstract void Phase2Attack();
    protected abstract void Phase3Attack();

    
    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;

        // first hit always notifies player arrived 
        if (!playerArrived)
            playerArrived = true;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        float healthPercent = (float)currentHealth / maxHealth;

        OnBossHealthChanged?.Invoke(healthPercent);
        worldHealthBar?.OnHealthChanged(healthPercent);

        AudioManager.Instance?.PlaySFX("BossHurt");
        TrySetTrigger("Hit");

        Camera.main?.GetComponent<CameraFollow>()?.ShakeCamera(0.15f, 0.1f);

        if (healthPercent <= 0.33f && currentPhase != BossPhase.Phase3)
        {
            currentPhase = BossPhase.Phase3;
            OnPhaseChanged?.Invoke(currentPhase);
            Phase3Attack();
        }
        else if (healthPercent <= 0.66f && currentPhase == BossPhase.Phase1)
        {
            currentPhase = BossPhase.Phase2;
            OnPhaseChanged?.Invoke(currentPhase);
            Phase2Attack();
        }

        if (currentHealth <= 0)
            StartCoroutine(DieSequence());
    }

  
    protected virtual IEnumerator DieSequence()
    {
        isDead = true;

        AudioManager.Instance?.PlaySFX("BossDie");
        TrySetTrigger("Death");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        yield return new WaitForSeconds(2f);

        OnBossDead?.Invoke();
        GameManager.Instance?.AddScore(5000);

        if (worldHealthBar != null)
            Destroy(worldHealthBar.gameObject);

#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = null;
#endif

        Destroy(gameObject);
    }


    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;

        if (col.contacts[0].normal.y < -0.5f)
        {
            TakeDamage(1);
            Rigidbody2D playerRb = col.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
                playerRb.velocity = new Vector2(playerRb.velocity.x, 12f);
        }
        else
        {
            col.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(1);
        }
    }

    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        if (col.transform.position.y > transform.position.y + 0.5f)
        {
            TakeDamage(1);
            Rigidbody2D playerRb = col.GetComponent<Rigidbody2D>();
            if (playerRb != null)
                playerRb.velocity = new Vector2(playerRb.velocity.x, 12f);
        }
        else
        {
            col.GetComponent<PlayerHealth>()?.TakeDamage(1);
        }
    }

    
    protected void TrySetBool(string param, bool value)
    {
        if (anim == null) return;
        foreach (var p in anim.parameters)
            if (p.name == param) { anim.SetBool(param, value); return; }
    }

    protected void TrySetTrigger(string param)
    {
        if (anim == null) return;
        foreach (var p in anim.parameters)
            if (p.name == param) { anim.SetTrigger(param); return; }
    }
}