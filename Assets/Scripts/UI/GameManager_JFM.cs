using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager_JFM : MonoBehaviour
{
    [Header("UI")]
    public SideBarUI sideBarUI;

    [Header("Next Arrow UI")]
    public CanvasGroup nextArrow;        // 右下角箭头按钮上的 CanvasGroup
    public float arrowFadeTime = 0.25f;

    [Header("Result UI")]
    public CanvasGroup resultPanel;      // 过渡/结算界面 Panel 的 CanvasGroup
    public float resultFadeTime = 0.25f;

    [Header("Patient Attachment")]
    public Transform defaultAttachPoint; // 如果物品没填自己的挂点，就用这个
    public bool replaceOnPatient = true;

    // 记录：哪些正确物品已经“解锁过”
    private HashSet<int> unlockedIds = new HashSet<int>();

    // 病人身上当前显示的那个“副本”
    private GameObject currentPlaced;

    // 过关状态
    private bool canFinish = false;
    private bool resultShown = false;
    private Coroutine arrowCo;

    void Start()
    {
        // 开局：箭头和过渡界面都隐藏（对象保持Active更好做淡入淡出）
        HideCanvasGroupImmediate(nextArrow);
        HideCanvasGroupImmediate(resultPanel);
    }

    /// <summary>
    /// 提交一个正确物品。返回：是否是第一次解锁（决定要不要点亮侧边栏）
    /// </summary>
    public bool RegisterCorrectItem(DraggableItem2D item)
    {
        if (item == null) return false;

        // 用 instanceID 作为“唯一身份”，简单好用
        int id = item.GetInstanceID();
        bool isNew = unlockedIds.Add(id);

        // 首次解锁：侧边栏解锁下一个槽位，并写入 icon
        if (isNew && sideBarUI != null)
        {
            sideBarUI.RevealNextWithIcon(item.GetSprite(), playAnim: true);
        }

        // 解锁任意一个即可过关：显示右下角箭头
        if (unlockedIds.Count >= 1)
            ShowNextArrow();

        return isNew;
    }

    /// <summary>
    /// 在病人身上显示/替换物品展示副本
    /// </summary>
    public void ShowOnPatient(DraggableItem2D item)
    {
        if (item == null) return;

        Transform attach = item.patientAttachPoint != null ? item.patientAttachPoint : defaultAttachPoint;
        if (attach == null) return;

        if (replaceOnPatient && currentPlaced != null)
        {
            Destroy(currentPlaced);
            currentPlaced = null;
        }

        GameObject placed = new GameObject(item.name + "_Placed");
        placed.transform.position = attach.position;
        placed.transform.rotation = attach.rotation;
        placed.transform.localScale = item.transform.localScale;

        var placedSR = placed.AddComponent<SpriteRenderer>();
        placedSR.sprite = item.GetSprite();
        placedSR.sortingLayerID = item.GetSortingLayerID();
        placedSR.sortingOrder = item.GetSortingOrder() + 1;

        currentPlaced = placed;
    }

    /// <summary>
    /// 解锁>=1后调用：显示右下角箭头（渐入）
    /// </summary>
    private void ShowNextArrow()
    {
        if (nextArrow == null) return;
        if (canFinish) return; // 已经显示过就不重复

        canFinish = true;

        if (arrowCo != null) StopCoroutine(arrowCo);
        arrowCo = StartCoroutine(FadeCanvasGroup(nextArrow, nextArrow.alpha, 1f, arrowFadeTime, true));
    }

    /// <summary>
    /// 绑定到 NextArrowButton 的 OnClick：打开过渡界面
    /// </summary>
    public void OnClickNextArrow()
    {
        if (!canFinish) return;
        if (resultShown) return;

        resultShown = true;

        // 弹出过渡界面
        if (resultPanel != null)
            StartCoroutine(FadeCanvasGroup(resultPanel, resultPanel.alpha, 1f, resultFadeTime, true));

        // 隐藏箭头，避免重复点（关闭面板时会再显示回来）
        if (nextArrow != null)
            StartCoroutine(FadeCanvasGroup(nextArrow, nextArrow.alpha, 0f, 0.15f, false));
    }

    /// <summary>
    /// 过渡界面按钮：继续探索（关闭面板，回到游戏）
    /// </summary>
    public void OnClickKeepExploring()
    {
        CloseResultPanel();
    }

    /// <summary>
    /// 过渡界面按钮：进入下一天（现在先占位）
    /// 以后你有 Day2 场景时，把 Debug.Log 换成 SceneManager.LoadScene(...)
    /// </summary>
    public void OnClickNextDay()
    {
        Debug.Log("Next Day clicked (placeholder)");
    }

    /// <summary>
    /// 关掉过渡面板：继续探索
    /// </summary>
    public void CloseResultPanel()
    {
        // 关掉过渡面板
        if (resultPanel != null)
            StartCoroutine(FadeCanvasGroup(resultPanel, resultPanel.alpha, 0f, 0.15f, false));

        // 允许再次打开（继续探索后仍可点箭头再次打开面板）
        resultShown = false;

        // ✅ 关键：关闭面板后，箭头保持显示（一直存在）
        if (nextArrow != null && canFinish)
            StartCoroutine(FadeCanvasGroup(nextArrow, nextArrow.alpha, 1f, 0.15f, true));
    }

    // ----------------- Helpers -----------------

    private void HideCanvasGroupImmediate(CanvasGroup g)
    {
        if (g == null) return;
        g.alpha = 0f;
        g.interactable = false;
        g.blocksRaycasts = false;
        g.gameObject.SetActive(true);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup g, float from, float to, float time, bool enableInteract)
    {
        if (g == null) yield break;

        g.gameObject.SetActive(true);

        if (time <= 0f)
        {
            g.alpha = to;
            g.interactable = enableInteract;
            g.blocksRaycasts = enableInteract;
            yield break;
        }

        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / time);
            g.alpha = Mathf.Lerp(from, to, p);
            yield return null;
        }

        g.alpha = to;
        g.interactable = enableInteract && to > 0.001f;
        g.blocksRaycasts = enableInteract && to > 0.001f;
    }
}