using UnityEngine;

public class BlinkArrow : MonoBehaviour
{
    public float speed = 2f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;

    public float floatSpeed = 2f;
    public float floatAmount = 3f;

    private CanvasGroup cg;
    private Vector3 startPos;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        if (cg == null) return;

        // 闪烁透明度
        float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
        cg.alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        // 轻微上下浮动
        transform.localPosition = startPos + Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatAmount;
    }
}