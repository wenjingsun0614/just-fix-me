using UnityEngine;

public class MenuArrowFollower : MonoBehaviour
{
    public RectTransform arrowRect;
    public RectTransform defaultTarget;
    public Vector2 offset = new Vector2(-60f, 0f);

    void Start()
    {
        if (defaultTarget != null)
            MoveToTarget(defaultTarget);
    }

    public void MoveToTarget(RectTransform target)
    {
        if (arrowRect == null || target == null) return;

        arrowRect.position = target.position;
        arrowRect.anchoredPosition += offset;
    }
}