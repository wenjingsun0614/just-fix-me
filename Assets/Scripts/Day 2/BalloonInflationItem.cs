using UnityEngine;

[RequireComponent(typeof(DraggableItem2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BalloonInflationItem : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite deflatedSprite;
    public Sprite halfInflatedSprite;
    public Sprite inflatedSprite;

    [Header("Mini Game")]
    public BalloonMiniGameUI miniGameUI;

    [Header("Completed Visual")]
    public float inflatedWorldScaleMultiplier = 1.25f;

    [Tooltip("成功后回到架子时，往上抬一点的世界坐标偏移")]
    public float completedShelfYOffset = 0.18f;

    [Tooltip("完成后在架子上的上下浮动幅度")]
    public float floatAmplitude = 0.04f;

    [Tooltip("完成后在架子上的上下浮动速度")]
    public float floatSpeed = 2.2f;

    private DraggableItem2D drag;
    private SpriteRenderer sr;

    private bool inflationCompleted = false;

    private Vector3 originalScale;
    private Vector3 completedScale;

    private Vector3 shelfBasePosition;
    private bool shelfFloatInitialized = false;

    void Awake()
    {
        drag = GetComponent<DraggableItem2D>();
        sr = GetComponent<SpriteRenderer>();

        originalScale = transform.localScale;
        completedScale = originalScale * inflatedWorldScaleMultiplier;

        if (deflatedSprite != null)
            sr.sprite = deflatedSprite;

        // 一开始不能直接成功
        drag.isCorrectItem = false;

        // 把 3 张图传给小游戏 UI
        if (miniGameUI != null)
        {
            miniGameUI.deflatedSprite = deflatedSprite;
            miniGameUI.halfInflatedSprite = halfInflatedSprite;
            miniGameUI.fullInflatedSprite = inflatedSprite;
        }
    }

    void Update()
    {
        // 只有在“已经充好气”且当前没在拖拽时，才做架子上的漂浮
        if (!inflationCompleted) return;
        if (drag == null) return;
        if (drag.IsDragging) return;

        // 如果物体当前不可见（比如装备到病人身上）就不做浮动
        if (sr != null && !sr.enabled) return;

        if (!shelfFloatInitialized)
        {
            shelfBasePosition = transform.position;
            shelfFloatInitialized = true;
        }

        Vector3 p = shelfBasePosition;
        p.y += Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = p;
    }

    public bool HasCompletedInflation()
    {
        return inflationCompleted;
    }

    public void TriggerMiniGame()
    {
        if (inflationCompleted) return;
        if (miniGameUI == null) return;

        miniGameUI.Open(OnMiniGameSuccess);
    }

    void OnMiniGameSuccess()
    {
        inflationCompleted = true;

        if (inflatedSprite != null)
            sr.sprite = inflatedSprite;

        // ✅ 成功后场景里的气球本体放大一点
        transform.localScale = completedScale;

        // ✅ 成功后先把当前这个位置记成浮动基准（后面回架子时还会再更新）
        shelfBasePosition = transform.position;
        shelfFloatInitialized = false;

        // 现在这个物体变成“正确物品”
        drag.isCorrectItem = true;

        // 小游戏成功后，回到原本成功流程
        drag.TriggerSuccessAfterMiniGame();
    }

    /// <summary>
    /// 给外部调用：当物体回到架子时，重新设置“完成状态”的架子位置和浮动基准
    /// </summary>
    public void ApplyCompletedShelfPoseIfNeeded()
    {
        if (!inflationCompleted) return;

        // 充好气后回架子时，整体上移一点
        Vector3 p = transform.position;
        p.y += completedShelfYOffset;
        transform.position = p;

        // 保持完成后的大小
        transform.localScale = completedScale;

        // 重新记录浮动基准点
        shelfBasePosition = transform.position;
        shelfFloatInitialized = true;
    }

    /// <summary>
    /// 可选：如果以后你想让别的系统知道当前是否完成
    /// </summary>
    public bool IsInflated()
    {
        return inflationCompleted;
    }
}