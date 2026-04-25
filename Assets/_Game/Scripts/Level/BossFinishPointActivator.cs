using UnityEngine;

public class BossFinishPointActivator : MonoBehaviour
{
    [SerializeField] private GameObject finishPoint;

    private void Start()
    {
        if (finishPoint != null)
            finishPoint.SetActive(false);
    }

    private void OnEnable()
    {
        BossBase.OnBossDead += ShowFinishPoint;
    }

    private void OnDisable()
    {
        BossBase.OnBossDead -= ShowFinishPoint;
    }

    private void ShowFinishPoint()
    {
        if (finishPoint != null)
            finishPoint.SetActive(true);
    }
}