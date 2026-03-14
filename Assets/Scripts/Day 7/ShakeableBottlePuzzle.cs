using System.Collections;
using UnityEngine;

public class ShakeableDraggableBottle : MonoBehaviour
{
    [Header("Drag")]
    public bool canDrag = true;
    public float followSpeed = 18f;

    [Header("Return Home")]
    public float returnHomeSpeed = 10f;

    [Header("Shake Detection")]
    [Tooltip("Higher = harder to trigger")]
    public float shakeThreshold = 16f;

    [Tooltip("Higher = shake meter drops faster")]
    public float shakeDecay = 6f;

    [Tooltip("Extra shake added when changing left/right direction")]
    public float directionChangeBonus = 2.2f;

    [Tooltip("How much horizontal movement contributes to shake")]
    public float horizontalShakeMultiplier = 45f;

    [Header("Drag Rotation")]
    public float tiltStrength = 80f;
    public float maxTiltAngle = 20f;
    public float tiltSmooth = 12f;
    public float returnRotationSmooth = 8f;

    [Header("Sorting While Dragging")]
    public SpriteRenderer bottleSpriteRenderer;
    public int dragSortingOrder = 999;

    [Header("Burst Result Objects")]
    public GameObject fullBottleObject;
    public GameObject corkObject;
    public GameObject emptyBottleObject;
    public GameObject sprayFXObject;

    [Header("Spawn Points")]
    public Transform emptyBottleSpawnPoint;

    [Header("Sorting")]
    public int corkVisibleSortingOrder = 20;

    [Header("Timing")]
    public float fullBottleFadeOutTime = 0.2f;
    public float sprayFXVisibleTime = 0.18f;

    [Header("Optional Limits")]
    public bool clampYWhileDragging = false;
    public float minY = -10f;
    public float maxY = 10f;

    private Camera cam;
    private bool isDragging = false;
    private bool hasBurst = false;
    private bool isReturningHome = false;

    private Vector3 dragOffset;
    private Vector3 targetWorldPos;
    private Vector3 homePosition;

    private float shakeMeter = 0f;
    private float lastHorizontalDelta = 0f;

    private int originalSortingOrder;
    private Quaternion originalRotation;

    private SpriteRenderer fullBottleSR;
    private SpriteRenderer emptyBottleSR;
    private SpriteRenderer corkSR;
    private Collider2D corkCollider;

    private Vector3 corkHomePos;

    void Awake()
    {
        cam = Camera.main;

        if (bottleSpriteRenderer == null)
            bottleSpriteRenderer = GetComponent<SpriteRenderer>();

        if (bottleSpriteRenderer != null)
            originalSortingOrder = bottleSpriteRenderer.sortingOrder;

        originalRotation = transform.rotation;
        homePosition = transform.position;

        if (fullBottleObject == null)
            fullBottleObject = gameObject;

        if (fullBottleObject != null)
            fullBottleSR = fullBottleObject.GetComponent<SpriteRenderer>();

        if (emptyBottleObject != null)
            emptyBottleSR = emptyBottleObject.GetComponent<SpriteRenderer>();

        if (corkObject != null)
        {
            corkSR = corkObject.GetComponent<SpriteRenderer>();
            corkCollider = corkObject.GetComponent<Collider2D>();
            corkHomePos = corkObject.transform.position;
        }
    }

    void Start()
    {
        homePosition = transform.position;

        // Full bottle fully visible
        if (fullBottleSR != null)
        {
            Color c = Color.white;
            c.a = 1f;
            fullBottleSR.color = c;
        }

        // Empty bottle starts hidden, fully opaque when shown
        if (emptyBottleObject != null)
        {
            if (emptyBottleSR == null)
                emptyBottleSR = emptyBottleObject.GetComponent<SpriteRenderer>();

            if (emptyBottleSR != null)
            {
                Color c = Color.white;
                c.a = 1f;
                emptyBottleSR.color = c;
            }

            emptyBottleObject.SetActive(false);
        }

        // Cork starts hidden, but stays at its placed home position
        if (corkObject != null)
        {
            if (corkSR == null)
                corkSR = corkObject.GetComponent<SpriteRenderer>();

            if (corkCollider == null)
                corkCollider = corkObject.GetComponent<Collider2D>();

            corkHomePos = corkObject.transform.position;

            if (corkSR != null)
            {
                corkSR.color = Color.white;
                corkSR.sortingOrder = corkVisibleSortingOrder;
            }

            corkObject.SetActive(false);

            if (corkCollider != null)
                corkCollider.enabled = false;
        }

        if (sprayFXObject != null)
            sprayFXObject.SetActive(false);
    }

    void Update()
    {
        if (hasBurst) return;

        if (isDragging && canDrag && GameFlow_JFM.CanDrag)
        {
            isReturningHome = false;

            UpdateDrag();
            UpdateShake();
            UpdateTilt();

            if (shakeMeter >= shakeThreshold)
            {
                BurstBottle();
            }
        }
        else
        {
            shakeMeter = Mathf.Max(0f, shakeMeter - shakeDecay * Time.deltaTime);
            lastHorizontalDelta = 0f;

            if (isReturningHome)
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    homePosition,
                    returnHomeSpeed * Time.deltaTime
                );

                if (Vector3.Distance(transform.position, homePosition) < 0.03f)
                {
                    transform.position = homePosition;
                    isReturningHome = false;
                }
            }

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                originalRotation,
                returnRotationSmooth * Time.deltaTime
            );
        }
    }

    void OnMouseDown()
    {
        if (hasBurst) return;
        if (!canDrag) return;
        if (!GameFlow_JFM.CanDrag) return;
        if (cam == null) return;

        isDragging = true;
        isReturningHome = false;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        dragOffset = transform.position - mouseWorld;

        if (bottleSpriteRenderer != null)
            bottleSpriteRenderer.sortingOrder = dragSortingOrder;
    }

    void OnMouseUp()
    {
        StopDraggingAndReturn();
    }

    void OnDisable()
    {
        StopDraggingAndReturn();
    }

    private void StopDraggingAndReturn()
    {
        isDragging = false;

        if (!hasBurst)
        {
            isReturningHome = true;

            if (bottleSpriteRenderer != null)
                bottleSpriteRenderer.sortingOrder = originalSortingOrder;
        }
    }

    private void UpdateDrag()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        targetWorldPos = mouseWorld + dragOffset;

        if (clampYWhileDragging)
            targetWorldPos.y = Mathf.Clamp(targetWorldPos.y, minY, maxY);

        transform.position = Vector3.Lerp(
            transform.position,
            targetWorldPos,
            followSpeed * Time.deltaTime
        );
    }

    private void UpdateShake()
    {
        float horizontalDelta = targetWorldPos.x - transform.position.x;

        shakeMeter += Mathf.Abs(horizontalDelta) * horizontalShakeMultiplier * Time.deltaTime;

        bool changedDirection =
            Mathf.Sign(horizontalDelta) != Mathf.Sign(lastHorizontalDelta) &&
            Mathf.Abs(horizontalDelta) > 0.025f &&
            Mathf.Abs(lastHorizontalDelta) > 0.025f;

        if (changedDirection)
            shakeMeter += directionChangeBonus;

        lastHorizontalDelta = horizontalDelta;
    }

    private void UpdateTilt()
    {
        float tilt = Mathf.Clamp(
            (targetWorldPos.x - transform.position.x) * tiltStrength,
            -maxTiltAngle,
            maxTiltAngle
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(0f, 0f, -tilt),
            tiltSmooth * Time.deltaTime
        );
    }

    private void BurstBottle()
    {
        if (hasBurst) return;

        hasBurst = true;
        isDragging = false;
        isReturningHome = false;

        StartCoroutine(BurstSequence());
    }

    private IEnumerator BurstSequence()
    {
        if (bottleSpriteRenderer != null)
            bottleSpriteRenderer.sortingOrder = originalSortingOrder;

        transform.rotation = originalRotation;

        Vector3 burstPos = transform.position;

        // Put spray FX at current bottle position
        if (sprayFXObject != null)
        {
            sprayFXObject.transform.position = burstPos;
            sprayFXObject.SetActive(true);
        }

        // Fade out full bottle
        if (fullBottleObject != null && fullBottleSR != null)
            StartCoroutine(FadeOutAndDisable(fullBottleObject, fullBottleSR, fullBottleFadeOutTime));
        else if (fullBottleObject != null)
            fullBottleObject.SetActive(false);

        // Show empty bottle on table
        if (emptyBottleObject != null)
        {
            if (emptyBottleSpawnPoint != null)
                emptyBottleObject.transform.position = emptyBottleSpawnPoint.position;

            emptyBottleObject.SetActive(true);

            if (emptyBottleSR != null)
            {
                Color c = Color.white;
                c.a = 1f;
                emptyBottleSR.color = c;
            }
        }

        // Show cork directly at its home position
        if (corkObject != null)
        {
            corkObject.SetActive(true);
            corkObject.transform.position = corkHomePos;
            corkObject.transform.rotation = Quaternion.identity;

            if (corkSR != null)
            {
                corkSR.color = Color.white;
                corkSR.sortingOrder = corkVisibleSortingOrder;
            }

            if (corkCollider != null)
                corkCollider.enabled = true;
        }

        yield return new WaitForSeconds(sprayFXVisibleTime);

        if (sprayFXObject != null)
            sprayFXObject.SetActive(false);
    }

    private IEnumerator FadeOutAndDisable(GameObject obj, SpriteRenderer sr, float duration)
    {
        if (obj == null || sr == null) yield break;

        Color c = sr.color;
        float startAlpha = c.a;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            c.a = Mathf.Lerp(startAlpha, 0f, p);
            sr.color = c;
            yield return null;
        }

        c.a = 0f;
        sr.color = c;
        obj.SetActive(false);
    }
}