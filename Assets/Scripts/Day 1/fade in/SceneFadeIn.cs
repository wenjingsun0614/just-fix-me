using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFadeIn : MonoBehaviour
{
    public static bool skipNextFadeIn = false;

    public Image fadeImage;
    public float blackHoldTime = 0.5f;
    public float fadeTime = 0.8f;

    void Awake()
    {
        if (fadeImage == null) return;

        Color c = fadeImage.color;

        if (skipNextFadeIn)
            c.a = 0f;
        else
            c.a = 1f;

        fadeImage.color = c;
    }

    void Start()
    {
        if (skipNextFadeIn)
        {
            skipNextFadeIn = false;
            return;
        }

        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        if (fadeImage == null) yield break;

        // ✅ 先保持纯黑一小会
        yield return new WaitForSeconds(blackHoldTime);

        Color c = fadeImage.color;
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