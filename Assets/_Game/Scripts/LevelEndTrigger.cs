using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        HUDController hud = FindFirstObjectByType<HUDController>();
        if (hud != null)
        {
            GameManager.Instance.LevelComplete();
        }
    }
}