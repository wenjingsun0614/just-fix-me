using UnityEngine;
using TMPro;

public class UIHintPulse : MonoBehaviour
{
    public float fadeSpeed = 2.5f;
    public float minAlpha = 0.35f;
    public float maxAlpha = 1f;

    public float scaleAmount = 0.08f;
    public float scaleSpeed = 2.5f;

    private TMP_Text text;
    private Vector3 baseScale;

    void Start()
    {
        text = GetComponent<TMP_Text>();
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (text == null) return;

        float fade = Mathf.Lerp(minAlpha, maxAlpha,
            (Mathf.Sin(Time.unscaledTime * fadeSpeed) + 1f) * 0.5f);

        Color c = text.color;
        c.a = fade;
        text.color = c;

        float scale = 1f + Mathf.Sin(Time.unscaledTime * scaleSpeed) * scaleAmount;
        transform.localScale = baseScale * scale;
    }
}