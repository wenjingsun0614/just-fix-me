using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DraggableItem2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ShakeToDrop : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite iceCreamFullSprite;   // 初始：有雪糕球
    public Sprite coneOnlySprite;       // 成功：只有筒
    public Sprite dropSprite;           // 掉落的雪糕球（单独一张）

    [Header("Shake Detection (tweak these)")]
    public float minSpeed = 6f;
    public float windowTime = 0.7f;
    public int flipsRequired = 6;

    [Header("Result")]
    public bool startAsIncorrect = true;

    [Header("Drop (short + natural)")]
    public float inheritVelocityMultiplier = 0.55f; // 继承筒的水平速度（小一点更稳）
    public float startUpVelocity = 1.0f;            // 小上抛
    public float gravity = 14f;                     // 重力大一点 -> 掉得快更爽
    public float flyTime = 0.65f;                   // 飞行时间（更短）
    public float fadeOutTime = 0.18f;               // 淡出时间（更短）

    [Header("Detach Feel (prevents 'sticking to mouse')")]
    public Vector2 spawnOffset = new Vector2(0.12f, 0.10f); // 生成时先偏移一点点（立刻分离）
    public float randomXImpulse = 0.9f;                     // 额外横向甩出
    public float maxInheritedX = 8f;                         // 限制继承速度避免过猛

    [Header("Pop / Flash On Success")]
    public float popScale = 1.12f;
    public float popUpTime = 0.07f;
    public float popDownTime = 0.10f;
    public float flashTime = 0.10f;

    [Header("Optional FX")]
    public ParticleSystem dropFX;          // 可选粒子（碎屑/小星）
    public AudioSource audioSource;        // 以后放音效用

    private DraggableItem2D drag;
    private SpriteRenderer sr;

    private Vector3 lastPos;
    private float windowTimer;
    private int flips;
    private int lastDir; // -1 left, +1 right, 0 none
    private bool dropped;

    private Vector3 baseScale;
    private Color baseColor;
    private Coroutine feedbackCo;

    private Vector3 prevPos;
    private Vector2 approxVel;

    void Awake()
    {
        drag = GetComponent<DraggableItem2D>();
        sr = GetComponent<SpriteRenderer>();

        baseScale = transform.localScale;
        baseColor = sr.color;

        if (iceCreamFullSprite != null)
            sr.sprite = iceCreamFullSprite;

        ResetShake();

        if (startAsIncorrect && drag != null)
            drag.isCorrectItem = false;
    }

    void Update()
    {
        if (dropped) return;
        if (drag == null) return;

        if (!drag.IsDragging)
        {
            ResetShake();
            return;
        }

        Vector3 pos = transform.position;
        Vector3 delta = pos - lastPos;

        // 估算拖拽速度（用于掉落惯性）
        approxVel = (Vector2)((pos - prevPos) / Mathf.Max(Time.deltaTime, 0.0001f));
        prevPos = pos;

        float vx = (delta.x / Mathf.Max(Time.deltaTime, 0.0001f));
        float speed = Mathf.Abs(vx);

        windowTimer += Time.deltaTime;
        if (windowTimer > windowTime)
        {
            ResetShake();
            return;
        }

        if (speed >= minSpeed)
        {
            int dir = vx > 0 ? 1 : -1;

            if (lastDir != 0 && dir != lastDir)
            {
                flips++;
                if (flips >= flipsRequired)
                {
                    DropIceCream();
                    return;
                }
            }

            lastDir = dir;
        }

        lastPos = pos;
    }

    void DropIceCream()
    {
        dropped = true;

        // 1) 生成掉落物（雪糕球）
        SpawnDropPiece();

        // 2) 可选粒子
        if (dropFX != null) dropFX.Play();

        // 3) 变成正确物体 + 换成“只有筒”
        if (coneOnlySprite != null)
            sr.sprite = coneOnlySprite;

        if (drag != null)
            drag.isCorrectItem = true;

        // 4) pop + flash（更显眼）
        if (feedbackCo != null) StopCoroutine(feedbackCo);
        feedbackCo = StartCoroutine(PopAndFlash());
    }

    void SpawnDropPiece()
    {
        if (dropSprite == null || sr == null) return;

        GameObject go = new GameObject(name + "_DropPiece");

        // ✅ 关键：生成时先偏移一点点，让它立刻“脱离”鼠标跟随的筒
        Vector3 offsetWorld = new Vector3(spawnOffset.x, spawnOffset.y, 0f);
        go.transform.position = transform.position + offsetWorld;
        go.transform.localScale = transform.localScale;

        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = dropSprite;
        r.sortingLayerID = sr.sortingLayerID;
        r.sortingOrder = sr.sortingOrder + 2;

        // 初速度：继承一点点X速度 + 随机甩出 + 小上抛
        float inheritedX = Mathf.Clamp(approxVel.x, -maxInheritedX, maxInheritedX) * inheritVelocityMultiplier;
        float extraX = Random.Range(-randomXImpulse, randomXImpulse);
        Vector2 v0 = new Vector2(inheritedX + extraX, startUpVelocity);

        StartCoroutine(ThrowRoutine(go.transform, r, v0));
    }

    IEnumerator ThrowRoutine(Transform t, SpriteRenderer r, Vector2 v)
    {
        float time = 0f;
        Color c = r.color;

        // 飞行：惯性 + 重力（短）
        while (time < flyTime)
        {
            time += Time.deltaTime;

            v.y -= gravity * Time.deltaTime;
            t.position += (Vector3)(v * Time.deltaTime);

            // 旋转：短时间内转一下就够了
            float spin = Mathf.Lerp(480f, 1100f, Mathf.Clamp01(Mathf.Abs(v.x) / 8f));
            t.rotation = Quaternion.Euler(0, 0, spin * time);

            yield return null;
        }

        // 快速淡出
        float ft = 0f;
        while (ft < fadeOutTime)
        {
            ft += Time.deltaTime;
            float p = Mathf.Clamp01(ft / Mathf.Max(fadeOutTime, 0.0001f));
            c.a = Mathf.Lerp(1f, 0f, p);
            r.color = c;
            yield return null;
        }

        Destroy(t.gameObject);
    }

    IEnumerator PopAndFlash()
    {
        // pop
        Vector3 up = baseScale * popScale;

        float t = 0f;
        while (t < popUpTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / Mathf.Max(popUpTime, 0.0001f));
            transform.localScale = Vector3.Lerp(baseScale, up, EaseOut(p));
            yield return null;
        }

        t = 0f;
        while (t < popDownTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / Mathf.Max(popDownTime, 0.0001f));
            transform.localScale = Vector3.Lerp(up, baseScale, EaseOut(p));
            yield return null;
        }
        transform.localScale = baseScale;

        // flash：短暂变白再回来
        if (sr != null)
        {
            Color white = Color.white;
            white.a = baseColor.a;

            sr.color = white;

            float ft = 0f;
            while (ft < flashTime)
            {
                ft += Time.deltaTime;
                float p = Mathf.Clamp01(ft / Mathf.Max(flashTime, 0.0001f));
                sr.color = Color.Lerp(white, baseColor, p);
                yield return null;
            }
            sr.color = baseColor;
        }
    }

    void ResetShake()
    {
        windowTimer = 0f;
        flips = 0;
        lastDir = 0;

        lastPos = transform.position;
        prevPos = transform.position;
        approxVel = Vector2.zero;
    }

    float EaseOut(float x) => 1f - Mathf.Pow(1f - x, 3f);
}