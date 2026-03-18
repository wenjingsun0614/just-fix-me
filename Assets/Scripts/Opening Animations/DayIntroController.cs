using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DayIntroController : MonoBehaviour
{
    public enum Speaker
    {
        Patient,
        Doctor
    }

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker;

        [TextArea(2, 4)]
        public string text;
    }

    [Header("Bubble References")]
    public GameObject patientBubble;
    public Image patientBubbleImage;
    public TMP_Text patientText;

    public GameObject doctorBubble;
    public Image doctorBubbleImage;
    public TMP_Text doctorText;

    [Header("Puzzle UI")]
    public GameObject speechBubbleZone;
    public GameObject sideBar;

    [Header("Others")]
    public Day1HoverTutorialHint tutorialHint;
    public ShowFixButtonAfterIntro fixButtonController;
    public HorsePatientEasterEgg horsePatientEasterEgg;
    public GameObject[] interactionObjects;

    [Header("Timing")]
    public float introAnimationDuration = 2f;
    public float delayBeforeDialogue = 0.2f;
    public float typeSpeed = 0.03f;
    public float fadeDuration = 0.2f;

    [Header("Dialogue")]
    public DialogueLine[] dialogueLines;

    private int currentLineIndex = 0;
    private bool dialogueActive = false;
    private bool introFinished = false;
    private bool isTyping = false;
    private bool isTransitioning = false;

    // 点击
    private bool pendingClick = false;

    private Coroutine typingCoroutine;
    private Coroutine transitionCoroutine;
    private Speaker? currentSpeaker = null;

    void Start()
    {
        SafeClearAllText();

        if (patientBubble) patientBubble.SetActive(false);
        if (doctorBubble) doctorBubble.SetActive(false);

        SetBubbleAlpha(patientBubbleImage, patientText, 0f);
        SetBubbleAlpha(doctorBubbleImage, doctorText, 0f);

        if (speechBubbleZone) speechBubbleZone.SetActive(false);
        if (sideBar) sideBar.SetActive(false);

        GameFlow_JFM.LockDrag();
        if (horsePatientEasterEgg) horsePatientEasterEgg.DisableDragging();

        DisableInteractions();

        StartCoroutine(BeginIntroFlow());
    }

    void Update()
    {
        if (!dialogueActive || introFinished) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            // 正在过渡则缓存点击
            if (isTransitioning)
            {
                pendingClick = true;
                return;
            }

            OnDialogueClick();
        }
    }

    IEnumerator BeginIntroFlow()
    {
        yield return new WaitForSeconds(introAnimationDuration);
        yield return new WaitForSeconds(delayBeforeDialogue);
        StartDialogue();
    }

    void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            FinishIntro();
            return;
        }

        SafeClearAllText();

        currentLineIndex = 0;
        dialogueActive = true;
        currentSpeaker = null;

        transitionCoroutine = StartCoroutine(ShowCurrentLineRoutine());
    }

    void OnDialogueClick()
    {
        if (isTransitioning) return;

        if (isTyping)
        {
            CompleteCurrentLineInstantly();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        transitionCoroutine = StartCoroutine(ShowCurrentLineRoutine());
    }

    IEnumerator ShowCurrentLineRoutine()
    {
        isTransitioning = true;

        DialogueLine line = dialogueLines[currentLineIndex];

        // 提前清空
        SafeClearAllText();

        // 强制 UI 刷新
        Canvas.ForceUpdateCanvases();

        // Speaker 切换
        if (currentSpeaker == null)
        {
            yield return StartCoroutine(FadeInSpeaker(line.speaker));
        }
        else if (currentSpeaker.Value != line.speaker)
        {
            yield return StartCoroutine(SwitchSpeaker(line.speaker));
        }

        currentSpeaker = line.speaker;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        // 开始打字
        TMP_Text target = (line.speaker == Speaker.Patient) ? patientText : doctorText;
        typingCoroutine = StartCoroutine(TypeText(target, line.text));

        isTransitioning = false;

        // 如果点过自动执行
        if (pendingClick)
        {
            pendingClick = false;
            OnDialogueClick();
        }
    }

    IEnumerator FadeInSpeaker(Speaker speaker)
    {
        GameObject bubble = (speaker == Speaker.Patient) ? patientBubble : doctorBubble;
        Image img = (speaker == Speaker.Patient) ? patientBubbleImage : doctorBubbleImage;
        TMP_Text txt = (speaker == Speaker.Patient) ? patientText : doctorText;

        if (bubble) bubble.SetActive(true);
        yield return StartCoroutine(FadeBubble(img, txt, 0f, 1f));
    }

    IEnumerator SwitchSpeaker(Speaker newSpeaker)
    {
        GameObject hideBubble = (newSpeaker == Speaker.Patient) ? doctorBubble : patientBubble;
        Image hideImg = (newSpeaker == Speaker.Patient) ? doctorBubbleImage : patientBubbleImage;
        TMP_Text hideTxt = (newSpeaker == Speaker.Patient) ? doctorText : patientText;

        GameObject showBubble = (newSpeaker == Speaker.Patient) ? patientBubble : doctorBubble;
        Image showImg = (newSpeaker == Speaker.Patient) ? patientBubbleImage : doctorBubbleImage;
        TMP_Text showTxt = (newSpeaker == Speaker.Patient) ? patientText : doctorText;

        if (hideBubble)
        {
            yield return StartCoroutine(FadeBubble(hideImg, hideTxt, 1f, 0f));
            hideBubble.SetActive(false);
        }

        if (showBubble) showBubble.SetActive(true);
        yield return StartCoroutine(FadeBubble(showImg, showTxt, 0f, 1f));
    }

    IEnumerator FadeBubble(Image img, TMP_Text txt, float from, float to)
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / fadeDuration);
            SetBubbleAlpha(img, txt, a);
            yield return null;
        }

        SetBubbleAlpha(img, txt, to);
    }

    void SetBubbleAlpha(Image img, TMP_Text txt, float alpha)
    {
        if (img)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }

        if (txt)
        {
            Color c = txt.color;
            c.a = alpha;
            txt.color = c;
        }
    }

    IEnumerator TypeText(TMP_Text target, string fullText)
    {
        isTyping = true;
        target.text = "";

        foreach (char c in fullText)
        {
            target.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    void CompleteCurrentLineInstantly()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        DialogueLine line = dialogueLines[currentLineIndex];
        TMP_Text target = (line.speaker == Speaker.Patient) ? patientText : doctorText;

        target.text = line.text;
        isTyping = false;
    }

    void EndDialogue()
    {
        dialogueActive = false;
        StartCoroutine(EndDialogueRoutine());
    }

    IEnumerator EndDialogueRoutine()
    {
        yield return new WaitForSeconds(0.05f);

        if (patientBubble) patientBubble.SetActive(false);
        if (doctorBubble) doctorBubble.SetActive(false);

        FinishIntro();
    }

    void FinishIntro()
    {
        introFinished = true;

        if (speechBubbleZone) speechBubbleZone.SetActive(true);
        if (sideBar) sideBar.SetActive(true);

        EnableInteractions();
        GameFlow_JFM.UnlockDrag();

        if (tutorialHint) tutorialHint.EnableTutorial();
        if (fixButtonController) fixButtonController.ShowFixButton();
        if (horsePatientEasterEgg) horsePatientEasterEgg.EnableDraggingAtCurrentPosition();
    }

    void SafeClearAllText()
    {
        if (patientText) patientText.text = "";
        if (doctorText) doctorText.text = "";
    }

    void DisableInteractions()
    {
        foreach (var obj in interactionObjects)
        {
            if (!obj) continue;

            var btn = obj.GetComponent<Button>();
            if (btn) btn.interactable = false;

            var col = obj.GetComponent<Collider>();
            if (col) col.enabled = false;

            var col2 = obj.GetComponent<Collider2D>();
            if (col2) col2.enabled = false;
        }
    }

    void EnableInteractions()
    {
        foreach (var obj in interactionObjects)
        {
            if (!obj) continue;

            var btn = obj.GetComponent<Button>();
            if (btn) btn.interactable = true;

            var col = obj.GetComponent<Collider>();
            if (col) col.enabled = true;

            var col2 = obj.GetComponent<Collider2D>();
            if (col2) col2.enabled = true;
        }
    }

    public bool IsIntroFinished()
    {
        return introFinished;
    }
}