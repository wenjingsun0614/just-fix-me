using UnityEngine;

public class SimpleFloat : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatAmplitude = 0.08f;   // 上下浮动幅度
    public float floatSpeed = 1.2f;        // 浮动速度

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = startPos + new Vector3(0f, offsetY, 0f);
    }
}