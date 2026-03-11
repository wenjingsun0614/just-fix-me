using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BalloonMiniGameUI : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public Image balloonPreview;
    public Image progressFill;
    public TMP_Text hintText;

    [Header("Sprites")]
    public Sprite deflatedSprite;
    public Sprite halfInflatedSprite;
    public Sprite fullInflatedSprite;

    [Header("Gameplay")]
    public float fillSpeed = 0.55f;     // 按住时上涨速度
    public float drainSpeed = 0.18f;    // 松开时漏气速度
    public float successValue = 1f;     // 到 1 成功

    [Header("Visual Thresholds")]
    [Range(0f, 1f)] public float halfThreshold = 0.4f;
    [Range(0f, 1f)] public float fullThreshold = 0.9f;

    [Header("Balloon Animation")]
    public float minScale = 0.9f;
    public float midScale = 1.05f;
    public float maxScale = 1.18f;
    public float wobbleAmount = 0.03f;
    public float wobbleSpeed = 10f;
    public float scaleSmooth = 12f;

    private bool isOpen = false;
    private float currentValue = 0f;
    private Action onSuccess;

    private Vector3 previewBaseScale;
    private Vector3 previewTargetScale;

    void Start()
    {
        if (balloonPreview != null)
        {
            previewBaseScale = balloonPreview.rectTransform.localScale;
            previewTargetScale = previewBaseScale;
        }

        HideImmediate();
    }

    void Update()
    {
        if (!isOpen) return;

        bool holding = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);

        if (hintText != null)
        {
            hintText.text = holding ? "Inflating..." : "Hold mouse or SPACE to inflate";
        }

        // 1) 进度变化：按住涨，松开漏气
        if (holding)
            currentValue += fillSpeed * Time.unscaledDeltaTime;
        else
            currentValue -= drainSpeed * Time.unscaledDeltaTime;

        currentValue = Mathf.Clamp01(currentValue);

        // 2) 更新 UI
        if (progressFill != null)
            progressFill.fillAmount = currentValue;

        UpdateBalloonVisual(holding);

        // 3) 达到成功阈值
        if (currentValue >= successValue)
        {
            CompleteSuccess();
        }
    }

    public void Open(Action successCallback)
    {
        isOpen = true;
        currentValue = 0f;
        onSuccess = successCallback;

        if (progressFill != null)
            progressFill.fillAmount = 0f;

        if (hintText != null)
            hintText.text = "Hold mouse or SPACE to inflate";

        if (balloonPreview != null)
        {
            balloonPreview.sprite = deflatedSprite;
            balloonPreview.rectTransform.localScale = previewBaseScale;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Close()
    {
        isOpen = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    void CompleteSuccess()
    {
        isOpen = false;
        Time.timeScale = 1f;

        if (balloonPreview != null)
        {
            balloonPreview.sprite = fullInflatedSprite;
            balloonPreview.rectTransform.localScale = previewBaseScale * maxScale;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        gameObject.SetActive(false);

        onSuccess?.Invoke();
        onSuccess = null;
    }

    void UpdateBalloonVisual(bool holding)
    {
        if (balloonPreview == null) return;

        // 1) 根据进度切换 sprite
        if (currentValue < halfThreshold)
        {
            if (deflatedSprite != null)
                balloonPreview.sprite = deflatedSprite;

            previewTargetScale = previewBaseScale * minScale;
        }
        else if (currentValue < fullThreshold)
        {
            if (halfInflatedSprite != null)
                balloonPreview.sprite = halfInflatedSprite;

            previewTargetScale = previewBaseScale * midScale;
        }
        else
        {
            if (fullInflatedSprite != null)
                balloonPreview.sprite = fullInflatedSprite;

            previewTargetScale = previewBaseScale * maxScale;
        }

        // 2) 吹的时候加一点“鼓起/跳动”感
        Vector3 finalScale = previewTargetScale;

        if (holding)
        {
            float wobble = Mathf.Sin(Time.unscaledTime * wobbleSpeed) * wobbleAmount;
            finalScale *= (1f + wobble);
        }

        balloonPreview.rectTransform.localScale = Vector3.Lerp(
            balloonPreview.rectTransform.localScale,
            finalScale,
            Time.unscaledDeltaTime * scaleSmooth
        );

        float wobbleX = Mathf.Sin(Time.unscaledTime * 6f) * 2f;
        balloonPreview.rectTransform.rotation =
            Quaternion.Euler(0, 0, wobbleX);
    }

    void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        gameObject.SetActive(false);
    }
}