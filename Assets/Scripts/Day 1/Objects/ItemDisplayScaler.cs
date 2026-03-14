using UnityEngine;

public class ItemDisplayScaler : MonoBehaviour
{
    [Header("Display Scale")]
    [Tooltip("显示时的缩放，例如 0.5 表示缩小一半")]
    public float displayScale = 0.5f;

    void OnEnable()
    {
        ApplyScale();
    }

    public void ApplyScale()
    {
        transform.localScale = Vector3.one * displayScale;
    }

    // 如果你在代码里生成物体，也可以手动调用
    public void SetScale(float scale)
    {
        displayScale = scale;
        ApplyScale();
    }
}