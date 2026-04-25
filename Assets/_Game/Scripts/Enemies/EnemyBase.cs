using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] protected EnemyData data;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;
    protected int currentHealth;
    protected bool isDead = false;
    protected bool isFacingRight = true;

    // Object Pool reference
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = data.maxHealth;
    }

    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        anim.SetTrigger("Hit");
        
        AudioManager.Instance.PlaySFX("EnemyDie");

        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        GameManager.Instance.AddScore(data.scoreValue);
        anim.SetTrigger("Death");
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 0.6f);
    }

    protected void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Stomp kill
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;

        
        if (col.contacts[0].normal.y < -0.5f)
        {
            
            TakeDamage(999);
            col.gameObject.GetComponent<Rigidbody2D>().velocity =
                new Vector2(col.gameObject.GetComponent<Rigidbody2D>().velocity.x, 12f);
        }
        else
        {
            PlayerStateMachine playerSM = col.gameObject.GetComponent<PlayerStateMachine>();
            if (playerSM != null)
            {
                playerSM.DamageSourceX = transform.position.x;
            }

            col.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(data.damageToPlayer);
        }
    }
}