using UnityEngine;

public class ScreenGlowPulse : MonoBehaviour
{
    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float a = 0.35f + Mathf.Sin(Time.time * 4f) * 0.05f;
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }
}