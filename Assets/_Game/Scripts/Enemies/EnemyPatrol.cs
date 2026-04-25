using UnityEngine;
using System.Collections;

public class EnemyPatrol : EnemyBase
{
    [Header("Patrol")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform edgeCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float wallCheckRadius = 0.15f;
    [SerializeField] private float edgeCheckRadius = 0.15f;

    [Header("Pool")]
    [SerializeField] private string poolKey = "Enemy_MaskDude";

    private EnemySpawner owner;
    private bool isWaiting = false;

    public void SetOwner(EnemySpawner spawner)
    {
        owner = spawner;

        isDead = false;
        isWaiting = false;
        isFacingRight = true;
        currentHealth = data.maxHealth;

        StopAllCoroutines();

        if (sr != null)
        {
            sr.color = Color.white;
            sr.flipX = false;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;

        if (rb != null)
            rb.velocity = Vector2.zero;

        if (anim != null)
            anim.SetBool("isWalking", false);

        transform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (isDead || isWaiting) return;
        Patrol();
    }

    private void Patrol()
    {
        rb.velocity = new Vector2(
            isFacingRight ? data.moveSpeed : -data.moveSpeed,
            rb.velocity.y
        );

        anim.SetBool("isWalking", true);

        bool hitWall = CheckWall();
        bool atEdge = CheckEdge();

        if ((hitWall || atEdge) && !isWaiting)
            StartCoroutine(WaitAndFlip());
    }

    private bool CheckWall()
    {
        if (wallCheck == null) return false;

        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, dir, wallCheckRadius, groundMask);
        return hit.collider != null;
    }

    private bool CheckEdge()
    {
        if (edgeCheck == null) return false;
        return !Physics2D.OverlapCircle(edgeCheck.position, edgeCheckRadius, groundMask);
    }

    private IEnumerator WaitAndFlip()
    {
        isWaiting = true;
        rb.velocity = Vector2.zero;
        anim.SetBool("isWalking", false);

        yield return new WaitForSeconds(data.waitTimeAtTurn);

        float pushDir = isFacingRight ? -1f : 1f;
        rb.velocity = new Vector2(pushDir * data.moveSpeed * 0.5f, rb.velocity.y);

        yield return new WaitForSeconds(0.05f);

        rb.velocity = Vector2.zero;
        Flip();
        anim.SetBool("isWalking", true);
        isWaiting = false;
    }

    protected override void Die()
    {
        isDead = true;
        StopAllCoroutines();

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(data.scoreValue);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        if (rb != null)
            rb.velocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Death");

        owner?.OnEnemyDied();
        owner = null;

        StartCoroutine(DieAndReturn());
    }

    private IEnumerator DieAndReturn()
    {
        yield return new WaitForSeconds(0.6f);
        ObjectPool.Instance.Return(poolKey, gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Vector3 dir = isFacingRight ? Vector3.right : Vector3.left;
            Gizmos.DrawRay(wallCheck.position, dir * wallCheckRadius);
        }

        if (edgeCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(edgeCheck.position, edgeCheckRadius);
        }
    }
}