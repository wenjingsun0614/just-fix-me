using System.Collections;
using UnityEngine;

public class PatientEntranceOnce : MonoBehaviour
{
    [Header("Path (required)")]
    public Transform startPoint;      // 画面外起点（必须是角色的“同级”，不要做子物体）
    public Transform targetPoint;     // 停留终点（同级）

    [Header("Movement")]
    public float walkSpeed = 2.0f;     // world units/sec
    public float stopEpsilonX = 0.05f; // 到点阈值，适当大一点更容易及时停下
    public bool autoFlipX = true;      // 根据移动方向自动 flipX
    public SpriteRenderer spriteRenderer; // 可留空自动找

    [Header("Animator")]
    public Animator animator;              // 可留空自动找
    public string walkingBool = "Walking"; // Animator 里 bool 参数名（必须一致）

    [Header("Playback")]
    public bool playOnStart = true;

    // 本次按下 Play 期间，只播一次（重新按 Play 会再播）
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
            // 如果已经播过，就直接放到终点，并允许拖拽
            SnapToTarget();
            SetWalking(false);
            GameFlow_JFM.UnlockDrag();
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
            GameFlow_JFM.UnlockDrag();
            yield break;
        }

        // 1) 锁住游戏拖拽
        GameFlow_JFM.LockDrag();

        // 2) 瞬移到起点
        transform.position = startPoint.position;

        // 锁定高度：使用起点的Y，避免走斜线/飘起来
        float lockedY = transform.position.y;

        // 3) 开始走路动画
        SetWalking(true);

        float targetX = targetPoint.position.x;
        float speed = Mathf.Max(0.01f, walkSpeed);

        // 4) 只沿 X 移动，直到到达 targetX
        while (Mathf.Abs(transform.position.x - targetX) > stopEpsilonX)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.MoveTowards(pos.x, targetX, speed * Time.deltaTime);
            pos.y = lockedY;
            transform.position = pos;

            // 自动翻转朝向
            if (autoFlipX && spriteRenderer != null)
            {
                float dir = targetX - pos.x; // >0 往右，<0 往左
                spriteRenderer.flipX = dir < 0f;
            }

            yield return null;
        }

        // 5) 强制吸到终点
        Vector3 finalPos = transform.position;
        finalPos.x = targetX;
        finalPos.y = lockedY;
        transform.position = finalPos;

        // 6) 立即停止走路动画
        SetWalking(false);

        // 7) 解锁拖拽（游戏开始）
        GameFlow_JFM.UnlockDrag();

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

        Vector3 pos = targetPoint.position;
        transform.position = pos;
    }

    // 以后如果想手动重播入场可以调用
    public void Replay()
    {
        if (co != null) StopCoroutine(co);
        playedThisRun = false;
        Start();
    }
}