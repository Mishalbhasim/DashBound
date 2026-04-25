using UnityEngine;

public class BossContactDamage : MonoBehaviour
{
    [SerializeField] private int damageToPlayer = 1;
    [SerializeField] private int damageToBoss = 1;
    [SerializeField] private float topHitThreshold = 0.5f;
    [SerializeField] private float bounceForce = 12f;

    private TrunkBoss trunkBoss;

    private void Awake()
    {
        trunkBoss = GetComponent<TrunkBoss>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        ContactPoint2D contact = collision.GetContact(0);

        // Player landed on boss from above
        if (contact.normal.y <= -topHitThreshold)
        {
            Debug.Log("Boss hit from above");

            if (trunkBoss != null)
                trunkBoss.TakeDamage(damageToBoss);

            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = new Vector2(playerRb.velocity.x, bounceForce);
            }
        }
        else
        {
            Debug.Log("Player touched boss from side/below");

            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
            }
        }
    }
}