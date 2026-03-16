using System.Collections;
using TMPro;
using UnityEngine;

public class Day1HoverTutorialHint : MonoBehaviour
{
    [Header("Hint UI")]
    public TMP_Text hintText;

    [Header("Fade")]
    public float fadeInTime = 0.25f;
    public float fadeOutTime = 0.18f;

    private bool tutorialEnabled = false;
    private bool hasShown = false;
    private bool permanentlyDismissed = false;

    private Coroutine fadeCoroutine;

    void Start()
    {
        if (hintText != null)
        {
            Color c = hintText.color;
            c.a = 0f;
            hintText.color = c;
        }
    }

    public void EnableTutorial()
    {
        if (permanentlyDismissed) return;
        tutorialEnabled = true;
    }

    public bool CanShowOnHover()
    {
        return tutorialEnabled && !hasShown && !permanentlyDismissed;
    }

    public void ShowHint()
    {
        if (!CanShowOnHover() || hintText == null) return;

        hasShown = true;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeTo(1f, fadeInTime));
    }

    public void DismissPermanently()
    {
        permanentlyDismissed = true;
        tutorialEnabled = false;

        if (hintText == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeTo(0f, fadeOutTime));
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (hintText == null) yield break;

        Color c = hintText.color;
        float startAlpha = c.a;

        if (duration <= 0f)
        {
            c.a = targetAlpha;
            hintText.color = c;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            c.a = Mathf.Lerp(startAlpha, targetAlpha, p);
            hintText.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        hintText.color = c;
    }
}