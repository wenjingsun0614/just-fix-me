using UnityEngine;

public class MistFloat : MonoBehaviour
{
    [Header("Movement")]
    public float horizontalAmplitude = 0.15f; // ×óÓÒ°Ú¶¯·¶Î§
    public float horizontalSpeed = 0.25f;     // ×óÓÒ°Ú¶¯ËÙ¶È

    public float verticalAmplitude = 0.03f;   // ÉÏÏÂÇá¸¡
    public float verticalSpeed = 0.18f;

    [Header("Scale Pulse")]
    public float scalePulseAmount = 0.015f;   // ÇáÎ¢ºôÎü¸Ð
    public float scalePulseSpeed = 0.25f;

    private Vector3 basePos;
    private Vector3 baseScale;

    private float offsetA;
    private float offsetB;
    private float offsetC;

    void Start()
    {
        basePos = transform.position;
        baseScale = transform.localScale;

        offsetA = Random.Range(0f, 10f);
        offsetB = Random.Range(0f, 10f);
        offsetC = Random.Range(0f, 10f);
    }

    void Update()
    {
        float x = Mathf.Sin((Time.time + offsetA) * horizontalSpeed) * horizontalAmplitude;
        float y = Mathf.Sin((Time.time + offsetB) * verticalSpeed) * verticalAmplitude;

        transform.position = basePos + new Vector3(x, y, 0f);

        float pulse = 1f + Mathf.Sin((Time.time + offsetC) * scalePulseSpeed) * scalePulseAmount;
        transform.localScale = baseScale * pulse;
    }
}