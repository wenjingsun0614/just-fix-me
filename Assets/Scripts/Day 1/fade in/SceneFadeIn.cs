using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFadeIn : MonoBehaviour
{
    public Image fadeImage;
    public float blackHoldTime = 0.5f; // 先保持纯黑多久
    public float fadeTime = 0.8f;      // 再慢慢淡出

    void Awake()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f; // ✅ 一进场先保证是纯黑
            fadeImage.color = c;
        }
    }

    void Start()
    {
        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        if (fadeImage == null) yield break;

        Color c = fadeImage.color;

        // ✅ 先保持黑屏一小会，避免“黑完马上跳”
        yield return new WaitForSeconds(blackHoldTime);

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeTime);
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, 0f);
    }
}