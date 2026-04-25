
using UnityEngine;
using System.Collections;

public class TrunkBoss : BossBase
{
    [Header("Trunk Boss Attacks")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float fireDelay = 1f;

    [Header("Arena Boundaries")]
    [SerializeField] private float arenaLeftEdge = -8f;
    [SerializeField] private float arenaRightEdge = 8f;
    [SerializeField] private float detectionRange = 12f;

    private Transform player;
    private Vector3 startPosition;


    protected override void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        startPosition = transform.position;
        base.Start();
    }

    
    private void Update()
    {
        if (isDead) return;

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
            yield return DecideAction(chaseTime: 2f, shootCount: 1);
            yield return new WaitForSeconds(1.5f);
        }
    }

    protected override void Phase2Attack()
    {
        StopAllCoroutines();
        chaseSpeed *= 1.2f;
        StartCoroutine(Phase2Loop());
    }

    private IEnumerator Phase2Loop()
    {
        while (!isDead && currentPhase == BossPhase.Phase2)
        {
            yield return DecideAction(chaseTime: 1.5f, shootCount: 2);
            yield return ChargeAttack();
            yield return new WaitForSeconds(1f);
        }
    }

    protected override void Phase3Attack()
    {
        StopAllCoroutines();
        StartCoroutine(Phase3Loop());
    }

    private IEnumerator Phase3Loop()
    {
        while (!isDead)
        {
            yield return DecideAction(chaseTime: 1f, shootCount: 3);
            yield return SpiralAttack();
            yield return new WaitForSeconds(0.8f);
        }
    }

    
    private IEnumerator DecideAction(float chaseTime, int shootCount)
    {
        if (player == null) yield break;

        float distToPlayer = Mathf.Abs(
            player.position.x - transform.position.x
        );

        
        if (playerArrived || distToPlayer <= detectionRange)
        {
            playerArrived = true;
            yield return ChasePlayer(chaseTime);
            yield return ShootProjectiles(shootCount);
        }
        else
        {
            // Player not arrived yet
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
            if (player != null)
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

                transform.localScale = new Vector3(
                    Mathf.Abs(transform.localScale.x) * dir,
                    transform.localScale.y,
                    transform.localScale.z
                );
            }

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
            float currentX = transform.position.x;
            float centerX = startPosition.x;

            if (Mathf.Abs(currentX - centerX) < 0.2f)
            {
                rb.velocity = Vector2.zero;
                break;
            }

            float dir = centerX > currentX ? 1f : -1f;
            rb.velocity = new Vector2(dir * (chaseSpeed * 0.6f), rb.velocity.y);

            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x) * dir,
                transform.localScale.y,
                transform.localScale.z
            );

            yield return null;
        }

        rb.velocity = Vector2.zero;
        TrySetBool("isWalking", false);
    }

 
    private IEnumerator ShootProjectiles(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (isDead) yield break;

            TrySetTrigger("Attack");
            AudioManager.Instance?.PlaySFX("BossAttack");

            if (projectilePrefab != null && firePoint != null && player != null)
            {
                GameObject proj = Instantiate(
                    projectilePrefab,
                    firePoint.position,
                    Quaternion.identity
                );

                Vector2 dir = (player.position - firePoint.position).normalized;

                Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
                if (projRb != null)
                    projRb.velocity = dir * projectileSpeed;

                Destroy(proj, 4f);
            }

            yield return new WaitForSeconds(fireDelay);
        }
    }

    private IEnumerator ChargeAttack()
    {
        if (isDead || player == null) yield break;

        TrySetTrigger("Charge");
        AudioManager.Instance?.PlaySFX("BossCharge");

        // Warning flash
        yield return FlashWarning(Color.red, 3);

        float dir = player.position.x > transform.position.x ? 1f : -1f;
        float chargeTime = 0.5f;
        float chargeSpeed = chaseSpeed * 3f;
        float t = 0f;

        while (t < chargeTime && !isDead)
        {
            float currentX = transform.position.x;
            bool atLeftEdge = currentX <= arenaLeftEdge && dir < 0;
            bool atRightEdge = currentX >= arenaRightEdge && dir > 0;

            if (atLeftEdge || atRightEdge)
            {
                rb.velocity = Vector2.zero;
                Camera.main?.GetComponent<CameraFollow>()
                    ?.ShakeCamera(0.3f, 0.2f);
                break;
            }

            rb.velocity = new Vector2(dir * chargeSpeed, rb.velocity.y);
            t += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator SpiralAttack()
    {
        if (isDead) yield break;

        TrySetTrigger("Spiral");
        AudioManager.Instance?.PlaySFX("BossRoar");

        rb.velocity = Vector2.zero;

        int bulletCount = 8;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            if (projectilePrefab == null) break;

            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector3 spawnPos = firePoint != null
                ? firePoint.position
                : transform.position;

            GameObject proj = Instantiate(
                projectilePrefab,
                spawnPos,
                Quaternion.identity
            );

            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
            if (projRb != null)
                projRb.velocity = dir * projectileSpeed;

            Destroy(proj, 4f);
            yield return new WaitForSeconds(0.08f);
        }

        yield return new WaitForSeconds(0.5f);
    }

   
    private IEnumerator FlashWarning(Color flashColor, int times)
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) yield break;

        for (int i = 0; i < times; i++)
        {
            sr.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(arenaLeftEdge, -10f, 0),
            new Vector3(arenaLeftEdge, 10f, 0)
        );
        Gizmos.DrawLine(
            new Vector3(arenaRightEdge, -10f, 0),
            new Vector3(arenaRightEdge, 10f, 0)
        );
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            Application.isPlaying ? startPosition : transform.position,
            0.4f
        );
    }
}