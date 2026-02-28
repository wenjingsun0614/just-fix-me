using UnityEngine;

public class BallsWiggleOnDrag : MonoBehaviour
{
    [Header("Refs")]
    public DraggableItem2D organizerDraggable; // 拖入 Organizer 的 DraggableItem2D

    [Header("Dragging Minimum (moves as soon as you pick it up)")]
    public float minPosAmpWhenDragging = 0.01f; // 拿起来就会有的最小晃动
    public float minRotAmpWhenDragging = 1.5f;

    [Header("Speed Influence (more shake when you fling)")]
    public float speedToAmplitude = 0.08f; // 灵敏度（你可以 0.06~0.12）
    public float maxExtraAmplitude = 0.14f; // 最大额外晃动（避免穿出篮子）

    [Header("Motion Settings")]
    public float frequency = 12f;
    public float smooth = 12f;
    public float settleSpeed = 18f; // 松手后回稳速度

    Transform[] balls;
    Vector3[] baseLocalPos;
    Quaternion[] baseLocalRot;
    float[] seed;

    Vector3 lastOrganizerPos;
    float currentExtraAmp;

    void Awake()
    {
        // 自动找 Organizer 的 draggable（你也可以手动拖）
        if (organizerDraggable == null)
            organizerDraggable = GetComponentInParent<DraggableItem2D>();

        // 收集第一层子物体作为“海洋球”
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

        if (organizerDraggable != null)
            lastOrganizerPos = organizerDraggable.transform.position;
    }

    void Update()
    {
        if (organizerDraggable == null) return;

        bool dragging = organizerDraggable.IsDragging;

        // 计算“父物体移动速度”
        Vector3 p = organizerDraggable.transform.position;
        float speed = (p - lastOrganizerPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        lastOrganizerPos = p;

        // 速度驱动额外幅度（拖拽时才算）
        float targetExtra = dragging ? Mathf.Min(speed * speedToAmplitude, maxExtraAmplitude) : 0f;

        // 平滑，避免突然跳
        float lerpSpeed = dragging ? smooth : settleSpeed;
        currentExtraAmp = Mathf.Lerp(currentExtraAmp, targetExtra, Time.deltaTime * lerpSpeed);

        // 最终幅度：拖拽时=最小值 + 速度额外；不拖拽=0
        float posAmp = dragging ? (minPosAmpWhenDragging + currentExtraAmp) : 0f;
        float rotAmp = dragging ? (minRotAmpWhenDragging + currentExtraAmp * 120f) : 0f;

        float t = Time.time;

        // 应用到每颗球
        for (int i = 0; i < balls.Length; i++)
        {
            if (balls[i] == null) continue;

            float s = seed[i];
            float wob = Mathf.Sin((t * frequency) + s);
            float wob2 = Mathf.Cos((t * (frequency * 0.85f)) + s * 0.7f);

            Vector3 targetPos = baseLocalPos[i] + new Vector3(wob, wob2, 0f) * posAmp;
            Quaternion targetRot = baseLocalRot[i] * Quaternion.Euler(0f, 0f, wob * rotAmp);

            balls[i].localPosition = Vector3.Lerp(balls[i].localPosition, targetPos, Time.deltaTime * smooth);
            balls[i].localRotation = Quaternion.Slerp(balls[i].localRotation, targetRot, Time.deltaTime * smooth);
        }
    }
}