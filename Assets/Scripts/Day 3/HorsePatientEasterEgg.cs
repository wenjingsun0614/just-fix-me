using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HorsePatientEasterEgg : MonoBehaviour
{
    [Header("Drag")]
    public bool draggingEnabled = false;
    public float snapBackTime = 0.15f;

    [Header("Ferrari Drop")]
    public Collider2D pennantDropZone;

    [Header("UI / Flow")]
    public GameObject speechBubbleZone;
    public GameObject ferrariPopupPanel;
    public GameObject continueButton;

    [Header("Doctor Reaction")]
    public GameObject doctorQuestionBubble;
    public CanvasGroup doctorQuestionCanvasGroup;
    public float questionBubbleDuration = 0.8f;

    public GameObject doctorReactionBubble;
    public TMP_Text doctorReactionText;

    [TextArea(2, 3)]
    public string doctorLine1 = "Wait... where did the patient go?";

    [TextArea(2, 3)]
    public string doctorLine2 = "Was that a Ferrari joke just now?";

    public float typeSpeed = 0.03f;
    public float delayBetweenLines = 0.15f;

    [Header("Question Pop FX")]
    public float questionPopDuration = 0.18f;
    public Vector3 questionStartScale = new Vector3(0.7f, 0.7f, 1f);
    public Vector3 questionOvershootScale = new Vector3(1.12f, 1.12f, 1f);

    [Header("Pennant Visual")]
    public SpriteRenderer pennantSpriteRenderer;
    public Sprite pennantNormalSprite;
    public Sprite pennantHorseSprite;

    [Header("Ferrari FX")]
    public GameObject ferrariFlashFX;
    public Transform ferrariFlashSpawnPoint;

    [Header("Special Result")]
    public string specialResultName = "FerrariHorse";

    [Header("Optional Global Lock")]
    public bool disableAllDraggingAfterEasterEgg = true;

    [Header("Optional GameManager")]
    public GameManager_JFM gameManager;

    private Camera cam;
    private Collider2D col;
    private SpriteRenderer horseSpriteRenderer;
    private Animator horseAnimator;

    private Vector3 dragOffset;
    private Vector3 homePos;

    private bool isDragging = false;
    private bool easterEggTriggered = false;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();
        horseSpriteRenderer = GetComponent<SpriteRenderer>();
        horseAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        homePos = transform.position;

        if (ferrariPopupPanel != null)
            ferrariPopupPanel.SetActive(false);

        if (doctorQuestionBubble != null)
            doctorQuestionBubble.SetActive(false);

        if (doctorReactionBubble != null)
            doctorReactionBubble.SetActive(false);

        if (continueButton != null)
            continueButton.SetActive(false);

        if (doctorQuestionCanvasGroup != null)
            doctorQuestionCanvasGroup.alpha = 0f;

        if (pennantSpriteRenderer != null && pennantNormalSprite != null)
            pennantSpriteRenderer.sprite = pennantNormalSprite;

        draggingEnabled = false;
    }

    void Update()
    {
        if (!draggingEnabled) return;
        if (easterEggTriggered) return;
        if (!GameFlow_JFM.CanDrag) return;
        if (cam == null || col == null) return;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = transform.position.z;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 p = new Vector2(world.x, world.y);
            if (col.enabled && col.OverlapPoint(p))
            {
                isDragging = true;
                dragOffset = transform.position - world;
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            transform.position = world + dragOffset;
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            bool inPennantZone = false;

            if (pennantDropZone != null)
            {
                Vector2 center = col.bounds.center;
                inPennantZone = pennantDropZone.OverlapPoint(center);
            }

            if (inPennantZone)
            {
                TriggerFerrariEasterEgg();
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(SnapBackRoutine());
            }
        }
    }

    public void EnableDraggingAtCurrentPosition()
    {
        homePos = transform.position;
        draggingEnabled = true;

        if (col != null)
            col.enabled = true;

        enabled = true;
    }

    public void DisableDragging()
    {
        draggingEnabled = false;
        isDragging = false;
    }

    void TriggerFerrariEasterEgg()
    {
        if (easterEggTriggered) return;

        easterEggTriggered = true;
        draggingEnabled = false;
        isDragging = false;

        if (disableAllDraggingAfterEasterEgg)
            GameFlow_JFM.LockDrag();

        if (speechBubbleZone != null)
            speechBubbleZone.SetActive(false);

        if (pennantSpriteRenderer != null && pennantHorseSprite != null)
            pennantSpriteRenderer.sprite = pennantHorseSprite;

        PlayFerrariFlash();

        if (horseSpriteRenderer != null)
            horseSpriteRenderer.enabled = false;

        if (col != null)
            col.enabled = false;

        if (horseAnimator != null)
            horseAnimator.enabled = false;

        if (ferrariPopupPanel != null)
            ferrariPopupPanel.SetActive(true);
    }

    void PlayFerrariFlash()
    {
        if (ferrariFlashFX == null) return;

        Vector3 fxPos = ferrariFlashSpawnPoint != null ? ferrariFlashSpawnPoint.position : transform.position;
        GameObject fx = Instantiate(ferrariFlashFX, fxPos, Quaternion.identity);

        ParticleSystem[] systems = fx.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in systems)
        {
            ps.Play();
        }
    }

    public void CloseFerrariPopup()
    {
        if (ferrariPopupPanel != null)
            ferrariPopupPanel.SetActive(false);

        StopAllCoroutines();
        StartCoroutine(DoctorReactionSequence());
    }

    IEnumerator DoctorReactionSequence()
    {
        yield return StartCoroutine(ShowQuestionBubblePop());

        yield return new WaitForSeconds(questionBubbleDuration);

        if (doctorQuestionBubble != null)
            doctorQuestionBubble.SetActive(false);

        if (doctorReactionBubble != null)
            doctorReactionBubble.SetActive(true);

        if (doctorReactionText != null)
            yield return StartCoroutine(TypeText(doctorReactionText, doctorLine1));

        yield return StartCoroutine(WaitForAdvanceInput());

        yield return new WaitForSeconds(delayBetweenLines);

        if (doctorReactionText != null)
            yield return StartCoroutine(TypeText(doctorReactionText, doctorLine2));

        yield return StartCoroutine(WaitForAdvanceInput());

        if (doctorReactionBubble != null)
            doctorReactionBubble.SetActive(false);

        // ¹Ø¼ü£º¼¤»î Ferrari override£¬¸²¸ÇÆÕÍ¨Ñ¡ÔñÂß¼­
        if (gameManager != null)
            gameManager.ActivateFerrariOverride(specialResultName);

        if (continueButton != null)
            continueButton.SetActive(true);
    }

    IEnumerator ShowQuestionBubblePop()
    {
        if (doctorQuestionBubble == null) yield break;

        doctorQuestionBubble.SetActive(true);

        RectTransform rt = doctorQuestionBubble.GetComponent<RectTransform>();
        if (rt != null)
            rt.localScale = questionStartScale;

        if (doctorQuestionCanvasGroup != null)
            doctorQuestionCanvasGroup.alpha = 0f;

        float t = 0f;
        while (t < questionPopDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / questionPopDuration);
            float e = 1f - Mathf.Pow(1f - p, 3f);

            if (doctorQuestionCanvasGroup != null)
                doctorQuestionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, e);

            if (rt != null)
            {
                Vector3 scale;
                if (p < 0.7f)
                {
                    float sub = p / 0.7f;
                    scale = Vector3.Lerp(questionStartScale, questionOvershootScale, sub);
                }
                else
                {
                    float sub = (p - 0.7f) / 0.3f;
                    scale = Vector3.Lerp(questionOvershootScale, Vector3.one, sub);
                }

                rt.localScale = scale;
            }

            yield return null;
        }

        if (doctorQuestionCanvasGroup != null)
            doctorQuestionCanvasGroup.alpha = 1f;

        if (rt != null)
            rt.localScale = Vector3.one;
    }

    IEnumerator TypeText(TMP_Text targetText, string fullText)
    {
        if (targetText == null) yield break;

        targetText.text = "";

        foreach (char c in fullText)
        {
            targetText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    IEnumerator WaitForAdvanceInput()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                yield break;

            yield return null;
        }
    }

    IEnumerator SnapBackRoutine()
    {
        Vector3 from = transform.position;
        Vector3 to = homePos;

        float t = 0f;
        while (t < snapBackTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / snapBackTime);
            p = 1f - Mathf.Pow(1f - p, 3f);
            transform.position = Vector3.Lerp(from, to, p);
            yield return null;
        }

        transform.position = to;
    }
}