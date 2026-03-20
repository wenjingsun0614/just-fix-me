using UnityEngine;

public class SunPulse : MonoBehaviour
{
    public float scaleAmount = 0.05f;
    public float speed = 1f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        float scale = 1 + Mathf.Sin(Time.time * speed) * scaleAmount;
        transform.localScale = originalScale * scale;
    }
}