using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [Header("Pool Key")]
    [SerializeField] private string poolKey = "Fruit";
    [SerializeField] private float activationBuffer = 3f;

    private GameObject spawnedFruit = null;
    private bool isSpawned = false;
    private bool wasCollected = false;
    private bool isSpawning = false;   
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (wasCollected) return;
        if (isSpawned) return;
        if (isSpawning) return;   

        if (IsVisible())
            SpawnFruit();
    }

    private void SpawnFruit()
    {
        isSpawning = true;       

        spawnedFruit = ObjectPool.Instance.Get(poolKey);
        if (spawnedFruit == null)
        {
            isSpawning = false;
            return;
        }

        spawnedFruit.transform.position = transform.position;
        spawnedFruit.transform.rotation = Quaternion.identity;

        Collectible col = spawnedFruit.GetComponent<Collectible>();
        if (col != null)
            col.SetOwner(this);

        isSpawned = true;
        isSpawning = false;         
    }

    public void OnFruitCollected()
    {
        wasCollected = true;
        isSpawned = false;
        isSpawning = false;
        spawnedFruit = null;
    }

    public void OnFruitReturned()
    {
        isSpawned = false;
        isSpawning = false;
        spawnedFruit = null;
    }

    public void ResetSpawner()
    {
        wasCollected = false;
        isSpawned = false;
        isSpawning = false;

        if (spawnedFruit != null)
        {
            ObjectPool.Instance.Return(poolKey, spawnedFruit);
            spawnedFruit = null;
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
}