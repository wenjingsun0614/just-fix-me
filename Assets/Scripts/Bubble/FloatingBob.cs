using UnityEngine;

public class FloatingBob : MonoBehaviour
{
    [Header("Bob")]
    public float amplitudeY = 0.08f;   // 上下浮动幅度（world units）
    public float frequency = 1.2f;     // 浮动速度（Hz）

    [Header("Sway (optional)")]
    public float rotAmplitude = 2f;    // 轻微左右摇（度）
    public float rotFrequency = 1.0f;

    [Header("Randomize")]
    public bool randomStartPhase = true;

    Vector3 startPos;
    Quaternion startRot;
    float phase;

    void Awake()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
        phase = randomStartPhase ? Random.Range(0f, 1000f) : 0f;
    }

    void Update()
    {
        float t = Time.time + phase;

        float y = Mathf.Sin(t * Mathf.PI * 2f * frequency) * amplitudeY;
        float r = Mathf.Sin(t * Mathf.PI * 2f * rotFrequency) * rotAmplitude;

        transform.localPosition = startPos + new Vector3(0f, y, 0f);
        transform.localRotation = startRot * Quaternion.Euler(0f, 0f, r);
    }

    // 如果你以后想在“成功动画/闪电”时暂停浮动，可以调用：
    public void ResetToStart()
    {
        transform.localPosition = startPos;
        transform.localRotation = startRot;
    }
}