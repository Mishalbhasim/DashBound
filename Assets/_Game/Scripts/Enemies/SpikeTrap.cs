using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            col.GetComponent<PlayerHealth>()?.TakeDamage(1);
    }
}