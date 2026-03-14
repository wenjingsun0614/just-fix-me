using System.Collections;
using UnityEngine;

public class BalloonSpecialBehaviour : MonoBehaviour
{
    [Header("Points")]
    public Transform hangerPoint;
    public Transform ceilingPoint;

    [Header("Rope")]
    public SpriteRenderer[] ropeRenderers;
    public float ropeFadeDuration = 0.35f;

    [Header("Rise To Ceiling")]
    public float riseDuration = 1.0f;
    public AnimationCurve riseEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Ceiling Float")]
    public float ceilingFloatAmplitude = 0.08f;
    public float ceilingFloatSpeed = 1.2f;

    [Header("Reappear")]
    public float reappearFadeTime = 0.18f;

    [Header("State")]
    public bool ropeRemoved = false;
    public bool useCeilingAsHome = false;

    private DraggableItem2D draggable;
    private SpriteRenderer sr;
    private Collider2D col;

    private bool isAnimating = false;
    private Vector3 ceilingBasePos;
    private float floatSeed;

    void Awake()
    {
        draggable = GetComponent<DraggableItem2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        floatSeed = Random.Range(0f, 100f);
    }

    void Start()
    {
        if (useCeilingAsHome && ceilingPoint != null)
        {
            ceilingBasePos = ceilingPoint.position;
            transform.position = ceilingBasePos;

            if (draggable != null)
                draggable.SetHomePosition(ceilingBasePos);
        }
        else if (hangerPoint != null)
        {
            transform.position = hangerPoint.position;

            if (draggable != null)
                draggable.SetHomePosition(hangerPoint.position);
        }

        ApplyRopeStateImmediate();
    }

    void Update()
    {
        if (!useCeilingAsHome) return;
        if (isAnimating) return;
        if (draggable != null && draggable.IsDragging) return;
        if (ceilingPoint == null) return;

        float y = Mathf.Sin((Time.time + floatSeed) * ceilingFloatSpeed) * ceilingFloatAmplitude;
        transform.position = ceilingBasePos + new Vector3(0f, y, 0f);
    }

    public void OnAcceptedIntoBubble()
    {
        if (!ropeRemoved)
        {
            ropeRemoved = true;
            StartCoroutine(FadeOutRope());
        }

        Debug.Log("Balloon accepted -> trigger seagull gas mini-game here.");
    }

    public bool HandleShowInWorldAtHome(DraggableItem2D owner)
    {
        if (owner == null) return false;

        // 还没真正被用过，就走默认逻辑
        if (!ropeRemoved)
            return false;

        StopAllCoroutines();

        if (!useCeilingAsHome)
        {
            useCeilingAsHome = true;

            if (ceilingPoint != null)
                owner.SetHomePosition(ceilingPoint.position);

            StartCoroutine(ShowFromHangerThenRise());
            return true;
        }

        StartCoroutine(ShowDirectlyAtCeiling());
        return true;
    }

    IEnumerator FadeOutRope()
    {
        if (ropeRenderers == null || ropeRenderers.Length == 0)
            yield break;

        float t = 0f;
        Color[] startColors = new Color[ropeRenderers.Length];

        for (int i = 0; i < ropeRenderers.Length; i++)
        {
            if (ropeRenderers[i] != null)
                startColors[i] = ropeRenderers[i].color;
        }

        while (t < ropeFadeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / ropeFadeDuration);

            for (int i = 0; i < ropeRenderers.Length; i++)
            {
                if (ropeRenderers[i] == null) continue;

                Color c = startColors[i];
                c.a = Mathf.Lerp(startColors[i].a, 0f, p);
                ropeRenderers[i].color = c;
            }

            yield return null;
        }

        for (int i = 0; i < ropeRenderers.Length; i++)
        {
            if (ropeRenderers[i] != null)
                ropeRenderers[i].enabled = false;
        }
    }

    IEnumerator ShowFromHangerThenRise()
    {
        if (hangerPoint == null || ceilingPoint == null)
            yield break;

        isAnimating = true;

        if (sr != null)
        {
            sr.enabled = true;
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        if (col != null)
            col.enabled = false;

        transform.position = hangerPoint.position;
        transform.localScale = Vector3.one * transform.localScale.x; // 保持现有比例，不重置怪值

        Vector3 start = hangerPoint.position;
        Vector3 end = ceilingPoint.position;

        float t = 0f;
        while (t < riseDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / riseDuration);
            float eased = riseEase.Evaluate(p);
            transform.position = Vector3.Lerp(start, end, eased);
            yield return null;
        }

        ceilingBasePos = end;
        transform.position = ceilingBasePos;

        if (draggable != null)
            draggable.SetHomePosition(ceilingBasePos);

        if (col != null)
            col.enabled = true;

        isAnimating = false;
    }

    IEnumerator ShowDirectlyAtCeiling()
    {
        if (ceilingPoint == null)
            yield break;

        isAnimating = true;
        ceilingBasePos = ceilingPoint.position;

        transform.position = ceilingBasePos;

        if (draggable != null)
            draggable.SetHomePosition(ceilingBasePos);

        if (sr != null)
        {
            sr.enabled = true;
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;

            float t = 0f;
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
        }

        if (col != null)
            col.enabled = true;

        isAnimating = false;
    }

    void ApplyRopeStateImmediate()
    {
        if (ropeRenderers == null) return;

        foreach (var rope in ropeRenderers)
        {
            if (rope == null) continue;

            rope.enabled = !ropeRemoved;

            Color c = rope.color;
            c.a = ropeRemoved ? 0f : 1f;
            rope.color = c;
        }
    }
}