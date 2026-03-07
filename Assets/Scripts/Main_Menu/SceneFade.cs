using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFade : MonoBehaviour
{
    public Image fadeImage;
    public float fadeTime = 0.8f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // ✅ 不在开场改 fadeImage.color.a
        // 只保证默认不挡点击
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeRoutine(sceneName));
    }

    private IEnumerator FadeRoutine(string sceneName)
    {
        if (fadeImage == null) yield break;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;

        Color c = fadeImage.color;
        float startAlpha = c.a;   // ✅ 从当前 alpha 开始，不强制从 0 开始
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(startAlpha, 1f, t / fadeTime);
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, 1f);
        SceneManager.LoadScene(sceneName);
    }
}