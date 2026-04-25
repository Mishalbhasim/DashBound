
using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    private Animator anim;
    private bool activated = false;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (activated) return;
        if (!col.CompareTag("Player")) return;

        activated = true;

        // Set respawn position
        LevelManager.Instance.SetCheckpoint(transform.position);

        // Play the flag raise animation
        if (anim != null)
            anim.SetTrigger("Activate");

        // Play sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Checkpoint");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = activated ? Color.green : Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

#if UNITY_EDITOR
        UnityEditor.Handles.color = activated ? Color.green : Color.white;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.8f,
            activated ? "ACTIVE" : "CHECKPOINT"
        );
#endif
    }
}