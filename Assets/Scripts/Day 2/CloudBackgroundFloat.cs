using UnityEngine;

public class CloudBackgroundFloat : MonoBehaviour
{
    public BoxCollider2D area;

    [Header("Horizontal Sway")]
    public float horizontalAmplitude = 0.25f; // 左右摆动距离
    public float horizontalSpeed = 0.45f;     // 左右摆动速度

    [Header("Vertical Float")]
    public float floatAmplitude = 0.03f;      // 上下浮动幅度
    public float floatSpeed = 0.9f;           // 上下浮动速度

    [Header("Scale Pulse")]
    public float scalePulseAmount = 0.02f;    // 轻微呼吸缩放
    public float scalePulseSpeed = 0.7f;      // 呼吸速度

    private Vector3 basePos;
    private Vector3 baseScale;

    private float offsetA;
    private float offsetB;
    private float offsetC;

    void Start()
    {
        basePos = transform.position;
        baseScale = transform.localScale;

        // 给每朵云不同相位，避免同步运动
        offsetA = Random.Range(0f, 10f);
        offsetB = Random.Range(0f, 10f);
        offsetC = Random.Range(0f, 10f);

        ClampBasePosInsideArea();
    }

    void Update()
    {
        if (area == null) return;

        ClampBasePosInsideArea();

        float xOffset = Mathf.Sin((Time.time + offsetA) * horizontalSpeed) * horizontalAmplitude;
        float yOffset = Mathf.Sin((Time.time + offsetB) * floatSpeed) * floatAmplitude;

        Vector3 p = basePos + new Vector3(xOffset, yOffset, 0f);

        // 再保险夹一下，确保不会飘出 mask 区域太多
        Bounds bounds = area.bounds;
        p.x = Mathf.Clamp(p.x, bounds.min.x, bounds.max.x);
        p.y = Mathf.Clamp(p.y, bounds.min.y, bounds.max.y);

        transform.position = p;

        float scalePulse = 1f + Mathf.Sin((Time.time + offsetC) * scalePulseSpeed) * scalePulseAmount;
        transform.localScale = baseScale * scalePulse;
    }

    void ClampBasePosInsideArea()
    {
        if (area == null) return;

        Bounds bounds = area.bounds;

        basePos.x = Mathf.Clamp(basePos.x, bounds.min.x, bounds.max.x);
        basePos.y = Mathf.Clamp(basePos.y, bounds.min.y, bounds.max.y);
    }
}