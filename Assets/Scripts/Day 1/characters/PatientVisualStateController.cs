using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientVisualStateController : MonoBehaviour
{
    [System.Serializable]
    public class StateEntry
    {
        public string itemName;
        public Sprite stateSprite;
    }

    [Header("References")]
    public SpriteRenderer targetRenderer;
    public Animator animator;

    [Header("Base State")]
    public Sprite defaultSprite;

    [Header("State Mapping")]
    public List<StateEntry> states = new List<StateEntry>();

    [Header("Transition")]
    public float fadeDuration = 0.12f;
    public float bounceScale = 1.08f;
    public float bounceDuration = 0.12f;

    [Header("Animator Control")]
    public bool disableAnimatorWhenSwap = true;

    private Dictionary<string, Sprite> stateMap = new Dictionary<string, Sprite>();
    private Coroutine transitionCo;
    private Vector3 originalScale;

    [Header("FX")]
    public ParticleSystem swapParticle;

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (targetRenderer != null)
            originalScale = targetRenderer.transform.localScale;

        stateMap.Clear();
        foreach (var entry in states)
        {
            if (entry == null) continue;
            if (string.IsNullOrEmpty(entry.itemName)) continue;
            if (entry.stateSprite == null) continue;

            stateMap[entry.itemName] = entry.stateSprite;
        }

        if (targetRenderer != null && defaultSprite != null)
        {
            targetRenderer.sprite = defaultSprite;
        }
    }

    public void ApplyStateByItemName(string itemName)
    {
        if (targetRenderer == null) return;

        if (disableAnimatorWhenSwap && animator != null && animator.enabled)
        {
            animator.enabled = false;
        }

        Sprite nextSprite = defaultSprite;

        if (!string.IsNullOrEmpty(itemName) && stateMap.TryGetValue(itemName, out var mapped))
        {
            nextSprite = mapped;
        }

        if (transitionCo != null)
            StopCoroutine(transitionCo);

        transitionCo = StartCoroutine(SwapRoutine(nextSprite));
    }

    public void ResetToDefault()
    {
        ApplyStateByItemName("");
    }

    public void ForceSetDefaultImmediately()
    {
        if (transitionCo != null)
        {
            StopCoroutine(transitionCo);
            transitionCo = null;
        }

        if (targetRenderer == null) return;

        if (disableAnimatorWhenSwap && animator != null && animator.enabled)
        {
            animator.enabled = false;
        }

        targetRenderer.sprite = defaultSprite;

        Color c = targetRenderer.color;
        c.a = 1f;
        targetRenderer.color = c;

        targetRenderer.transform.localScale = originalScale;
    }

    IEnumerator SwapRoutine(Sprite nextSprite)
    {
        Color c = targetRenderer.color;
        Transform tr = targetRenderer.transform;

        // fade out
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeDuration);
            c.a = Mathf.Lerp(1f, 0f, p);
            targetRenderer.color = c;
            yield return null;
        }

        c.a = 0f;
        targetRenderer.color = c;

        // swap sprite
        targetRenderer.sprite = nextSprite;

        if (swapParticle != null)
        {
            swapParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            swapParticle.Play();
        }

        // reset scale
        tr.localScale = originalScale;

        // fade in + bounce at the same time
        float totalBounceTime = bounceDuration * 2f;
        float totalTime = Mathf.Max(fadeDuration, totalBounceTime);

        t = 0f;
        while (t < totalTime)
        {
            t += Time.deltaTime;

            // fade in
            float fadeP = Mathf.Clamp01(t / fadeDuration);
            c.a = Mathf.Lerp(0f, 1f, fadeP);
            targetRenderer.color = c;

            // bounce scale
            float scale;
            if (t <= bounceDuration)
            {
                float upP = Mathf.Clamp01(t / bounceDuration);
                scale = Mathf.Lerp(1f, bounceScale, upP);
            }
            else if (t <= totalBounceTime)
            {
                float downP = Mathf.Clamp01((t - bounceDuration) / bounceDuration);
                scale = Mathf.Lerp(bounceScale, 1f, downP);
            }
            else
            {
                scale = 1f;
            }

            tr.localScale = originalScale * scale;

            yield return null;
        }

        c.a = 1f;
        targetRenderer.color = c;
        tr.localScale = originalScale;

        transitionCo = null;
    }
}