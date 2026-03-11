using UnityEngine;

[RequireComponent(typeof(DraggableItem2D))]
public class CloudDriftInArea : MonoBehaviour
{
    public BoxCollider2D area;   // 窗户范围

    public float speed = 0.25f;
    public float floatAmplitude = 0.05f;
    public float floatSpeed = 1.2f;

    private DraggableItem2D drag;
    private Vector3 basePos;
    private int direction = 1;

    private bool floatEnabled = true;

    void Start()
    {
        drag = GetComponent<DraggableItem2D>();

        if (area == null)
        {
            basePos = transform.position;
            return;
        }

        Bounds bounds = area.bounds;

        // 开始时先把云的位置夹到 area 里面
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, bounds.min.x, bounds.max.x);
        p.y = Mathf.Clamp(p.y, bounds.min.y, bounds.max.y);

        transform.position = p;
        basePos = p;
    }

    void Update()
    {
        if (!floatEnabled) return;
        if (drag != null && drag.IsDragging) return;
        if (area == null) return;

        Bounds bounds = area.bounds;

        // 左右移动
        basePos.x += direction * speed * Time.deltaTime;

        if (basePos.x >= bounds.max.x)
        {
            basePos.x = bounds.max.x;
            direction = -1;
        }
        else if (basePos.x <= bounds.min.x)
        {
            basePos.x = bounds.min.x;
            direction = 1;
        }

        // 上下浮动
        Vector3 p = basePos;
        p.y += Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        // 再限制一下 y
        p.y = Mathf.Clamp(p.y, bounds.min.y, bounds.max.y);

        transform.position = p;
    }

    public void StopFloating()
    {
        floatEnabled = false;
    }

    public void ResumeFloating()
    {
        floatEnabled = true;
        basePos = transform.position;
    }
}