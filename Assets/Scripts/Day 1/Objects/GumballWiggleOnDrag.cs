using UnityEngine;

public class GumballWiggleOnDrag : MonoBehaviour
{
    [Header("Refs")]
    public DraggableItem2D jarDraggable;

    [Header("Base Motion (idle = 0 recommended)")]
    public float basePosAmplitude = 0f;   // 不拖拽时保持静止
    public float baseRotAmplitude = 0f;

    [Header("Dragging Minimum (so it moves as soon as you pick it up)")]
    public float minPosAmpWhenDragging = 0.01f; // 拿起来就会有的最小晃动
    public float minRotAmpWhenDragging = 2f;

    [Header("Speed Influence")]
    public float speedToAmplitude = 0.06f; // ✅ 建议先用 0.06
    public float maxExtraAmplitude = 0.08f;

    [Header("Motion Settings")]
    public float frequency = 12f;
    public float smooth = 10f;
    public float settleSpeed = 6f;

    Transform[] balls;
    Vector3[] baseLocalPos;
    Quaternion[] baseLocalRot;
    float[] seed;

    Vector3 lastJarPos;
    float currentExtraAmp;

    void Awake()
    {
        int n = transform.childCount;
        balls = new Transform[n];
        baseLocalPos = new Vector3[n];
        baseLocalRot = new Quaternion[n];
        seed = new float[n];

        for (int i = 0; i < n; i++)
        {
            balls[i] = transform.GetChild(i);
            baseLocalPos[i] = balls[i].localPosition;
            baseLocalRot[i] = balls[i].localRotation;
            seed[i] = Random.Range(0f, 1000f);
        }

        if (jarDraggable == null)
            jarDraggable = GetComponentInParent<DraggableItem2D>();

        if (jarDraggable != null)
            lastJarPos = jarDraggable.transform.position;
    }

    void Update()
    {
        if (jarDraggable == null) return;

        float t = Time.time;

        // 速度
        Vector3 jarPos = jarDraggable.transform.position;
        float speed = (jarPos - lastJarPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        lastJarPos = jarPos;

        bool dragging = jarDraggable.IsDragging;

        // 速度带来的额外晃动
        float targetExtra = dragging ? Mathf.Min(speed * speedToAmplitude, maxExtraAmplitude) : 0f;

        // 平滑
        float lerpSpeed = dragging ? smooth : settleSpeed;
        currentExtraAmp = Mathf.Lerp(currentExtraAmp, targetExtra, Time.deltaTime * lerpSpeed);

        // 最终幅度：不拖=0；拖=最小值 + 速度额外值
        float finalPosAmp = basePosAmplitude;
        float finalRotAmp = baseRotAmplitude;

        if (dragging)
        {
            finalPosAmp = Mathf.Max(finalPosAmp, minPosAmpWhenDragging) + currentExtraAmp;
            finalRotAmp = Mathf.Max(finalRotAmp, minRotAmpWhenDragging) + (currentExtraAmp * 200f);
        }

        for (int i = 0; i < balls.Length; i++)
        {
            if (balls[i] == null) continue;

            float s = seed[i];
            float wob = Mathf.Sin((t * frequency) + s);
            float wob2 = Mathf.Cos((t * (frequency * 0.85f)) + s * 0.7f);

            Vector3 targetPos = baseLocalPos[i] + new Vector3(wob, wob2, 0f) * finalPosAmp;
            Quaternion targetRot = baseLocalRot[i] * Quaternion.Euler(0f, 0f, wob * finalRotAmp);

            balls[i].localPosition = Vector3.Lerp(balls[i].localPosition, targetPos, Time.deltaTime * smooth);
            balls[i].localRotation = Quaternion.Slerp(balls[i].localRotation, targetRot, Time.deltaTime * smooth);
        }
    }
}