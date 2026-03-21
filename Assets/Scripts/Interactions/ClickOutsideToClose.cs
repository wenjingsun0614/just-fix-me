using UnityEngine;
using UnityEngine.EventSystems;

public class ClickOutsideToClose : MonoBehaviour, IPointerDownHandler
{
    public GameObject panelRoot;
    public RectTransform contentArea;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!panelRoot.activeSelf) return;

        if (panelRoot == null || contentArea == null) return;

        bool isInside = RectTransformUtility.RectangleContainsScreenPoint(
            contentArea,
            eventData.position,
            eventData.pressEventCamera
        );

        if (!isInside)
        {
            panelRoot.SetActive(false);
        }
    }
}