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
    public float questionBubbleDuration = 0.8f;

    public GameObject doctorReactionBubble;
    public TMP_Text doctorReactionText;

    [TextArea(2, 3)]
    public string doctorLine1 = "Wait... where did the patient go?";

    [TextArea(2, 3)]
    public string doctorLine2 = "Was that a Ferrari joke just now?";

    public float typeSpeed = 0.03f;
    public float delayBetweenLines = 0.15f;

    [Header("Pennant Visual")]
    public SpriteRenderer pennantSpriteRenderer;
    public Sprite pennantNormalSprite;
    public Sprite pennantHorseSprite;

    [Header("Ferrari FX")]
    public GameObject ferrariFlashFX;
    public Transform ferrariFlashSpawnPoint;

    [Header("Optional Global Lock")]
    public bool disableAllDraggingAfterEasterEgg = true;

    private Camera cam;
    private Collider2D col;
    private Vector3 dragOffset;
    private Vector3 homePos;
    private DraggableItem2D draggableItem;

    private bool isDragging = false;
    private bool easterEggTriggered = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();
        draggableItem = GetComponent<DraggableItem2D>();
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

        if (pennantSpriteRenderer != null && pennantNormalSprite != null)
            pennantSpriteRenderer.sprite = pennantNormalSprite;
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
            if (col.OverlapPoint(p))
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
                StopCoroutineSafe(ref typingCoroutine);
                StartCoroutine(SnapBackRoutine());
            }
        }
    }

    public void EnableDraggingAtCurrentPosition()
    {
        homePos = transform.position;
        draggingEnabled = true;

        if (draggableItem != null)
            draggableItem.SetHomePosition(transform.position);

        Debug.Log("Horse drag enabled at final pos: " + transform.position);
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
        {
            GameFlow_JFM.LockDrag();
        }

        // 关掉正常 puzzle 路线
        if (speechBubbleZone != null)
            speechBubbleZone.SetActive(false);

        // 锦旗换图
        if (pennantSpriteRenderer != null && pennantHorseSprite != null)
            pennantSpriteRenderer.sprite = pennantHorseSprite;

        // 锦旗位置闪光
        if (ferrariFlashFX != null)
        {
            Vector3 fxPos = ferrariFlashSpawnPoint != null ? ferrariFlashSpawnPoint.position : transform.position;
            Instantiate(ferrariFlashFX, fxPos, Quaternion.identity);
        }

        // 马消失，表现为“被吸进法拉利彩蛋里了”
        gameObject.SetActive(false);

        // 弹出法拉利视窗
        if (ferrariPopupPanel != null)
            ferrariPopupPanel.SetActive(true);
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
        // 1. 先冒问号
        if (doctorQuestionBubble != null)
            doctorQuestionBubble.SetActive(true);

        yield return new WaitForSeconds(questionBubbleDuration);

        if (doctorQuestionBubble != null)
            doctorQuestionBubble.SetActive(false);

        // 2. 再显示正式对话气泡
        if (doctorReactionBubble != null)
            doctorReactionBubble.SetActive(true);

        // 第一行
        if (doctorReactionText != null)
            yield return StartCoroutine(TypeText(doctorReactionText, doctorLine1));

        yield return StartCoroutine(WaitForAdvanceInput());

        yield return new WaitForSeconds(delayBetweenLines);

        // 第二行
        if (doctorReactionText != null)
            yield return StartCoroutine(TypeText(doctorReactionText, doctorLine2));

        yield return StartCoroutine(WaitForAdvanceInput());

        // 3. 气泡消失
        if (doctorReactionBubble != null)
            doctorReactionBubble.SetActive(false);

        // 4. 只给继续下一关
        if (continueButton != null)
            continueButton.SetActive(true);
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

    void StopCoroutineSafe(ref Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }
}