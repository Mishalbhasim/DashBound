
using UnityEngine;
using System.Collections;

public class RinoBoss : BossBase
{
    [Header("Rino Boss")]
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private GameObject shockwavePrefab;

    [Header("Arena Boundaries")]
    [SerializeField] private float arenaLeftEdge = -10f;
    [SerializeField] private float arenaRightEdge = 10f;
    [SerializeField] private float detectionRange = 14f;

    //player reached flag
    private bool playerReached = false;
    

    private Transform player;
    private Vector3 startPosition;

    
    protected override void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        startPosition = transform.position;
        base.Start();
    }

    // boss can not leave arena 
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
            yield return DecideAction(chaseTime: 2f);
            yield return ChargeAcrossArena();
            yield return new WaitForSeconds(1f);
        }
    }

    protected override void Phase2Attack()
    {
        StopAllCoroutines();
        chargeSpeed *= 1.3f;
        chaseSpeed *= 1.3f;
        StartCoroutine(Phase2Loop());
    }

    private IEnumerator Phase2Loop()
    {
        while (!isDead && currentPhase == BossPhase.Phase2)
        {
            yield return DecideAction(chaseTime: 1.5f);
            yield return ChargeAcrossArena();
            yield return GroundSlam();            
            yield return new WaitForSeconds(0.8f);
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
            yield return DecideAction(chaseTime: 1f);
            yield return ChargeAcrossArena();
            yield return ChargeAcrossArena();     
            yield return GroundSlam();            
            yield return new WaitForSeconds(0.5f);
        }
    }

    

    
    private IEnumerator DecideAction(float chaseTime)
    {
        if (player == null) yield break;

        float dist = Mathf.Abs(player.position.x - transform.position.x);

        if (dist <= detectionRange)
        {
            // First time player enters range
            if (!playerReached)
            {
                playerReached = true;
                rb.velocity = Vector2.zero;
                TrySetBool("isWalking", false);
                AudioManager.Instance?.PlaySFX("BossRoar");
                yield return new WaitForSeconds(0.8f); 
            }

            yield return ChasePlayer(chaseTime);
        }
        else
        {
            if (!playerReached)
            {
                
                rb.velocity = Vector2.zero;
                TrySetBool("isWalking", false);
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                
                yield return ReturnToCenter();
            }
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
            if (dist < 0.2f) { rb.velocity = Vector2.zero; break; }

            float dir = startPosition.x > transform.position.x ? 1f : -1f;
            rb.velocity = new Vector2(dir * chaseSpeed * 0.6f, rb.velocity.y);
            FaceDirection(dir);
            yield return null;
        }

        rb.velocity = Vector2.zero;
        TrySetBool("isWalking", false);
    }

    
    private IEnumerator ChargeAcrossArena()
    {
        if (isDead || player == null) yield break;

        rb.velocity = Vector2.zero;
        TrySetTrigger("Charge");
        AudioManager.Instance?.PlaySFX("BossCharge");

        
        yield return FlashWarning(Color.red, 3);
        yield return new WaitForSeconds(0.2f);

        float dir = player.position.x > transform.position.x ? 1f : -1f;
        float t = 0f;

        while (t < 1.5f && !isDead)
        {
            float currentX = transform.position.x;
            bool atLeftEdge = currentX <= arenaLeftEdge && dir < 0;
            bool atRightEdge = currentX >= arenaRightEdge && dir > 0;

            if (atLeftEdge || atRightEdge)
            {
                rb.velocity = Vector2.zero;
                SpawnShockwave(transform.position);
                AudioManager.Instance?.PlaySFX("BossRoar");

               
                Camera.main?.GetComponent<CameraFollow>()?.ShakeCamera(0.3f, 0.2f);
                break;
            }

            rb.velocity = new Vector2(dir * chargeSpeed, rb.velocity.y);
            FaceDirection(dir);
            t += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.4f);
    }

    
    private IEnumerator GroundSlam()
    {
        if (isDead || player == null) yield break;

      
        rb.velocity = Vector2.zero;
        TrySetBool("isWalking", false);

        
        yield return FlashWarning(Color.yellow, 3);
        yield return new WaitForSeconds(0.3f);

        
        TrySetTrigger("ChargeDown");
        AudioManager.Instance?.PlaySFX("BossCharge");

       
        float dir = player.position.x > transform.position.x ? 1f : -1f;
        float t = 0f;

        while (t < 0.4f && !isDead)
        {
            float currentX = transform.position.x;
            bool atLeftEdge = currentX <= arenaLeftEdge && dir < 0;
            bool atRightEdge = currentX >= arenaRightEdge && dir > 0;

            if (atLeftEdge || atRightEdge)
            {
                rb.velocity = Vector2.zero;
                break;
            }

            
            rb.velocity = new Vector2(dir * chargeSpeed * 2.5f, rb.velocity.y);
            FaceDirection(dir);
            t += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;

        
        SpawnShockwave(transform.position + Vector3.left * 1.5f);
        SpawnShockwave(transform.position + Vector3.right * 1.5f);

        
        Camera.main?.GetComponent<CameraFollow>()?.ShakeCamera(0.4f, 0.3f);
        AudioManager.Instance?.PlaySFX("BossRoar");

        yield return new WaitForSeconds(0.6f);
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

    private void SpawnShockwave(Vector3 pos)
    {
        if (shockwavePrefab == null) return;
        GameObject sw = Instantiate(shockwavePrefab, pos, Quaternion.identity);
        Destroy(sw, 1f);
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
        Gizmos.DrawLine(new Vector3(arenaLeftEdge, -10, 0),
                        new Vector3(arenaLeftEdge, 10, 0));
        Gizmos.DrawLine(new Vector3(arenaRightEdge, -10, 0),
                        new Vector3(arenaRightEdge, 10, 0));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}