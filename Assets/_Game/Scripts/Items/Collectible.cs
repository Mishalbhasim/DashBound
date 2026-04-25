using UnityEngine;
using System.Collections;

public class Collectible : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Pool")]
    [SerializeField] private string myPoolKey = "Fruit";

    private Vector3 startPos;
    private SpriteRenderer sr;
    private Collider2D myCollider;
    private FruitSpawner owner;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        startPos = transform.position;
        owner = null;

        if (sr != null)
        {
            sr.enabled = true;
            sr.color = Color.white;
        }

        transform.localScale = Vector3.one;

        if (myCollider != null)
            myCollider.enabled = true;
    }

    public void SetOwner(FruitSpawner spawner)
    {
        owner = spawner;
        startPos = transform.position;
    }

    private void Update()
    {
        float y = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPos + new Vector3(0f, y, 0f);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        if (GameManager.Instance != null)
            GameManager.Instance.CollectFruit();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("CollectFruit");

        StartCoroutine(CollectSequence());
    }

    private IEnumerator CollectSequence()
    {
        if (myCollider != null)
            myCollider.enabled = false;

        float t = 0f;
        while (t < 0.25f)
        {
            float p = t / 0.25f;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, p);

            if (sr != null)
                sr.color = new Color(1f, 1f, 1f, 1f - p);

            t += Time.deltaTime;
            yield return null;
        }

        owner?.OnFruitCollected();
        owner = null;

        ObjectPool.Instance.Return(myPoolKey, gameObject);
    }
}