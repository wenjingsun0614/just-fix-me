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

    [Header("Special Character")]
    public HorsePatientEasterEgg horsePatientEasterEgg;

    [Header("Disable Interaction During Intro")]
    [Tooltip("Drag objects here. They stay visible, but their Button / Collider / Collider2D will be disabled during intro.")]
    public GameObject[] interactionObjects;

    [Header("Intro Timing")]
    public float introAnimationDuration = 2f;
    public float delayBeforeDialogue = 0.2f;

    [Header("Typewriter")]
    public float typeSpeed = 0.03f;

    [Header("Fade")]
    public float fadeDuration = 0.2f;

    [Header("Dialogue")]
    public DialogueLine[] dialogueLines;

    private int currentLineIndex = 0;
    private bool dialogueActive = false;
    private bool introFinished = false;
    private bool isTyping = false;

    private Coroutine typingCoroutine;
    private Coroutine transitionCoroutine;
    private Speaker? currentSpeaker = null;

    void Start()
    {
        if (patientBubble != null) patientBubble.SetActive(false);
        if (doctorBubble != null) doctorBubble.SetActive(false);

        SetBubbleAlpha(patientBubbleImage, patientText, 0f);
        SetBubbleAlpha(doctorBubbleImage, doctorText, 0f);

        if (speechBubbleZone != null)
            speechBubbleZone.SetActive(false);

        if (sideBar != null)
            sideBar.SetActive(false);

        // 开场默认锁拖拽
        GameFlow_JFM.LockDrag();

        // 马病人彩蛋拖拽也默认关闭
        if (horsePatientEasterEgg != null)
            horsePatientEasterEgg.DisableDragging();

        DisableInteractions();

        StartCoroutine(BeginIntroFlow());
    }

    void Update()
    {
        if (!dialogueActive || introFinished) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
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

        currentLineIndex = 0;
        dialogueActive = true;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(ShowCurrentLineRoutine());
    }

    void OnDialogueClick()
    {
        if (dialogueLines == null || currentLineIndex >= dialogueLines.Length)
            return;

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

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(ShowCurrentLineRoutine());
    }

    IEnumerator ShowCurrentLineRoutine()
    {
        DialogueLine line = dialogueLines[currentLineIndex];

        if (currentSpeaker == null)
        {
            yield return StartCoroutine(FadeInSpeaker(line.speaker));
            currentSpeaker = line.speaker;
        }
        else if (currentSpeaker.Value != line.speaker)
        {
            yield return StartCoroutine(SwitchSpeaker(line.speaker));
            currentSpeaker = line.speaker;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (line.speaker == Speaker.Patient)
        {
            if (patientText != null)
            {
                patientText.text = "";
                typingCoroutine = StartCoroutine(TypeText(patientText, line.text));
            }
        }
        else
        {
            if (doctorText != null)
            {
                doctorText.text = "";
                typingCoroutine = StartCoroutine(TypeText(doctorText, line.text));
            }
        }
    }

    IEnumerator FadeInSpeaker(Speaker speaker)
    {
        if (speaker == Speaker.Patient)
        {
            if (patientBubble != null) patientBubble.SetActive(true);
            yield return StartCoroutine(FadeBubble(patientBubbleImage, patientText, 0f, 1f));
        }
        else
        {
            if (doctorBubble != null) doctorBubble.SetActive(true);
            yield return StartCoroutine(FadeBubble(doctorBubbleImage, doctorText, 0f, 1f));
        }
    }

    IEnumerator SwitchSpeaker(Speaker newSpeaker)
    {
        if (newSpeaker == Speaker.Patient)
        {
            yield return StartCoroutine(FadeBubble(doctorBubbleImage, doctorText, 1f, 0f));
            if (doctorBubble != null) doctorBubble.SetActive(false);

            if (patientBubble != null) patientBubble.SetActive(true);
            yield return StartCoroutine(FadeBubble(patientBubbleImage, patientText, 0f, 1f));
        }
        else
        {
            yield return StartCoroutine(FadeBubble(patientBubbleImage, patientText, 1f, 0f));
            if (patientBubble != null) patientBubble.SetActive(false);

            if (doctorBubble != null) doctorBubble.SetActive(true);
            yield return StartCoroutine(FadeBubble(doctorBubbleImage, doctorText, 0f, 1f));
        }
    }

    IEnumerator FadeBubble(Image bubbleImage, TMP_Text bubbleText, float from, float to)
    {
        float t = 0f;

        SetBubbleAlpha(bubbleImage, bubbleText, from);

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / fadeDuration);
            SetBubbleAlpha(bubbleImage, bubbleText, a);
            yield return null;
        }

        SetBubbleAlpha(bubbleImage, bubbleText, to);
    }

    void SetBubbleAlpha(Image bubbleImage, TMP_Text bubbleText, float alpha)
    {
        if (bubbleImage != null)
        {
            Color c = bubbleImage.color;
            c.a = alpha;
            bubbleImage.color = c;
        }

        if (bubbleText != null)
        {
            Color c = bubbleText.color;
            c.a = alpha;
            bubbleText.color = c;
        }
    }

    IEnumerator TypeText(TMP_Text targetText, string fullText)
    {
        if (targetText == null) yield break;

        isTyping = true;
        targetText.text = "";

        foreach (char c in fullText)
        {
            targetText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    void CompleteCurrentLineInstantly()
    {
        if (dialogueLines == null || currentLineIndex >= dialogueLines.Length)
            return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        DialogueLine line = dialogueLines[currentLineIndex];

        if (line.speaker == Speaker.Patient)
        {
            if (patientText != null)
                patientText.text = line.text;
        }
        else
        {
            if (doctorText != null)
                doctorText.text = line.text;
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        dialogueActive = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        StartCoroutine(EndDialogueRoutine());
    }

    IEnumerator EndDialogueRoutine()
    {
        if (currentSpeaker == Speaker.Patient)
        {
            yield return StartCoroutine(FadeBubble(patientBubbleImage, patientText, 1f, 0f));
            if (patientBubble != null) patientBubble.SetActive(false);
        }
        else if (currentSpeaker == Speaker.Doctor)
        {
            yield return StartCoroutine(FadeBubble(doctorBubbleImage, doctorText, 1f, 0f));
            if (doctorBubble != null) doctorBubble.SetActive(false);
        }

        FinishIntro();
    }

    void FinishIntro()
    {
        introFinished = true;

        if (speechBubbleZone != null)
            speechBubbleZone.SetActive(true);

        if (sideBar != null)
            sideBar.SetActive(true);

        EnableInteractions();

        // 对话结束后，才允许全局拖拽
        GameFlow_JFM.UnlockDrag();

        // 对话结束后，马病人才开始可拖
        if (horsePatientEasterEgg != null)
            horsePatientEasterEgg.EnableDraggingAtCurrentPosition();
    }

    void DisableInteractions()
    {
        if (interactionObjects == null) return;

        foreach (GameObject obj in interactionObjects)
        {
            if (obj == null) continue;

            Button btn = obj.GetComponent<Button>();
            if (btn != null)
                btn.interactable = false;

            Collider col = obj.GetComponent<Collider>();
            if (col != null)
                col.enabled = false;

            Collider2D col2 = obj.GetComponent<Collider2D>();
            if (col2 != null)
                col2.enabled = false;

            MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != null && script != this)
                    script.enabled = false;
            }
        }
    }

    void EnableInteractions()
    {
        if (interactionObjects == null) return;

        foreach (GameObject obj in interactionObjects)
        {
            if (obj == null) continue;

            Button btn = obj.GetComponent<Button>();
            if (btn != null)
                btn.interactable = true;

            Collider col = obj.GetComponent<Collider>();
            if (col != null)
                col.enabled = true;

            Collider2D col2 = obj.GetComponent<Collider2D>();
            if (col2 != null)
                col2.enabled = true;

            MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != null && script != this)
                    script.enabled = true;
            }
        }
    }
}