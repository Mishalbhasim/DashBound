
using UnityEngine;
using System.Collections;

public class BossArenaActivator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bossGameObject;
    

    [Header("Settings")]
    [SerializeField] private float revealDelay = 0.5f;

    private bool triggered = false;

    private void Start()
    {
        if (bossGameObject != null)
            bossGameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (triggered) return;
        if (!col.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(ActivateBoss());
    }

    private IEnumerator ActivateBoss()
    {
        

        yield return new WaitForSeconds(revealDelay);

        if (bossGameObject != null)
        {
            bossGameObject.SetActive(true);

            
            yield return null;

            
            BossBase boss = bossGameObject.GetComponent<BossBase>();
            if (boss != null)
                boss.NotifyPlayerArrived();
            
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
            Gizmos.DrawCube(transform.position, box.size);
    }
}