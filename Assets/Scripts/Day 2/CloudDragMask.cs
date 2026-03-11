using UnityEngine;

[RequireComponent(typeof(DraggableItem2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class CloudDragMaskSwitch : MonoBehaviour
{
    private DraggableItem2D drag;
    private SpriteRenderer sr;

    private bool wasDraggingLastFrame = false;

    void Awake()
    {
        drag = GetComponent<DraggableItem2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (drag == null || sr == null) return;

        // 开始拖拽：取消 mask，允许拖出窗户
        if (drag.IsDragging && !wasDraggingLastFrame)
        {
            sr.maskInteraction = SpriteMaskInteraction.None;
        }

        // ❌ 不要在“松手时”立刻恢复 mask
        // 因为成功流程中它还要飞去气泡，恢复太早会被裁掉

        wasDraggingLastFrame = drag.IsDragging;
    }

    public void RestoreMask()
    {
        if (sr == null) return;
        sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }

    public void DisableMask()
    {
        if (sr == null) return;
        sr.maskInteraction = SpriteMaskInteraction.None;
    }
}