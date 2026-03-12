using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewsSceneController : MonoBehaviour
{
    [Header("UI")]
    public Image tvScreenImage;
    public TMP_Text dialogueText;
    public RectTransform continueArrow;
    public CanvasGroup continueArrowCanvasGroup;

    [Header("Scene Flow")]
    public string nextSceneName = "day3_clinic";

    [Header("Optional News Images")]
    public Sprite defaultNewsSprite;

    [Header("Typewriter")]
    public float typeSpeed = 0.03f;

    [Header("Arrow Animation")]
    public float arrowFadeSpeed = 2.2f;
    public float arrowFloatSpeed = 2f;
    [Range(0f, 1f)] public float arrowMinAlpha = 0.3f;
    [Range(0f, 1f)] public float arrowMaxAlpha = 1f;
    public float arrowFloatAmount = 3f;

    [Header("Arrow Position")]
    [Tooltip("箭头距离末尾文字的水平偏移，负值更近")]
    public float arrowOffsetX = -2f;
    [Tooltip("箭头相对末尾文字的垂直偏移")]
    public float arrowOffsetY = -2f;

    private List<string> currentLines = new List<string>();
    private int currentIndex = 0;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool sceneReady = false;

    private Vector2 arrowBasePos;

    void Start()
    {
        if (continueArrow != null)
            continueArrow.gameObject.SetActive(false);

        SetupBranchContent();
        ShowCurrentLine();
        sceneReady = true;
    }

    void Update()
    {
        if (!sceneReady) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            HandleClick();
        }

        if (!isTyping && continueArrow != null && continueArrow.gameObject.activeSelf)
        {
            AnimateArrow();
            UpdateArrowPosition();
        }
    }

    void SetupBranchContent()
    {
        currentLines.Clear();

        string day1 = GameProgress_JFM.day1SelectedItemName;
        string day2 = GameProgress_JFM.day2SelectedItemName;

        if (tvScreenImage != null && defaultNewsSprite != null)
            tvScreenImage.sprite = defaultNewsSprite;

        currentLines.Add("Tonight's special report: unusual treatment methods continue to draw public attention.");

        if (!string.IsNullOrEmpty(day1))
            currentLines.Add("Earlier reports linked the first incident to: " + day1 + ".");

        if (!string.IsNullOrEmpty(day2))
            currentLines.Add("A second case has now been associated with: " + day2 + ".");

        currentLines.Add("Public opinion remains divided as more cases emerge.");
        currentLines.Add("Authorities are expected to investigate further developments tomorrow.");
    }

    void ShowCurrentLine()
    {
        if (dialogueText == null) return;

        StopTypingOnly();

        if (continueArrow != null)
            continueArrow.gameObject.SetActive(false);

        typingCoroutine = StartCoroutine(TypeLine(currentLines[currentIndex]));
    }

    void HandleClick()
    {
        if (isTyping)
        {
            StopTypingOnly();
            dialogueText.text = currentLines[currentIndex];
            isTyping = false;
            ShowArrow();
            return;
        }

        AdvanceDialogue();
    }

    void AdvanceDialogue()
    {
        currentIndex++;

        if (currentIndex >= currentLines.Count)
        {
            GoToNextScene();
            return;
        }

        ShowCurrentLine();
    }

    void GoToNextScene()
    {
        StopTypingOnly();

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        ShowArrow();
    }

    void ShowArrow()
    {
        if (continueArrow == null) return;

        continueArrow.gameObject.SetActive(true);
        UpdateArrowPosition();

        arrowBasePos = continueArrow.anchoredPosition;

        if (continueArrowCanvasGroup != null)
            continueArrowCanvasGroup.alpha = 1f;
    }

    void UpdateArrowPosition()
    {
        if (dialogueText == null || continueArrow == null) return;
        if (string.IsNullOrEmpty(dialogueText.text)) return;

        dialogueText.ForceMeshUpdate();

        TMP_TextInfo textInfo = dialogueText.textInfo;
        if (textInfo.characterCount == 0) return;

        int lastVisibleCharIndex = textInfo.characterCount - 1;

        for (int i = textInfo.characterCount - 1; i >= 0; i--)
        {
            if (textInfo.characterInfo[i].isVisible)
            {
                lastVisibleCharIndex = i;
                break;
            }
        }

        TMP_CharacterInfo charInfo = textInfo.characterInfo[lastVisibleCharIndex];

        Vector3 charTopRight = charInfo.topRight;
        Vector3 worldPos = dialogueText.transform.TransformPoint(charTopRight);
        Vector2 localPos;

        RectTransform parentRect = continueArrow.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out localPos
        );

        continueArrow.anchoredPosition = localPos + new Vector2(arrowOffsetX, arrowOffsetY);
        arrowBasePos = continueArrow.anchoredPosition;
    }

    void AnimateArrow()
    {
        if (continueArrow == null) return;

        float fadeT = (Mathf.Sin(Time.time * arrowFadeSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(arrowMinAlpha, arrowMaxAlpha, fadeT);

        if (continueArrowCanvasGroup != null)
            continueArrowCanvasGroup.alpha = alpha;

        float y = Mathf.Sin(Time.time * arrowFloatSpeed) * arrowFloatAmount;
        continueArrow.anchoredPosition = arrowBasePos + new Vector2(0f, y);
    }

    void StopTypingOnly()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }
}