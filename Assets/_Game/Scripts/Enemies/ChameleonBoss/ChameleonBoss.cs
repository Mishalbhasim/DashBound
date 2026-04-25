// Scripts/Enemies/SpecificEnemies/ChameleonBoss.cs
using UnityEngine;
using System.Collections;

public class ChameleonBoss : BossBase
{
    [Header("Chameleon Boss")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float invisDuration = 2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 9f;

    [Header("Arena Boundaries")]
    [SerializeField] private float arenaLeftEdge = -10f;
    [SerializeField] private float arenaRightEdge = 10f;
    [SerializeField] private float detectionRange = 16f;

    private Transform player;
    private Vector3 startPosition;
    private SpriteRenderer bossSR;


    protected override void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        startPosition = transform.position;
        bossSR = GetComponentInChildren<SpriteRenderer>();
        base.Start();
    }

    
    private void Update()
    {
        if (isDead) return;

        //boss never leaves arena

        float clampedX = Mathf.Clamp(
            transform.position.x,
            arenaLeftEdge,
            arenaRightEdge
        );

        if (Mathf.Abs(clampedX - transform.position.x) > 0.01f)
        {
            transform.position = new Vector3(
                clampedX,
                transform.position.y,
                transform.position.z
            );
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    
    protected override void StartBossFight()
    {
        StartCoroutine(Phase1Loop());
    }

    
    private IEnumerator Phase1Loop()
    {
        while (!isDead && currentPhase == BossPhase.Phase1)
        {
            yield return DecideAction(chaseTime: 2f);
            yield return GoInvisibleAndShoot(bulletCount: 3);
            yield return new WaitForSeconds(1f);
        }
    }

    protected override void Phase2Attack()
    {
        StopAllCoroutines();
        chaseSpeed *= 1.3f;
        invisDuration = 1.5f;
        StartCoroutine(Phase2Loop());
    }

    private IEnumerator Phase2Loop()
    {
        while (!isDead && currentPhase == BossPhase.Phase2)
        {
            yield return DecideAction(chaseTime: 1.5f);
            yield return TeleportBehindPlayer();
            yield return GoInvisibleAndShoot(bulletCount: 5);
            yield return new WaitForSeconds(0.8f);
        }
    }

    protected override void Phase3Attack()
    {
        StopAllCoroutines();
        invisDuration = 0.8f;
        StartCoroutine(Phase3Loop());
    }

    private IEnumerator Phase3Loop()
    {
        while (!isDead)
        {
            yield return DecideAction(chaseTime: 1f);
            yield return TeleportBehindPlayer();
            yield return GoInvisibleAndShoot(bulletCount: 6);
            yield return SpiralShot();
            yield return new WaitForSeconds(0.5f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // MOVEMENT
    // ═══════════════════════════════════════════════════════════════
    private IEnumerator DecideAction(float chaseTime)
    {
        if (player == null) yield break;

        float dist = Mathf.Abs(player.position.x - transform.position.x);

        // playerArrived set by BossArenaActivator or on first hit
        if (playerArrived || dist <= detectionRange)
        {
            playerArrived = true;
            yield return ChasePlayer(chaseTime);
        }
        else
        {
            rb.velocity = Vector2.zero;
            TrySetBool("isWalking", false);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator ChasePlayer(float duration)
    {
        TrySetBool("isWalking", true);
        float t = 0f;

        while (t < duration && !isDead)
        {
            float currentX = transform.position.x;
            float dir = player.position.x > currentX ? 1f : -1f;
            bool atLeftEdge = currentX <= arenaLeftEdge && dir < 0;
            bool atRightEdge = currentX >= arenaRightEdge && dir > 0;

            if (atLeftEdge || atRightEdge)
            {
                rb.velocity = Vector2.zero;
                break;
            }

            rb.velocity = new Vector2(dir * chaseSpeed, rb.velocity.y);
            FaceDirection(dir);
            t += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        TrySetBool("isWalking", false);
    }

    private IEnumerator ReturnToCenter()
    {
        TrySetBool("isWalking", true);

        while (!isDead)
        {
            float dist = Mathf.Abs(transform.position.x - startPosition.x);
            if (dist < 0.2f)
            {
                rb.velocity = Vector2.zero;
                break;
            }

            float dir = startPosition.x > transform.position.x ? 1f : -1f;
            rb.velocity = new Vector2(dir * chaseSpeed * 0.6f, rb.velocity.y);
            FaceDirection(dir);
            yield return null;
        }

        rb.velocity = Vector2.zero;
        TrySetBool("isWalking", false);
    }

    // ═══════════════════════════════════════════════════════════════
    // ATTACKS
    // ═══════════════════════════════════════════════════════════════
    private IEnumerator GoInvisibleAndShoot(int bulletCount)
    {
        if (isDead) yield break;

        TrySetTrigger("Disappear");
        AudioManager.Instance?.PlaySFX("BossRoar");

        // Fade out to near invisible
        yield return FadeSprite(1f, 0.15f, 0.4f);

        rb.velocity = Vector2.zero;

        // Stay invisible
        yield return new WaitForSeconds(invisDuration);

        // Reappear
        yield return FadeSprite(0.15f, 1f, 0.3f);
        TrySetTrigger("Appear");

        // Burst shoot at player
        for (int i = 0; i < bulletCount; i++)
        {
            if (isDead) yield break;
            ShootAtPlayer();
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator TeleportBehindPlayer()
    {
        if (isDead || player == null) yield break;

        // Disappear
        TrySetTrigger("Disappear");
        yield return FadeSprite(1f, 0f, 0.15f);

        // Move to opposite side of player
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;
        float offsetDir = -facingDir;
        float newX = Mathf.Clamp(
            player.position.x + offsetDir * 2f,
            arenaLeftEdge,
            arenaRightEdge
        );

        transform.position = new Vector3(
            newX,
            transform.position.y,
            transform.position.z
        );

        // Face player after teleport
        float dirToPlayer = player.position.x > transform.position.x ? 1f : -1f;
        FaceDirection(dirToPlayer);

        // Reappear
        yield return FadeSprite(0f, 1f, 0.15f);
        TrySetTrigger("Appear");

        AudioManager.Instance?.PlaySFX("BossCharge");
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator SpiralShot()
    {
        if (isDead) yield break;

        TrySetTrigger("Spiral");
        rb.velocity = Vector2.zero;

        int count = 12;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            if (projectilePrefab == null) break;

            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector3 pos = firePoint != null
                ? firePoint.position
                : transform.position;

            GameObject proj = Instantiate(
                projectilePrefab,
                pos,
                Quaternion.identity
            );

            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
            if (projRb != null)
                projRb.velocity = dir * projectileSpeed;

            Destroy(proj, 4f);
            yield return new WaitForSeconds(0.06f);
        }

        yield return new WaitForSeconds(0.5f);
    }

    // ─────────────────────────────────────────────────────────────────
    private void ShootAtPlayer()
    {
        if (projectilePrefab == null || player == null) return;

        Vector3 pos = firePoint != null
            ? firePoint.position
            : transform.position;

        GameObject proj = Instantiate(
            projectilePrefab,
            pos,
            Quaternion.identity
        );

        Vector2 dir = (player.position - pos).normalized;

        Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
        if (projRb != null)
            projRb.velocity = dir * projectileSpeed;

        Destroy(proj, 4f);
        AudioManager.Instance?.PlaySFX("BossAttack");
    }

    private IEnumerator FadeSprite(float from, float to, float duration)
    {
        if (bossSR == null) yield break;

        float t = 0f;
        while (t < duration)
        {
            float alpha = Mathf.Lerp(from, to, t / duration);
            bossSR.color = new Color(1f, 1f, 1f, alpha);
            t += Time.deltaTime;
            yield return null;
        }

        bossSR.color = new Color(1f, 1f, 1f, to);
    }

    private void FaceDirection(float dir)
    {
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * dir,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(arenaLeftEdge, -10, 0),
            new Vector3(arenaLeftEdge, 10, 0)
        );
        Gizmos.DrawLine(
            new Vector3(arenaRightEdge, -10, 0),
            new Vector3(arenaRightEdge, 10, 0)
        );
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}