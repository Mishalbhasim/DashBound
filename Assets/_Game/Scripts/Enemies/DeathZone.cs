using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        
        if (col.CompareTag("Player"))
        {
            col.GetComponent<PlayerHealth>()?.Kill();
            return;
        }

        if (col.CompareTag("Enemy"))
        {
            Destroy(col.gameObject);
            return;
        }

        if (col.CompareTag("Item"))
        {
            Destroy(col.gameObject);
        }
    }
}   