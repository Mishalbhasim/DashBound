using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PoolConfig
{
    public string key;
    public GameObject prefab;
    public int initialSize = 10;
}

public class ObjectPool : Singleton<ObjectPool>
{
    [SerializeField] private PoolConfig[] poolConfigs;

    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();

    // Track ALL active (in-use) objects so we can return them on scene change
    private Dictionary<string, List<GameObject>> activeObjects = new Dictionary<string, List<GameObject>>();

    protected override void Awake()
    {
        base.Awake();

        foreach (var config in poolConfigs)
        {
            if (string.IsNullOrWhiteSpace(config.key))
            {
                Debug.LogWarning("[ObjectPool] Pool key is empty.");
                continue;
            }
            if (config.prefab == null)
            {
                Debug.LogWarning($"[ObjectPool] Prefab missing for key '{config.key}'.");
                continue;
            }
            if (pools.ContainsKey(config.key))
            {
                Debug.LogWarning($"[ObjectPool] Duplicate pool key '{config.key}'.");
                continue;
            }

            pools[config.key] = new Queue<GameObject>();
            prefabMap[config.key] = config.prefab;
            activeObjects[config.key] = new List<GameObject>();

            for (int i = 0; i < config.initialSize; i++)
                CreateNew(config.key);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    private GameObject CreateNew(string key)
    {
        GameObject obj = Instantiate(prefabMap[key], transform);
        obj.SetActive(false);
        pools[key].Enqueue(obj);
        return obj;
    }

    // ─────────────────────────────────────────────────────────────────
    public GameObject Get(string key)
    {
        if (!pools.ContainsKey(key))
        {
            Debug.LogWarning($"[ObjectPool] Pool '{key}' not found!");
            return null;
        }

        if (pools[key].Count == 0)
            CreateNew(key);

        GameObject obj = pools[key].Dequeue();

        // ── FIX: Keep as child of ObjectPool — NEVER SetParent(null) ──
        // Objects stay in DontDestroyOnLoad but are positioned in level
        // SetParent(null) was moving them to DDOL root = never destroyed
        obj.transform.SetParent(transform);
        // ─────────────────────────────────────────────────────────────

        obj.SetActive(true);

        // Reset physics
        Rigidbody2D rb2D = obj.GetComponent<Rigidbody2D>();
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }

        // Track as active
        if (!activeObjects.ContainsKey(key))
            activeObjects[key] = new List<GameObject>();
        activeObjects[key].Add(obj);

        return obj;
    }

    // ─────────────────────────────────────────────────────────────────
    public void Return(string key, GameObject obj)
    {
        if (obj == null) return;

        // Remove from active tracking
        if (activeObjects.ContainsKey(key))
            activeObjects[key].Remove(obj);

        obj.SetActive(false);
        obj.transform.SetParent(transform);  // back under ObjectPool

        if (!pools.ContainsKey(key))
        {
            Debug.LogWarning($"[ObjectPool] Return failed. Pool '{key}' not found!");
            return;
        }

        pools[key].Enqueue(obj);
    }

    // ─────────────────────────────────────────────────────────────────
    // Call this before every scene load — deactivates ALL active objects
    public void ReturnAll()
    {
        foreach (var kvp in activeObjects)
        {
            string key = kvp.Key;

            // Copy list to avoid modification during iteration
            List<GameObject> list = new List<GameObject>(kvp.Value);

            foreach (GameObject obj in list)
            {
                if (obj == null) continue;

                // Notify owner if it has IPoolable
                IPoolReturnable returnable = obj.GetComponent<IPoolReturnable>();
                returnable?.OnReturnedToPool();

                obj.SetActive(false);
                obj.transform.SetParent(transform);

                if (pools.ContainsKey(key))
                    pools[key].Enqueue(obj);
            }

            kvp.Value.Clear();
        }

        Debug.Log("[ObjectPool] ReturnAll — all active objects returned to pool");
    }
}