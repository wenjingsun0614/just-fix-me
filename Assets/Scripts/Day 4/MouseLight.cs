using UnityEngine;

public class MouseLightFollow : MonoBehaviour
{
    public RectTransform lightRect;
    public Canvas canvas;

    void Update()
    {
        if (lightRect == null || canvas == null) return;

        Vector2 localPoint;
        RectTransform canvasRect = canvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            canvas.worldCamera,
            out localPoint
        );

        lightRect.localPosition = localPoint;
    }
}