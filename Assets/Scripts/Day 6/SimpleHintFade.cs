using System.Collections;
using TMPro;
using UnityEngine;

public class SimpleHintFade : MonoBehaviour
{
    public TMP_Text hintText;

    [Header("Timing")]
    public float fadeInTime = 0.25f;
    public float stayTime = 10f;
    public float fadeOutTime = 0.2f;

    private Coroutine routine;

    void Start()
    {
        if (hintText != null)
        {
            Color c = hintText.color;
            c.a = 0f;
            hintText.color = c;
        }
    }

    public void Show(string message)
    {

        if (hintText == null) return;

        hintText.text = message;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        yield return FadeTo(1f, fadeInTime);
        yield return new WaitForSeconds(stayTime);
        yield return FadeTo(0f, fadeOutTime);
    }

    IEnumerator FadeTo(float target, float duration)
    {
        Color c = hintText.color;
        float start = c.a;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            c.a = Mathf.Lerp(start, target, p);
            hintText.color = c;
            yield return null;
        }

        c.a = target;
        hintText.color = c;
    }
}