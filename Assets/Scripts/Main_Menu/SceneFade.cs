using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFade : MonoBehaviour
{
    public Image fadeImage;
    public float fadeTime = 0.8f;

    void Awake()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f; // ✅ 开局保持透明，先看到菜单
            fadeImage.color = c;
        }
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeRoutine(sceneName));
    }

    IEnumerator FadeRoutine(string sceneName)
    {
        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / fadeTime);
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}