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

    [Header("Fallback Scene Flow")]
    public string fallbackNextSceneName = "day4_clinic";

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
    [Tooltip("Horizontal offset from the end of the sentence. Negative = closer.")]
    public float arrowOffsetX = -2f;
    [Tooltip("Vertical offset from the end of the sentence.")]
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

        int newsDay = GameProgress_JFM.currentNewsDay;

        if (tvScreenImage != null && defaultNewsSprite != null)
            tvScreenImage.sprite = defaultNewsSprite;

        switch (newsDay)
        {
            case 1:
                SetupDay1News();
                break;

            case 2:
                SetupDay2News();
                break;

            case 3:
                SetupDay3News();
                break;

            case 4:
                SetupDay4News();
                break;

            case 5:
                SetupDay5News();
                break;

            case 6:
                SetupDay6News();
                break;

            default:
                SetupFallbackNews();
                break;
        }
    }

    void SetupDay1News()
    {
        string item = GameProgress_JFM.day1SelectedItemName;

        currentLines.Add("Tonight's special report: a curious treatment case has drawn local attention.");

        if (!string.IsNullOrEmpty(item))
            currentLines.Add("Clinic records suggest the key item involved was: " + item + ".");

        currentLines.Add("Witnesses describe the procedure as unusual, but surprisingly effective.");
        currentLines.Add("The mysterious clinic is expected to reopen tomorrow.");
    }

    void SetupDay2News()
    {
        string day1 = GameProgress_JFM.day1SelectedItemName;
        string day2 = GameProgress_JFM.day2SelectedItemName;

        currentLines.Add("Tonight's special report: unusual treatment methods continue to draw public attention.");

        if (!string.IsNullOrEmpty(day1))
            currentLines.Add("Earlier reports linked the first incident to: " + day1 + ".");

        if (!string.IsNullOrEmpty(day2))
            currentLines.Add("A second case has now been associated with: " + day2 + ".");

        currentLines.Add("Public opinion remains divided as more cases emerge.");
        currentLines.Add("Authorities are expected to investigate further developments tomorrow.");
    }

    void SetupDay3News()
    {
        string item = GameProgress_JFM.day3SelectedItemName;

        currentLines.Add("Breaking news: the clinic has reported another strange but successful day.");

        if (!string.IsNullOrEmpty(item))
            currentLines.Add("Today's treatment log highlighted the use of: " + item + ".");

        currentLines.Add("Staff members declined formal comment, though the room appeared much tidier than before.");
        currentLines.Add("Patients continue to leave the clinic in better condition than expected.");
        currentLines.Add("More updates will follow as the investigation continues.");
    }

    void SetupDay4News()
    {
        string item = GameProgress_JFM.day4SelectedItemName;

        currentLines.Add("Late-night update: the clinic's reputation continues to spread across town.");

        if (!string.IsNullOrEmpty(item))
            currentLines.Add("Sources say today's key intervention involved: " + item + ".");

        currentLines.Add("Experts remain puzzled by the clinic's methods, but outcomes are difficult to ignore.");
        currentLines.Add("Residents are already speculating about what tomorrow may bring.");
    }

    void SetupDay5News()
    {
        string item = GameProgress_JFM.day5SelectedItemName;

        currentLines.Add("Tonight's clinic report: another unusual case has ended with unexpectedly positive results.");

        if (!string.IsNullOrEmpty(item))
            currentLines.Add("Reporters say today's most talked-about treatment involved: " + item + ".");

        currentLines.Add("Residents remain confused, amused, and increasingly impressed by the clinic's success rate.");
        currentLines.Add("With each passing day, the clinic seems to grow stranger—and somehow more effective.");
    }

    void SetupDay6News()
    {
        string item = GameProgress_JFM.day6SelectedItemName;

        currentLines.Add("Late-night bulletin: today's clinic case has sparked a fresh wave of bizarre rumors.");

        if (string.IsNullOrEmpty(item))
        {
            currentLines.Add("Records from the clinic were incomplete, leaving the public with more questions than answers.");
            currentLines.Add("Even so, witnesses insist the patient left in much better condition than before.");
            currentLines.Add("Tomorrow's developments are being watched closely.");
            return;
        }

        if (item == "LowBrightness")
        {
            currentLines.Add("According to clinic sources, the breakthrough came only after the room was dimmed to an unusually low brightness.");
            currentLines.Add("Experts are divided on whether this was a treatment method, a visual strategy, or simply an alarming electricity bill decision.");
            currentLines.Add("Despite the confusion, the patient reportedly responded well, and the clinic has refused to comment further.");
            return;
        }

        if (item == "FerrariHorse")
        {
            currentLines.Add("Witnesses describe a highly questionable sequence involving a horse, a Ferrari-themed display, and a complete breakdown of professional procedure.");
            currentLines.Add("Authorities have declined to explain how the case was still officially recorded as a success.");
            currentLines.Add("Residents remain unsettled, though some have called it the clinic's boldest treatment yet.");
            return;
        }

        currentLines.Add("Clinic records suggest the key item involved was: " + item + ".");
        currentLines.Add("Observers once again described the method as deeply unusual, but difficult to argue with.");
        currentLines.Add("As the clinic continues to attract attention, tomorrow's case is already the subject of speculation.");
    }

    void SetupFallbackNews()
    {
        currentLines.Add("Tonight's report is currently unavailable.");
        currentLines.Add("Please stand by for further updates.");
    }

    void ShowCurrentLine()
    {
        if (dialogueText == null) return;
        if (currentLines.Count == 0) return;

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

        string targetScene = GameProgress_JFM.nextSceneAfterNews;

        if (string.IsNullOrEmpty(targetScene))
            targetScene = fallbackNextSceneName;

        SceneManager.LoadScene(targetScene);
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