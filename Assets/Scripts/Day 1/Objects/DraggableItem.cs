using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableItem2D : MonoBehaviour
{
    public bool disableScaleReset = false;

    [Header("Drag")]
    public float snapBackTime = 0.18f;

    [Header("Render Priority (Drag on Top)")]
    public int dragSortingBoost = 50;
    private int originalSortingOrder;

    [Header("Drop Check")]
    public Collider2D dropZoneCollider;
    public bool isCorrectItem = false;

    [Header("Drop Zone FX")]
    public DropZoneFX dropZoneFX;

    [Header("Success FX")]
    public Transform bubbleCenter;
    public Transform patientAttachPoint;
    public float snapToCenterTime = 0.10f;
    public float popScale = 1.15f;
    public float popTime = 0.08f;
    public float returnTime = 0.10f;
    public float fadeOutTime = 0.12f;
    public float fadeInTime = 0.18f;

    [Header("Game Manager")]
    public GameManager_JFM gameManager;

    [Header("SideBar (legacy - optional)")]
    public SideBarUI sideBarUI;
    public int sideBarIndex = 0;

    [Header("Reappear")]
    public float reappearFadeTime = 0.18f;

    [Header("Day1 Tutorial Hint (optional)")]
    public Day1HoverTutorialHint tutorialHint;

    private Vector3 startPos;
    private Vector3 homePos;
    private Vector3 startScale;

    private bool dragging;
    private Vector3 dragOffset;

    private Camera cam;
    private Collider2D col;
    private SpriteRenderer sr;
    private Coroutine co;

    private Component displayScaler;
    private float displayScaleMultiplier = 1f;

    private BalloonSpecialBehaviour balloonSpecial;

    public bool IsDragging => dragging;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        startPos = transform.position;
        homePos = startPos;
        startScale = transform.localScale;

        if (sr != null) originalSortingOrder = sr.sortingOrder;

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager_JFM>();

        displayScaler = GetComponent("ItemDisplayScaler");
        if (displayScaler != null)
        {
            var type = displayScaler.GetType();
            var field = type.GetField("displayScaleMultiplier");
            if (field != null && field.FieldType == typeof(float))
            {
                displayScaleMultiplier = Mathf.Max(0.01f, (float)field.GetValue(displayScaler));
            }
        }

        balloonSpecial = GetComponent<BalloonSpecialBehaviour>();
    }

    void Update()
    {
        if (cam == null) return;
        if (!GameFlow_JFM.CanDrag) return;
        if (sr != null && !sr.enabled) return;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = transform.position.z;

        Vector2 p = new Vector2(world.x, world.y);

        bool isHoveringThisItem = false;
        if (col != null && col.enabled)
        {
            isHoveringThisItem = col.OverlapPoint(p);
        }

        // 对话结束后，第一次 hover 任意可拖物体时显示提示
        if (!dragging && isHoveringThisItem && tutorialHint != null && tutorialHint.CanShowOnHover())
        {
            tutorialHint.ShowHint();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (col != null && col.enabled && col.OverlapPoint(p))
            {
                dragging = true;
                SetDragLayer(true);
                dragOffset = transform.position - world;

                // 玩家真正开始拖动后，提示立刻消失且不再出现
                if (tutorialHint != null)
                {
                    tutorialHint.DismissPermanently();
                }

                if (co != null) StopCoroutine(co);
            }
        }

        if (dragging && Input.GetMouseButton(0))
        {
            transform.position = world + dragOffset;
        }

        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;
            SetDragLayer(false);

            if (co != null) StopCoroutine(co);

            Vector2 itemCenter = col.bounds.center;
            bool inZone = (dropZoneCollider != null) && dropZoneCollider.OverlapPoint(itemCenter);

            if (!inZone)
            {
                co = StartCoroutine(SnapBack());
                return;
            }

            if (isCorrectItem)
            {
                CloudDriftInArea cloud = GetComponent<CloudDriftInArea>();
                if (cloud != null)
                {
                    cloud.StopFloating();
                }

                co = StartCoroutine(SuccessSequence());
            }
            else
            {
                BalloonInflationItem balloon = GetComponent<BalloonInflationItem>();
                if (balloon != null && !balloon.HasCompletedInflation())
                {
                    balloon.TriggerMiniGame();
                    return;
                }

                co = StartCoroutine(SnapBack());
            }
        }
    }

    IEnumerator SuccessSequence()
    {
        if (col != null) col.enabled = false;
        SetDragLayer(false);

        if (bubbleCenter != null)
            yield return MoveTo(transform.position, bubbleCenter.position, snapToCenterTime);

        Vector3 displayBaseScale = startScale * displayScaleMultiplier;

        transform.localScale = displayBaseScale;
        yield return ScaleTo(displayBaseScale * popScale, popTime);
        yield return ScaleTo(displayBaseScale, returnTime);

        if (dropZoneFX != null) dropZoneFX.PlaySuccess();

        if (gameManager != null)
        {
            gameManager.RegisterCorrectItem(this);
        }
        else
        {
            if (sideBarUI != null) sideBarUI.SetFound(sideBarIndex, true);
        }

        if (balloonSpecial != null)
        {
            balloonSpecial.OnAcceptedIntoBubble();
        }

        yield return FadeTo(0f, fadeOutTime);

        transform.localScale = displayBaseScale;

        if (gameManager != null)
            gameManager.ShowOnPatient(this);
        else
            SpawnPlacedFallback();

        ResetVisualState();
    }

    void ResetVisualState()
    {
        if (!disableScaleReset)
            transform.localScale = startScale;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }

    void SetDragLayer(bool draggingNow)
    {
        if (sr == null) return;

        if (draggingNow)
            sr.sortingOrder = originalSortingOrder + dragSortingBoost;
        else
            sr.sortingOrder = originalSortingOrder;
    }

    public void HideInWorld()
    {
        dragging = false;
        SetDragLayer(false);

        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;
    }

    public void TriggerSuccessAfterMiniGame()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(SuccessSequence());
    }

    public void ShowInWorldAtHome()
    {
        if (balloonSpecial != null)
        {
            bool handled = balloonSpecial.HandleShowInWorldAtHome(this);
            if (handled) return;
        }

        ReturnHome();

        CloudDragMaskSwitch cloudMask = GetComponent<CloudDragMaskSwitch>();
        if (cloudMask != null)
        {
            cloudMask.RestoreMask();
        }

        CloudDriftInArea cloudDrift = GetComponent<CloudDriftInArea>();
        if (cloudDrift != null)
        {
            cloudDrift.ResumeFloating();
        }

        BalloonInflationItem balloon = GetComponent<BalloonInflationItem>();
        if (balloon != null)
        {
            balloon.ApplyCompletedShelfPoseIfNeeded();
            GetComponent<DraggableItem2D>().disableScaleReset = true;
        }

        if (sr != null)
        {
            sr.enabled = true;
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        if (col != null) col.enabled = false;

        StartCoroutine(FadeInAtHome());
    }

    IEnumerator FadeInAtHome()
    {
        if (sr == null) yield break;

        float t = 0f;
        Color c = sr.color;

        while (t < reappearFadeTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / reappearFadeTime);
            c.a = p;
            sr.color = c;
            yield return null;
        }

        c.a = 1f;
        sr.color = c;

        if (col != null) col.enabled = true;
    }

    void ReturnHome()
    {
        transform.position = homePos;

        if (!disableScaleReset)
            transform.localScale = startScale;
    }

    void SpawnPlacedFallback()
    {
        if (patientAttachPoint == null || sr == null) return;

        GameObject placed = new GameObject(name + "_Placed");
        placed.transform.position = patientAttachPoint.position;
        placed.transform.localScale = startScale * displayScaleMultiplier;

        var placedSR = placed.AddComponent<SpriteRenderer>();
        placedSR.sprite = sr.sprite;
        placedSR.sortingLayerID = sr.sortingLayerID;
        placedSR.sortingOrder = sr.sortingOrder + 1;
    }

    IEnumerator SnapBack()
    {
        yield return FadeTo(1f, 0.05f);

        if (!disableScaleReset)
        {
            transform.localScale = startScale;
        }

        yield return MoveTo(transform.position, homePos, snapBackTime);

        CloudDriftInArea cloud = GetComponent<CloudDriftInArea>();
        if (cloud != null)
        {
            cloud.ResumeFloating();
        }
    }

    IEnumerator MoveTo(Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f) { transform.position = to; yield break; }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            p = 1f - Mathf.Pow(1f - p, 3f);
            transform.position = Vector3.Lerp(from, to, p);
            yield return null;
        }
        transform.position = to;
    }

    IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 from = transform.localScale;
        if (duration <= 0f) { transform.localScale = target; yield break; }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            p = 1f - Mathf.Pow(1f - p, 3f);
            transform.localScale = Vector3.Lerp(from, target, p);
            yield return null;
        }
        transform.localScale = target;
    }

    IEnumerator FadeTo(float alpha, float duration)
    {
        if (sr == null) yield break;

        Color from = sr.color;
        Color to = sr.color;
        to.a = alpha;

        if (duration <= 0f) { sr.color = to; yield break; }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            sr.color = Color.Lerp(from, to, p);
            yield return null;
        }
        sr.color = to;
    }

    public void SetHomePosition(Vector3 newHomePos)
    {
        homePos = newHomePos;
    }

    public Vector3 GetHomePosition()
    {
        return homePos;
    }

    public Vector3 GetOriginalStartPosition()
    {
        return startPos;
    }

    public SpriteRenderer GetSpriteRenderer() => sr;
    public Collider2D GetCollider2D() => col;

    public Sprite GetSprite() => sr != null ? sr.sprite : null;
    public int GetSortingLayerID() => sr != null ? sr.sortingLayerID : 0;
    public int GetSortingOrder() => sr != null ? sr.sortingOrder : 0;
}