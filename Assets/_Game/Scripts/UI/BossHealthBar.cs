using UnityEngine;
using UnityEngine.UI;

public class BossWorldHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthFill;

    private Transform target;
    private Vector3 offset;

    public void Initialize(Transform bossTarget, string bossName, int maxHealth)
    {
        target = bossTarget;
        offset = new Vector3(0, 2f, 0);
        OnHealthChanged(1f);
    }

    public void OnHealthChanged(float healthPercent)
    {
        if (healthFill != null)
            healthFill.fillAmount = healthPercent;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
    }
}