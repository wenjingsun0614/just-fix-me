using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Arrow")]
    public MenuArrowFollower arrowFollower;

    [Header("Scale")]
    public float hoverScale = 1.08f;
    public float scaleSpeed = 12f;

    private RectTransform rect;
    private Vector3 originalScale;
    private Vector3 targetScale;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        rect.localScale = Vector3.Lerp(rect.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;

        if (arrowFollower != null)
            arrowFollower.MoveToTarget(rect);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}