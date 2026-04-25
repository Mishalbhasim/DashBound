using UnityEngine;
using System.Collections;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float shakeDelay = 0.5f;
    [SerializeField] private float fallDelay = 1.0f;
    [SerializeField] private float respawnTime = 3f;

    private Rigidbody2D rb;
    private Vector3 startPos;
    private bool triggered = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        startPos = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player") && !triggered)
            StartCoroutine(FallSequence());
    }

    private IEnumerator FallSequence()
    {
        triggered = true;
        
        float elapsed = 0;
        Vector3 originalPos = transform.position;
        while (elapsed < shakeDelay)
        {
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.05f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPos;

        yield return new WaitForSeconds(fallDelay - shakeDelay);

        // Fall
        rb.bodyType = RigidbodyType2D.Dynamic;
        yield return new WaitForSeconds(respawnTime);

        // Respawn
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        transform.position = startPos;
        triggered = false;
    }
}