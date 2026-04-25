using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private PowerUpData data;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        col.GetComponent<PlayerController>()?.ApplyPowerUp(data);
        AudioManager.Instance.PlaySFX("CollectPowerUp");
        UIEvents.OnPowerUpCollected?.Invoke(data); // update HUD icon
        Destroy(gameObject);
    }
}