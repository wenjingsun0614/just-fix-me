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

    // ✅ 当前“装备在病人身上”的物品（用于替换时让上一个回架子）
    private DraggableItem2D currentEquippedItem;

    // 过关状态
    private bool canFinish = false;
    private bool resultShown = false;
    private Coroutine arrowCo;

    void Start()
    {
        HideCanvasGroupImmediate(nextArrow);
        HideCanvasGroupImmediate(resultPanel);
    }

    public bool RegisterCorrectItem(DraggableItem2D item)
    {
        if (item == null) return false;

        int id = item.GetInstanceID();
        bool isNew = unlockedIds.Add(id);

        if (isNew && sideBarUI != null)
        {
            sideBarUI.RevealNextWithIcon(item.GetSprite(), playAnim: true);
        }

        if (unlockedIds.Count >= 1)
            ShowNextArrow();

        return isNew;
    }

    /// <summary>
    /// 在病人身上显示/替换物品展示副本
    /// 同时实现：
    /// - 当前物品从架子消失
    /// - 上一个装备的物品回到架子原位并可再次拖
    /// </summary>
    public void ShowOnPatient(DraggableItem2D item)
    {
        if (item == null) return;

        // ✅ 替换：先让上一个装备回架子
        if (replaceOnPatient && currentEquippedItem != null && currentEquippedItem != item)
        {
            currentEquippedItem.ShowInWorldAtHome();
        }

        // ✅ 当前装备：从架子消失
        item.HideInWorld();
        currentEquippedItem = item;

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

    private void ShowNextArrow()
    {
        if (nextArrow == null) return;
        if (canFinish) return;

        canFinish = true;

        if (arrowCo != null) StopCoroutine(arrowCo);
        arrowCo = StartCoroutine(FadeCanvasGroup(nextArrow, nextArrow.alpha, 1f, arrowFadeTime, true));
    }

    public void OnClickNextArrow()
    {
        if (!canFinish) return;
        if (resultShown) return;

        resultShown = true;

        if (resultPanel != null)
            StartCoroutine(FadeCanvasGroup(resultPanel, resultPanel.alpha, 1f, resultFadeTime, true));

        if (nextArrow != null)
            StartCoroutine(FadeCanvasGroup(nextArrow, nextArrow.alpha, 0f, 0.15f, false));
    }

    public void OnClickKeepExploring()
    {
        CloseResultPanel();
    }

    public void OnClickNextDay()
    {
        Debug.Log("Next Day clicked (placeholder)");
    }

    public void CloseResultPanel()
    {
        if (resultPanel != null)
            StartCoroutine(FadeCanvasGroup(resultPanel, resultPanel.alpha, 0f, 0.15f, false));

        resultShown = false;

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