using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableItem2D : MonoBehaviour
{
    [Header("Drag")]
    public float snapBackTime = 0.18f;

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

    public bool IsDragging => dragging;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        startPos = transform.position;
        startScale = transform.localScale;

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager_JFM>();
    }

    void Update()
    {
        if (cam == null) return;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = transform.position.z;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 p = new Vector2(world.x, world.y);
            if (col != null && col.OverlapPoint(p))
            {
                dragging = true;

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

        // 1) 吸到气泡中心
        if (bubbleCenter != null)
            yield return MoveTo(transform.position, bubbleCenter.position, snapToCenterTime);

        // 2) Pop（放大再回弹）
        yield return ScaleTo(startScale * popScale, popTime);
        yield return ScaleTo(startScale, returnTime);

        // 2.5) 星星特效
        if (dropZoneFX != null) dropZoneFX.PlaySuccess();

        // ✅ 新逻辑：由 GameManager 统一处理解锁 / UI / 过关箭头 / 病人替换
        if (gameManager != null)
        {
            gameManager.RegisterCorrectItem(this);
        }
        else
        {
            // 兼容旧：如果你还在用 sideBarIndex 的版本（可选）
            if (sideBarUI != null) sideBarUI.SetFound(sideBarIndex, true);
        }

        // 3) 在气泡里淡出
        yield return FadeTo(0f, fadeOutTime);

        // 4) 病人身上显示/替换（生成副本）
        if (gameManager != null)
            gameManager.ShowOnPatient(this);
        else
            SpawnPlacedFallback();

        // 5) 回到原位，恢复可见，继续可拖
        ReturnHome();

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
        placed.transform.localScale = startScale;

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