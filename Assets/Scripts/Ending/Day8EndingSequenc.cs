using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Day8EndingSequence : MonoBehaviour
{
    [Header("Overlay")]
    public Image blackOverlay;

    [Header("Main Message")]
    public TMP_Text centerMessageText;

    [Header("Credits")]
    public TMP_Text creditsTitleText;   // Thanks for playing :)
    public TMP_Text creditsBodyText;    // Created by + names

    [Header("Logo")]
    public Image logoImage;

    [Header("Scene")]
    public string mainMenuSceneName = "main_menu";

    [Header("Main Message Content")]
    [TextArea(2, 4)]
    public string centerMessage = "It’s okay.\nEverything can be fixed... probably.";

    [Header("Credits Content")]
    [TextArea(1, 3)]
    public string creditsTitle = "Thanks for playing :)";

    [TextArea(3, 8)]
    public string creditsBody =
        "Created by\n\n" +
        "Wenjing Sun\n" +
        "Menglan Zhong\n" +
        "Tianyi Gong";

    [Header("Timing")]
    public float fadeToBlackDuration = 1.2f;
    public float holdBlackBeforeMessage = 0.6f;
    public float typeSpeed = 0.06f;
    public float holdMessageDuration = 2.5f;
    public float fadeOutMessageDuration = 0.8f;
    public float holdBeforeCredits = 0.5f;

    [Header("Credits Timing")]
    public float titleFadeInDuration = 0.8f;
    public float holdAfterTitle = 0.6f;
    public float creditsFadeInDuration = 1f;
    public float holdCreditsDuration = 5f;
    public float creditsFadeOutDuration = 0.8f;
    public float finalBlackPause = 0.5f;

    [Header("Title Scale Pop")]
    public float titleStartScale = 0.95f;
    public float titleEndScale = 1f;

    private bool hasPlayed = false;
    private Vector3 titleOriginalScale;
    private Vector3 bodyOriginalScale;
    private Vector3 logoOriginalScale;

    void Start()
    {
        if (blackOverlay != null)
        {
            Color c = blackOverlay.color;
            c.a = 0f;
            blackOverlay.color = c;
        }

        if (centerMessageText != null)
        {
            centerMessageText.text = "";
            SetTMPAlpha(centerMessageText, 1f);
        }

        if (creditsTitleText != null)
        {
            creditsTitleText.text = "";
            SetTMPAlpha(creditsTitleText, 0f);
            titleOriginalScale = creditsTitleText.rectTransform.localScale;
            creditsTitleText.rectTransform.localScale = titleOriginalScale * titleStartScale;
        }

        if (creditsBodyText != null)
        {
            creditsBodyText.text = "";
            SetTMPAlpha(creditsBodyText, 0f);
            bodyOriginalScale = creditsBodyText.rectTransform.localScale;
        }

        if (logoImage != null)
        {
            Color c = logoImage.color;
            c.a = 0f;
            logoImage.color = c;
            logoOriginalScale = logoImage.rectTransform.localScale;
        }
    }

    public void PlayEndingSequence()
    {
        if (hasPlayed) return;
        hasPlayed = true;
        StartCoroutine(EndingRoutine());
    }

    IEnumerator EndingRoutine()
    {
        // 1. Fade to black
        yield return StartCoroutine(FadeOverlay(0f, 1f, fadeToBlackDuration));

        // 2. Hold
        yield return new WaitForSeconds(holdBlackBeforeMessage);

        // 3. Type center message
        if (centerMessageText != null)
        {
            centerMessageText.text = "";
            yield return StartCoroutine(TypeText(centerMessageText, centerMessage, typeSpeed));
        }

        // 4. Hold message
        yield return new WaitForSeconds(holdMessageDuration);

        // 5. Fade out center message
        if (centerMessageText != null)
        {
            yield return StartCoroutine(FadeTMP(centerMessageText, 1f, 0f, fadeOutMessageDuration));
            centerMessageText.text = "";
            SetTMPAlpha(centerMessageText, 1f);
        }

        // 6. Pause before credits
        yield return new WaitForSeconds(holdBeforeCredits);

        // 7. Show title first
        if (creditsTitleText != null)
        {
            creditsTitleText.text = creditsTitle;
            yield return StartCoroutine(FadeAndScaleTitle(0f, 1f, titleFadeInDuration));
        }

        // 8. Hold after title
        yield return new WaitForSeconds(holdAfterTitle);

        // 9. Show body + logo together
        if (creditsBodyText != null)
        {
            creditsBodyText.text = creditsBody;
        }

        yield return StartCoroutine(FadeBodyAndLogo(0f, 1f, creditsFadeInDuration));

        // 10. Hold credits
        yield return new WaitForSeconds(holdCreditsDuration);

        // 11. Fade out title + body + logo together
        yield return StartCoroutine(FadeAllCreditsAndLogo(1f, 0f, creditsFadeOutDuration));

        if (creditsTitleText != null)
        {
            creditsTitleText.text = "";
            creditsTitleText.rectTransform.localScale = titleOriginalScale * titleStartScale;
        }

        if (creditsBodyText != null)
        {
            creditsBodyText.text = "";
        }

        // 12. Make sure screen stays black, then pause slightly
        yield return StartCoroutine(FadeOverlay(1f, 1f, 0.2f));
        yield return new WaitForSeconds(finalBlackPause);

        // 13. Load main menu
        SceneManager.LoadScene(mainMenuSceneName);
    }

    IEnumerator FadeOverlay(float from, float to, float duration)
    {
        if (blackOverlay == null) yield break;

        float t = 0f;
        Color c = blackOverlay.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            c.a = Mathf.Lerp(from, to, p);
            blackOverlay.color = c;
            yield return null;
        }

        c.a = to;
        blackOverlay.color = c;
    }

    IEnumerator TypeText(TMP_Text textComp, string fullText, float speed)
    {
        textComp.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            char ch = fullText[i];
            textComp.text += ch;

            float waitTime = speed;

            if (ch == '\n')
            {
                waitTime = 0.45f;
            }
            else if (ch == '.')
            {
                waitTime = 0.4f;
            }
            else if (ch == ',')
            {
                waitTime = 0.2f;
            }

            if (i >= 2 &&
                fullText[i] == '.' &&
                fullText[i - 1] == '.' &&
                fullText[i - 2] == '.')
            {
                waitTime = 0.9f;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator FadeTMP(TMP_Text textComp, float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            SetTMPAlpha(textComp, Mathf.Lerp(from, to, p));
            yield return null;
        }

        SetTMPAlpha(textComp, to);
    }

    IEnumerator FadeAndScaleTitle(float from, float to, float duration)
    {
        if (creditsTitleText == null) yield break;

        float t = 0f;
        RectTransform rt = creditsTitleText.rectTransform;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);

            SetTMPAlpha(creditsTitleText, Mathf.Lerp(from, to, p));

            float scale = Mathf.Lerp(titleStartScale, titleEndScale, p);
            rt.localScale = titleOriginalScale * scale;

            yield return null;
        }

        SetTMPAlpha(creditsTitleText, to);
        rt.localScale = titleOriginalScale * titleEndScale;
    }

    IEnumerator FadeBodyAndLogo(float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float a = Mathf.Lerp(from, to, p);

            if (creditsBodyText != null)
            {
                SetTMPAlpha(creditsBodyText, a);
            }

            if (logoImage != null)
            {
                SetImageAlpha(logoImage, a);
            }

            yield return null;
        }

        if (creditsBodyText != null)
        {
            SetTMPAlpha(creditsBodyText, to);
        }

        if (logoImage != null)
        {
            SetImageAlpha(logoImage, to);
        }
    }

    IEnumerator FadeAllCreditsAndLogo(float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float a = Mathf.Lerp(from, to, p);

            if (creditsTitleText != null)
            {
                SetTMPAlpha(creditsTitleText, a);
            }

            if (creditsBodyText != null)
            {
                SetTMPAlpha(creditsBodyText, a);
            }

            if (logoImage != null)
            {
                SetImageAlpha(logoImage, a);
            }

            yield return null;
        }

        if (creditsTitleText != null)
        {
            SetTMPAlpha(creditsTitleText, to);
        }

        if (creditsBodyText != null)
        {
            SetTMPAlpha(creditsBodyText, to);
        }

        if (logoImage != null)
        {
            SetImageAlpha(logoImage, to);
        }
    }

    void SetTMPAlpha(TMP_Text textComp, float alpha)
    {
        if (textComp == null) return;

        Color c = textComp.color;
        c.a = alpha;
        textComp.color = c;
    }

    void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;

        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}