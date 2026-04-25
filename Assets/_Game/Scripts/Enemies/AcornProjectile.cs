
using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            col.GetComponent<PlayerHealth>()?.TakeDamage(1);
            Destroy(gameObject);
        }

        if (col.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}