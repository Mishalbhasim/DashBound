
using UnityEngine;


public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _applicationQuitting = false;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            // Don't create new instances when app is quitting
            if (_applicationQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance of {typeof(T)} requested during app quit.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>();

                    if (_instance == null)
                    {
                        var obj = new GameObject($"[Singleton] {typeof(T).Name}");
                        _instance = obj.AddComponent<T>();
                        Debug.Log($"[Singleton] Created new instance of {typeof(T).Name}");
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.Log($"[Singleton] Duplicate {typeof(T).Name} destroyed");
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationQuit()
    {
        _applicationQuitting = true;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}