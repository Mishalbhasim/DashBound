using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float pressScale = 0.92f;
    [SerializeField] private float animDuration = 0.15f;

    private Vector3 normalScale;

    private void Awake()
    {
        normalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(normalScale * hoverScale, animDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(normalScale, animDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOScale(normalScale * pressScale, 0.08f).SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOScale(normalScale * hoverScale, 0.08f).SetEase(Ease.OutQuad);
    }
}