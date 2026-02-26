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

    [Header("Drop Zone FX (Method B)")]
    public DropZoneFX dropZoneFX;                // 气泡上的 DropZoneFX（负责播放星星）

    [Header("Success FX")]
    public Transform bubbleCenter;               // 气泡中心点（BubbleCenter）
    public Transform patientAttachPoint;         // 病人挂点（PatientHatPoint）
    public float snapToCenterTime = 0.10f;       // 吸附到气泡中心速度
    public float popScale = 1.15f;               // pop 放大倍数
    public float popTime = 0.08f;                // pop 放大时间
    public float returnTime = 0.10f;             // pop 回到原比例时间
    public float fadeOutTime = 0.12f;            // 气泡里淡出
    public float fadeInTime = 0.18f;             // 病人身上淡入

    Vector3 startPos;
    Vector3 startScale;
    bool dragging;
    bool locked;                                 // 成功后锁定不再拖
    Camera cam;
    Collider2D col;
    SpriteRenderer sr;
    Coroutine co;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        startPos = transform.position;
        startScale = transform.localScale;
    }

    void Update()
    {
        if (locked) return;
        if (cam == null) return;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = transform.position.z;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 p = new Vector2(world.x, world.y);
            if (col.OverlapPoint(p))
            {
                dragging = true;
                if (co != null) StopCoroutine(co);
            }
        }

        if (dragging && Input.GetMouseButton(0))
        {
            transform.position = world;
        }

        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;
            if (co != null) StopCoroutine(co);

            // 用“中心点是否在气泡里”做判定（最稳）
            Vector2 itemCenter = col.bounds.center;
            bool inZone = (dropZoneCollider != null) && dropZoneCollider.OverlapPoint(itemCenter);

            if (!inZone)
            {
                co = StartCoroutine(SnapBack());
                return;
            }

            // 在气泡里
            if (isCorrectItem)
            {
                co = StartCoroutine(SuccessSequence());
            }
            else
            {
                co = StartCoroutine(SnapBack());
            }
        }
    }

    IEnumerator SuccessSequence()
    {
        locked = true; // 成功后锁定，避免又被拖

        // 1) 吸到气泡中心
        if (bubbleCenter != null)
            yield return MoveTo(transform.position, bubbleCenter.position, snapToCenterTime);

        // 2) Pop（放大再回弹）
        yield return ScaleTo(startScale * popScale, popTime);
        yield return ScaleTo(startScale, returnTime);

        // ⭐ 2.5) 触发气泡特效（星星上升）
        if (dropZoneFX != null) dropZoneFX.PlaySuccess();

        // 3) 在气泡里淡出消失
        yield return FadeTo(0f, fadeOutTime);

        // 4) 在病人身上生成副本淡入（推荐：不移动原物体，避免逻辑乱）
        if (patientAttachPoint != null)
        {
            GameObject placed = new GameObject(name + "_Placed");
            placed.transform.position = patientAttachPoint.position;
            placed.transform.localScale = startScale;

            var placedSR = placed.AddComponent<SpriteRenderer>();
            placedSR.sprite = sr.sprite;
            placedSR.sortingLayerID = sr.sortingLayerID;
            placedSR.sortingOrder = sr.sortingOrder + 1;

            // 从透明淡入
            Color c = placedSR.color;
            c.a = 0f;
            placedSR.color = c;

            float t = 0f;
            while (t < fadeInTime)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / fadeInTime);
                c.a = a;
                placedSR.color = c;
                yield return null;
            }
            c.a = 1f;
            placedSR.color = c;
        }

        // 5) 原拖拽物体隐藏（或 Destroy）
        gameObject.SetActive(false);
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
}