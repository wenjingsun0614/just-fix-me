using System.Collections;
using UnityEngine;

public class PatientEntranceOnce : MonoBehaviour
{
    [Header("Path (required)")]
    public Transform startPoint;
    public Transform targetPoint;

    [Header("Movement")]
    public float walkSpeed = 2.0f;
    public float stopEpsilonX = 0.05f;
    public bool autoFlipX = true;
    public SpriteRenderer spriteRenderer;

    [Header("Animator")]
    public Animator animator;
    public string walkingBool = "Walking";

    [Header("Playback")]
    public bool playOnStart = true;

    private static bool playedThisRun = false;
    private Coroutine co;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (!playOnStart) return;

        if (playedThisRun)
        {
            SnapToTarget();
            SetWalking(false);
            return;
        }

        co = StartCoroutine(EntranceRoutine());
    }

    IEnumerator EntranceRoutine()
    {
        playedThisRun = true;

        if (startPoint == null || targetPoint == null)
        {
            Debug.LogWarning("[PatientEntranceOnce] startPoint/targetPoint not assigned. Skip entrance.");
            SnapToTarget();
            SetWalking(false);
            yield break;
        }

        // 进场期间锁拖拽
        GameFlow_JFM.LockDrag();

        transform.position = startPoint.position;
        float lockedY = transform.position.y;

        SetWalking(true);

        float targetX = targetPoint.position.x;
        float speed = Mathf.Max(0.01f, walkSpeed);

        while (Mathf.Abs(transform.position.x - targetX) > stopEpsilonX)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.MoveTowards(pos.x, targetX, speed * Time.deltaTime);
            pos.y = lockedY;
            transform.position = pos;

            if (autoFlipX && spriteRenderer != null)
            {
                float dir = targetX - pos.x;
                spriteRenderer.flipX = dir < 0f;
            }

            yield return null;
        }

        Vector3 finalPos = transform.position;
        finalPos.x = targetX;
        finalPos.y = lockedY;
        transform.position = finalPos;

        SetWalking(false);

        // 这里不要再 UnlockDrag，交给 DayIntroController
        co = null;
    }

    void SetWalking(bool walking)
    {
        if (animator == null) return;
        animator.SetBool(walkingBool, walking);
    }

    void SnapToTarget()
    {
        if (targetPoint == null) return;
        transform.position = targetPoint.position;
    }

    public void Replay()
    {
        if (co != null) StopCoroutine(co);
        playedThisRun = false;
        Start();
    }
}