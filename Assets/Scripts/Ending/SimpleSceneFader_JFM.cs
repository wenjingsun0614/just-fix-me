using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimpleSceneFader_JFM : MonoBehaviour
{
    [Header("Fade Overlay")]
    public Image fadeOverlay;

    [Header("Timing")]
    public float fadeOutDuration = 0.8f;
    public float holdBeforeLoad = 0.1f;

    private bool isFading = false;

    void Awake()
    {
        if (fadeOverlay != null)
        {
            Color c = fadeOverlay.color;
            c.a = 0f;
            fadeOverlay.color = c;
        }
    }

    public void FadeToScene(string sceneName)
    {
        if (isFading) return;
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        isFading = true;

        if (fadeOverlay != null)
        {
            float t = 0f;
            Color c = fadeOverlay.color;

            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / fadeOutDuration);
                c.a = Mathf.Lerp(0f, 1f, p);
                fadeOverlay.color = c;
                yield return null;
            }

            c.a = 1f;
            fadeOverlay.color = c;
        }

        yield return new WaitForSeconds(holdBeforeLoad);

        SceneManager.LoadScene(sceneName);
    }
}