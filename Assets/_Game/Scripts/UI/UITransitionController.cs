
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UITransitionController : Singleton<UITransitionController>
{
    [SerializeField] private Image fadePanel;
    [SerializeField] private float autoFadeInDelay = 0.1f; // small delay after scene load

    private Coroutine activeRoutine;


    // In UITransitionController.cs replace Awake and OnEnable with this:

    protected override void Awake()
    {
        base.Awake();

        if (fadePanel == null)
            CreateFadePanel();

        // Start black
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.color = Color.black;
        }
    }

    private void Start()
    {
        // ── ADDED: fade in on the very first scene (MainMenu) ─────────────
        // OnSceneLoaded misses the first scene because Singleton initialises
        // after the scene already loaded — Start() catches it instead
        FadeIn(0.5f);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Fade in after every subsequent scene load
        StartCoroutine(AutoFadeIn());
    }

    private IEnumerator AutoFadeIn()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        FadeIn(0.5f);
    }


    public void FadeOut(float duration)
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(Fade(0f, 1f, duration));
    }

    public void FadeIn(float duration)
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(Fade(1f, 0f, duration));
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float ratio = Mathf.Clamp01(t / duration);
            fadePanel.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, ratio));
            yield return null;
        }

        fadePanel.color = new Color(0f, 0f, 0f, to);

        // Disable when fully transparent — saves draw calls
        if (to <= 0f)
            fadePanel.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    private void CreateFadePanel()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("FadePanel");
        go.transform.SetParent(canvas.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        fadePanel = go.AddComponent<Image>();
        fadePanel.color = Color.black;
        fadePanel.raycastTarget = false;

        // Put on top of everything
        go.transform.SetAsLastSibling();
    }
}