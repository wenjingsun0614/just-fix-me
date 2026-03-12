using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_JFM : MonoBehaviour
{
    [Header("Day Info")]
    public int currentDay = 1;

    [Header("UI")]
    public SideBarUI sideBarUI;

    [Header("Next Arrow UI")]
    public CanvasGroup nextArrow;        // 右下角箭头按钮上的 CanvasGroup
    public float arrowFadeTime = 0.25f;

    [Header("Result UI")]
    public CanvasGroup resultPanel;      // 过渡/结算界面 Panel 的 CanvasGroup
    public float resultFadeTime = 0.25f;

    [Header("Item Selection UI")]
    public ItemSelectionPanelUI itemSelectionPanel;  // 最终物品选择面板

    [Header("Patient Attachment")]
    public Transform defaultAttachPoint; // 如果物品没填自己的挂点，就用这个
    public bool replaceOnPatient = true;

    // 记录：哪些正确物品已经“解锁过”
    private HashSet<int> unlockedIds = new HashSet<int>();

    // 记录：已解锁的正确物品（给最终选择面板用）
    private List<DraggableItem2D> unlockedItems = new List<DraggableItem2D>();

    // 病人身上当前显示的那个“副本”
    private GameObject currentPlaced;

    // 当前装备在病人身上的物品（用于替换时让上一个回架子）
    private DraggableItem2D currentEquippedItem;

    // 最终确认选择的物品
    private DraggableItem2D finalSelectedItem;

    // 过关状态
    private bool canFinish = false;
    private bool resultShown = false;
    private Coroutine arrowCo;

    [Header("Scene Transition")]
    public SceneFade sceneFade;
    public string nextSceneName = "day2_clinic";



    void Start()
    {
        HideCanvasGroupImmediate(nextArrow);
        HideCanvasGroupImmediate(resultPanel);
    }

    /// <summary>
    /// 注册正确物品：首次解锁时加入侧边栏和已解锁列表
    /// </summary>
    public bool RegisterCorrectItem(DraggableItem2D item)
    {
        if (item == null) return false;

        int id = item.GetInstanceID();
        bool isNew = unlockedIds.Add(id);

        if (isNew)
        {
            // ✅ 记进已解锁物品列表（给最终选择面板使用）
            unlockedItems.Add(item);

            // ✅ 默认用物体自己的 sprite
            Sprite iconSprite = item.GetSprite();

            // ✅ 如果这个物体有 OrganizerSpecialItem，就用它指定的侧边栏 icon
            OrganizerSpecialItem special = item.GetComponent<OrganizerSpecialItem>();
            if (special != null)
            {
                iconSprite = special.GetSideBarIcon();
            }

            // ✅ 解锁侧边栏下一个槽位
            if (sideBarUI != null)
            {
                sideBarUI.RevealNextWithIcon(iconSprite, playAnim: true);
            }
        }

        // ✅ 解锁任意一个就可以出现下一步箭头
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

        // 替换：先让上一个装备回架子
        if (replaceOnPatient && currentEquippedItem != null && currentEquippedItem != item)
        {
            currentEquippedItem.ShowInWorldAtHome();
        }

        // 当前装备：从架子消失
        item.HideInWorld();
        currentEquippedItem = item;

        Transform attach = item.patientAttachPoint != null ? item.patientAttachPoint : defaultAttachPoint;
        if (attach == null) return;

        if (replaceOnPatient && currentPlaced != null)
        {
            Destroy(currentPlaced);
            currentPlaced = null;
        }

        GameObject placed = null;

        // ✅ 如果这是 Organizer 这种特殊物品，并且设置了展示 prefab
        OrganizerSpecialItem special = item.GetComponent<OrganizerSpecialItem>();
        if (special != null && special.GetPatientDisplayPrefab() != null)
        {
            placed = Instantiate(special.GetPatientDisplayPrefab(), attach.position, attach.rotation);
            placed.transform.localScale = item.transform.localScale;
        }
        else
        {
            // 默认逻辑：普通物品仍然只复制 root sprite
            placed = new GameObject(item.name + "_Placed");
            placed.transform.position = attach.position;
            placed.transform.rotation = attach.rotation;
            placed.transform.localScale = item.transform.localScale;

            var placedSR = placed.AddComponent<SpriteRenderer>();
            placedSR.sprite = item.GetSprite();
            placedSR.sortingLayerID = item.GetSortingLayerID();
            placedSR.sortingOrder = item.GetSortingOrder() + 1;
        }

        currentPlaced = placed;
    }

    /// <summary>
    /// 显示右下角下一步箭头
    /// </summary>
    private void ShowNextArrow()
    {
        if (nextArrow == null) return;
        if (canFinish) return;

        canFinish = true;

        if (arrowCo != null) StopCoroutine(arrowCo);
        arrowCo = StartCoroutine(FadeCanvasGroup(nextArrow, nextArrow.alpha, 1f, arrowFadeTime, true));
    }

    /// <summary>
    /// 点击右下角箭头：打开 ResultPanel
    /// </summary>
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

    /// <summary>
    /// ResultPanel：继续探索
    /// </summary>
    public void OnClickKeepExploring()
    {
        CloseResultPanel();
    }

    /// <summary>
    /// ResultPanel：进入下一天 → 打开物品选择面板
    /// </summary>
    public void OnClickNextDay()
    {
        if (itemSelectionPanel == null)
        {
            Debug.LogWarning("ItemSelectionPanelUI is not assigned on GameManager_JFM.");
            return;
        }

        if (unlockedItems.Count == 0)
        {
            Debug.LogWarning("No unlocked items available for selection.");
            return;
        }

        itemSelectionPanel.Open(unlockedItems, this);
    }

    /// <summary>
    /// 最终确认物品后调用
    /// </summary>
    public void SetFinalSelectedItem(DraggableItem2D item)
    {
        if (item == null) return;

        finalSelectedItem = item;

        // 1) Record selected item by day
        if (currentDay == 1)
        {
            GameProgress_JFM.day1SelectedItemName = item.name;
            GameProgress_JFM.currentNewsDay = 1;
            GameProgress_JFM.nextSceneAfterNews = "day2_clinic";
            Debug.Log("Day 1 selected item: " + item.name);
        }
        else if (currentDay == 2)
        {
            GameProgress_JFM.day2SelectedItemName = item.name;
            GameProgress_JFM.currentNewsDay = 2;
            GameProgress_JFM.nextSceneAfterNews = "day3_clinic";
            Debug.Log("Day 2 selected item: " + item.name);
        }
        else if (currentDay == 3)
        {
            GameProgress_JFM.day3SelectedItemName = item.name;
            GameProgress_JFM.currentNewsDay = 3;
            GameProgress_JFM.nextSceneAfterNews = "day4_clinic";
            Debug.Log("Day 3 selected item: " + item.name);
        }
        else if (currentDay == 4)
        {
            GameProgress_JFM.day4SelectedItemName = item.name;
            GameProgress_JFM.currentNewsDay = 4;
            GameProgress_JFM.nextSceneAfterNews = "day5_clinic";
            Debug.Log("Day 4 selected item: " + item.name);
        }

        // 2) Always go to the shared NewsScene first
        if (sceneFade != null)
        {
            Time.timeScale = 1f;
            sceneFade.FadeToScene("news_scenes");
        }
        else
        {
            Debug.LogWarning("SceneFade is missing on GameManager_JFM.");
        }
    }

    /// <summary>
    /// 关掉 ResultPanel：继续探索
    /// </summary>
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