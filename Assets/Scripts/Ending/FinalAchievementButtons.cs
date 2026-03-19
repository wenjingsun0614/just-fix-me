using System.Collections;
using UnityEngine;

public class FinalAchievementButtons : MonoBehaviour
{
    [Header("Scene")]
    public SceneFade sceneFade;
    public string nextSceneName = "day8_clinic";

    [Header("Arrow UI")]
    public CanvasGroup arrowCanvasGroup;
    public float arrowFadeDuration = 0.35f;

    private bool canGoNext = false;
    private bool isTransitioning = false;

    void Start()
    {
        HideArrowImmediate();
    }

    void Update()
    {
        if (!canGoNext || isTransitioning) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            GoToDay8();
        }
    }

    public void ShowArrowAfterTyping()
    {
        canGoNext = true;

        if (arrowCanvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeArrow(0f, 1f, arrowFadeDuration, true));
        }
    }

    public void GoToDay8()
    {
        if (!canGoNext || isTransitioning) return;

        isTransitioning = true;
        Time.timeScale = 1f;

        if (sceneFade != null)
        {
            sceneFade.FadeToScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("FinalAchievementButtons: sceneFade is not assigned.");
        }
    }

    void HideArrowImmediate()
    {
        if (arrowCanvasGroup == null) return;

        arrowCanvasGroup.alpha = 0f;
        arrowCanvasGroup.interactable = false;
        arrowCanvasGroup.blocksRaycasts = false;
    }

    IEnumerator FadeArrow(float from, float to, float duration, bool enableRaycastAtEnd)
    {
        if (arrowCanvasGroup == null) yield break;

        float timer = 0f;
        arrowCanvasGroup.alpha = from;
        arrowCanvasGroup.interactable = false;
        arrowCanvasGroup.blocksRaycasts = false;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            arrowCanvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        arrowCanvasGroup.alpha = to;
        arrowCanvasGroup.interactable = enableRaycastAtEnd;
        arrowCanvasGroup.blocksRaycasts = enableRaycastAtEnd;
    }
}