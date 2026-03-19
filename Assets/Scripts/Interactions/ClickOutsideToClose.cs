using UnityEngine;
using UnityEngine.EventSystems;

public class ClickOutsideToClose : MonoBehaviour, IPointerDownHandler
{
    [Header("Panel Root (这个脚本挂的物体)")]
    public GameObject panelRoot;

    [Header("真正内容区域（证书本体）")]
    public RectTransform contentArea;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (panelRoot == null || contentArea == null) return;

        // 判断点击是否在内容区域内
        bool isInside = RectTransformUtility.RectangleContainsScreenPoint(
            contentArea,
            eventData.position,
            eventData.pressEventCamera
        );

        // 如果点击在外面 → 关闭
        if (!isInside)
        {
            panelRoot.SetActive(false);
        }
    }
}