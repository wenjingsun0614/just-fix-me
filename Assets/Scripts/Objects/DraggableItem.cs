using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableItem2D : MonoBehaviour
{
    [Header("Drag")]
    public float snapBackTime = 0.18f;

    [Header("Render Priority (Drag on Top)")]
    public int dragSortingBoost = 50;   // 拖拽时临时提高 sortingOrder，保证在最上层
    private int originalSortingOrder;

    [Header("Drop Check")]
    public Collider2D dropZoneCollider;          // 拖入气泡的判定框
    public bool isCorrectItem = false;           // 正确物品勾 true

    [Header("Drop Zone FX")]
    public DropZoneFX dropZoneFX;                // 气泡上的 DropZoneFX（负责播放星星）

    [Header("Success FX")]
    public Transform bubbleCenter;               // 气泡中心点（BubbleCenter）
    public Transform patientAttachPoint;         // 病人挂点（可选：不填则用 GameManager 的 defaultAttachPoint）
    public float snapToCenterTime = 0.10f;       // 吸附到气泡中心速度
    public float popScale = 1.15f;               // pop 放大倍数
    public float popTime = 0.08f;                // pop 放大时间
    public float returnTime = 0.10f;             // pop 回到原比例时间
    public float fadeOutTime = 0.12f;            // 气泡里淡出
    public float fadeInTime = 0.18f;             // （保留字段：现在病人淡入由 GameManager 负责，可未来用）

    [Header("Game Manager")]
    public GameManager_JFM gameManager;          // 场景里的 GameManager_JFM（可留空，自动寻找）

    [Header("SideBar (legacy - optional)")]
    public SideBarUI sideBarUI;
    public int sideBarIndex = 0;

    private Vector3 startPos;
    private Vector3 startScale;

    private bool dragging;
    private Vector3 dragOffset;                  // 鼠标抓取点偏移（防止物体跳动）

    private Camera cam;
    private Collider2D col;
    private SpriteRenderer sr;
    private Coroutine co;

    // ✅ 可选：如果物体上挂了 ItemDisplayScaler，就在“气泡/病人展示”时缩放
    private Component displayScaler;
    private float displayScaleMultiplier = 1f;

    public bool IsDragging => dragging;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        startPos = transform.position;
        startScale = transform.localScale;

        if (sr != null) originalSortingOrder = sr.sortingOrder;

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager_JFM>();

        // 尝试读取你单独挂在路障上的缩放脚本：ItemDisplayScaler.displayScaleMultiplier
        displayScaler = GetComponent("ItemDisplayScaler");
        if (displayScaler != null)
        {
            var type = displayScaler.GetType();
            var field = type.GetField("displayScaleMultiplier");
            if (field != null && field.FieldType == typeof(float))
            {
                displayScaleMultiplier = Mathf.Max(0.01f, (float)field.GetValue(displayScaler));
            }
        }
    }

    void Update()
    {
        if (cam == null) return;

        // 如果当前物体被隐藏在架子上，就不要响应输入（更稳）
        if (sr != null && !sr.enabled) return;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = transform.position.z;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 p = new Vector2(world.x, world.y);
            if (col != null && col.enabled && col.OverlapPoint(p))
            {
                dragging = true;

                // ✅ 拖拽开始：抬到最上层
                SetDragLayer(true);

                // 记录抓取偏移，避免按下时物体瞬间跳到鼠标中心
                dragOffset = transform.position - world;

                if (co != null) StopCoroutine(co);
            }
        }

        if (dragging && Input.GetMouseButton(0))
        {
            transform.position = world + dragOffset;
        }

        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;

            // ✅ 拖拽结束：恢复原层级（先恢复，后面成功/失败动画也会再保证一次）
            SetDragLayer(false);

            if (co != null) StopCoroutine(co);

            // 用“物体中心点是否在气泡里”做判定（最稳）
            Vector2 itemCenter = col.bounds.center;
            bool inZone = (dropZoneCollider != null) && dropZoneCollider.OverlapPoint(itemCenter);

            if (!inZone)
            {
                co = StartCoroutine(SnapBack());
                return;
            }

            // 在气泡里
            if (isCorrectItem)
                co = StartCoroutine(SuccessSequence());
            else
                co = StartCoroutine(SnapBack());
        }
    }

    IEnumerator SuccessSequence()
    {
        // 成功时先禁止再次抓取/判定，避免动画过程中又被拖
        if (col != null) col.enabled = false;

        // ✅ 成功流程也确保层级恢复（避免被隐藏前还卡在高层级）
        SetDragLayer(false);

        // 1) 吸到气泡中心
        if (bubbleCenter != null)
            yield return MoveTo(transform.position, bubbleCenter.position, snapToCenterTime);

        // ✅ 计算“展示用缩放”（只影响气泡/病人显示）
        Vector3 displayBaseScale = startScale * displayScaleMultiplier;

        // 2) Pop（放大再回弹）——用展示缩放做基准（路障会更小）
        transform.localScale = displayBaseScale;
        yield return ScaleTo(displayBaseScale * popScale, popTime);
        yield return ScaleTo(displayBaseScale, returnTime);

        // 2.5) 星星特效
        if (dropZoneFX != null) dropZoneFX.PlaySuccess();

        // ✅ 新逻辑：由 GameManager 统一处理解锁 / UI / 过关箭头
        if (gameManager != null)
        {
            gameManager.RegisterCorrectItem(this);
        }
        else
        {
            // 兼容旧：如果你还在用 sideBarIndex 的版本（可选）
            if (sideBarUI != null) sideBarUI.SetFound(sideBarIndex, true);
        }

        // 3) 在气泡里淡出（让它看起来被使用）
        yield return FadeTo(0f, fadeOutTime);

        // ✅ 关键：在生成病人展示副本前，确保物体 scale 是“展示缩放”
        // 这样 GameManager 用 item.transform.localScale 生成 placed 时就是缩小版
        transform.localScale = displayBaseScale;

        // 4) 病人身上显示/替换（同时会处理：架子隐藏 & 上一个物体回架子）
        if (gameManager != null)
            gameManager.ShowOnPatient(this);
        else
            SpawnPlacedFallback();

        // ✅ 不再 ReturnHome / 立刻显示（由 GameManager 决定什么时候回架子）
        // 这里恢复透明度和缩放，防止下次回架子状态奇怪
        ResetVisualState();
    }

    void ResetVisualState()
    {
        transform.localScale = startScale;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
        // collider 是否启用由 Hide/Show 控制，这里不强行开
    }

    // ---------- Render Priority Helpers ----------

    void SetDragLayer(bool draggingNow)
    {
        if (sr == null) return;

        if (draggingNow)
            sr.sortingOrder = originalSortingOrder + dragSortingBoost;
        else
            sr.sortingOrder = originalSortingOrder;
    }

    // ---------- World Visibility Control (for swapping) ----------

    // 成功装备到病人时：从架子消失（不可点）
    public void HideInWorld()
    {
        dragging = false;

        // ✅ 隐藏前也确保恢复层级，避免下一次显示时还在高层
        SetDragLayer(false);

        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;
    }

    // 被新物体替换后：回到架子原位并可再次拖
    public float reappearFadeTime = 0.18f; // 可调：回架子淡入时间

    public void ShowInWorldAtHome()
    {
        ReturnHome();

        // ✅ 回架子时恢复原层级
        SetDragLayer(false);

        if (sr != null)
        {
            sr.enabled = true;

            Color c = sr.color;
            c.a = 0f;          // 从透明开始
            sr.color = c;
        }

        // 淡入过程中先不允许点击
        if (col != null) col.enabled = false;

        StartCoroutine(FadeInAtHome());
    }

    IEnumerator FadeInAtHome()
    {
        if (sr == null) yield break;

        float t = 0f;
        Color c = sr.color;

        while (t < reappearFadeTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / reappearFadeTime);
            c.a = p;
            sr.color = c;
            yield return null;
        }

        c.a = 1f;
        sr.color = c;

        // 淡入完成后才允许再次拖拽
        if (col != null) col.enabled = true;
    }

    void ReturnHome()
    {
        transform.position = startPos;
        transform.localScale = startScale;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }

    void SpawnPlacedFallback()
    {
        if (patientAttachPoint == null || sr == null) return;

        GameObject placed = new GameObject(name + "_Placed");
        placed.transform.position = patientAttachPoint.position;

        // ✅ fallback 也用展示缩放
        placed.transform.localScale = startScale * displayScaleMultiplier;

        var placedSR = placed.AddComponent<SpriteRenderer>();
        placedSR.sprite = sr.sprite;
        placedSR.sortingLayerID = sr.sortingLayerID;
        placedSR.sortingOrder = sr.sortingOrder + 1;
    }

    IEnumerator SnapBack()
    {
        // 失败：回原位 + 复原透明度/缩放
        yield return FadeTo(1f, 0.05f);
        transform.localScale = startScale;
        yield return MoveTo(transform.position, startPos, snapBackTime);

        // ✅ 失败结束也保险恢复层级
        SetDragLayer(false);
    }

    IEnumerator MoveTo(Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f) { transform.position = to; yield break; }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            p = 1f - Mathf.Pow(1f - p, 3f); // ease out
            transform.position = Vector3.Lerp(from, to, p);
            yield return null;
        }
        transform.position = to;
    }

    IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 from = transform.localScale;
        if (duration <= 0f) { transform.localScale = target; yield break; }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            p = 1f - Mathf.Pow(1f - p, 3f);
            transform.localScale = Vector3.Lerp(from, target, p);
            yield return null;
        }
        transform.localScale = target;
    }

    IEnumerator FadeTo(float alpha, float duration)
    {
        if (sr == null) yield break;

        Color from = sr.color;
        Color to = sr.color;
        to.a = alpha;

        if (duration <= 0f) { sr.color = to; yield break; }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            sr.color = Color.Lerp(from, to, p);
            yield return null;
        }
        sr.color = to;
    }

    // 给 GameManager / UI 用
    public Sprite GetSprite() => sr != null ? sr.sprite : null;
    public int GetSortingLayerID() => sr != null ? sr.sortingLayerID : 0;
    public int GetSortingOrder() => sr != null ? sr.sortingOrder : 0;
}