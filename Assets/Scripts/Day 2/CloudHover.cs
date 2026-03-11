using UnityEngine;

public class CloudHoverHint : MonoBehaviour
{
    public float hoverScale = 1.08f;
    public float wobbleAmount = 6f;
    public float wobbleSpeed = 8f;
    public float smoothSpeed = 8f;

    private Vector3 originalScale;
    private bool hovering = false;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // ✅ 入场动画没结束前，不允许 hover 效果
        if (!GameFlow_JFM.CanDrag)
        {
            hovering = false;

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                originalScale,
                Time.deltaTime * smoothSpeed
            );

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.identity,
                Time.deltaTime * smoothSpeed
            );

            return;
        }

        if (hovering)
        {
            float angle = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                originalScale * hoverScale,
                Time.deltaTime * smoothSpeed
            );

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0f, 0f, angle),
                Time.deltaTime * smoothSpeed
            );
        }
        else
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                originalScale,
                Time.deltaTime * smoothSpeed
            );

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.identity,
                Time.deltaTime * smoothSpeed
            );
        }
    }

    void OnMouseEnter()
    {
        // ✅ 游戏正式开始前，不触发 hover
        if (!GameFlow_JFM.CanDrag) return;

        hovering = true;
    }

    void OnMouseExit()
    {
        hovering = false;
    }
}