using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private string poolKey = "Enemy_MaskDude";
    [SerializeField] private float activationBuffer = 4f;

    private GameObject spawnedEnemy = null;
    private bool isSpawned = false;
    private bool isDead = false;
    private bool isSpawning = false;   
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (isDead) return;
        if (isSpawned) return;
        if (isSpawning) return;   

        if (IsVisible())
            SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        isSpawning = true;        

        spawnedEnemy = ObjectPool.Instance.Get(poolKey);
        if (spawnedEnemy == null)
        {
            
            isSpawning = false;
            return;
        }

        spawnedEnemy.transform.position = transform.position;
        spawnedEnemy.transform.rotation = Quaternion.identity;

        EnemyPatrol patrol = spawnedEnemy.GetComponent<EnemyPatrol>();
        if (patrol != null)
            patrol.SetOwner(this);

        isSpawned = true;
        isSpawning = false;       
    }

    public void OnEnemyDied()
    {
        isDead = true;
        isSpawned = false;
        isSpawning = false;
        spawnedEnemy = null;
    }

    public void OnEnemyReturned()
    {
        isSpawned = false;
        isSpawning = false;
        spawnedEnemy = null;
    }

    public void ResetSpawner()
    {
        isDead = false;
        isSpawned = false;
        isSpawning = false;

        if (spawnedEnemy != null)
        {
            ObjectPool.Instance.Return(poolKey, spawnedEnemy);
            spawnedEnemy = null;
        }
    }

    private bool IsVisible()
    {
        if (cam == null)
        {
            cam = Camera.main;   
            if (cam == null) return false;
        }

        float halfH = cam.orthographicSize + activationBuffer;
        float halfW = halfH * cam.aspect + activationBuffer;
        Vector3 cp = cam.transform.position;

        return transform.position.x > cp.x - halfW &&
               transform.position.x < cp.x + halfW &&
               transform.position.y > cp.y - halfH &&
               transform.position.y < cp.y + halfH;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, 0.35f);

#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.6f,
            poolKey
        );
#endif
    }
}