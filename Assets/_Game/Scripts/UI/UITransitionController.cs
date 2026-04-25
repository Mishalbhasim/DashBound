using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UITransitionController : Singleton<UITransitionController>
{
    [SerializeField] private Image fadePanel;

    public void FadeOut(float duration) => StartCoroutine(Fade(0f, 1f, duration));
    public void FadeIn(float duration) => StartCoroutine(Fade(1f, 0f, duration));

    private IEnumerator Fade(float from, float to, float duration)
    {
        fadePanel.gameObject.SetActive(true);
        float t = 0;
        while (t < duration)
        {
            fadePanel.color = new Color(0, 0, 0, Mathf.Lerp(from, to, t / duration));
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, to);
        if (to == 0) fadePanel.gameObject.SetActive(false);
    }
}