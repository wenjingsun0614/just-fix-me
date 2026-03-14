using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager_JFM : MonoBehaviour
{
    [Header("Day Info")]
    public int currentDay = 1;

    [Header("UI")]
    public SideBarUI sideBarUI;

    [Header("Next Arrow UI")]
    public CanvasGroup nextArrow;
    public float arrowFadeTime = 0.25f;

    [Header("Result UI")]
    public CanvasGroup resultPanel;
    public float resultFadeTime = 0.25f;

    [Header("Item Selection UI")]
    public ItemSelectionPanelUI itemSelectionPanel;

    [Header("Patient Attachment")]
    public Transform defaultAttachPoint;
    public bool replaceOnPatient = true;

    private HashSet<int> unlockedIds = new HashSet<int>();
    private List<DraggableItem2D> unlockedItems = new List<DraggableItem2D>();

    private GameObject currentPlaced;
    private DraggableItem2D currentEquippedItem;
    private DraggableItem2D finalSelectedItem;

    private bool canFinish = false;
    private bool resultShown = false;
    private Coroutine arrowCo;

    [Header("Scene Transition")]
    public SceneFade sceneFade;
    public string nextSceneName = "day2_clinic";

    [Header("Special Override")]
    public bool ferrariOverrideActive = false;
    public string ferrariOverrideResultName = "FerrariHorse";

    void Start()
    {
        HideCanvasGroupImmediate(nextArrow);
        HideCanvasGroupImmediate(resultPanel);
    }

    // 原本普通拖拽物品用这个
    public bool RegisterCorrectItem(DraggableItem2D item)
    {
        if (item == null) return false;

        int id = item.GetInstanceID();
        bool isNew = unlockedIds.Add(id);

        if (isNew)
        {
            unlockedItems.Add(item);

            Sprite iconSprite = item.GetSprite();

            OrganizerSpecialItem special = item.GetComponent<OrganizerSpecialItem>();
            if (special != null)
            {
                iconSprite = special.GetSideBarIcon();
            }

            if (sideBarUI != null)
            {
                sideBarUI.RevealNextWithIcon(iconSprite, playAnim: true);
            }
        }

        if (unlockedIds.Count >= 1)
            ShowNextArrow();

        return isNew;
    }

    // 给 Day6 这种“不是拖拽成功，但想当作一个已解锁物品”用
    public bool RegisterSpecialItem(DraggableItem2D item, bool showNextArrow = true)
    {
        if (item == null) return false;

        int id = item.GetInstanceID();
        bool isNew = unlockedIds.Add(id);

        if (isNew)
        {
            unlockedItems.Add(item);

            Sprite iconSprite = item.GetSprite();

            OrganizerSpecialItem special = item.GetComponent<OrganizerSpecialItem>();
            if (special != null)
            {
                iconSprite = special.GetSideBarIcon();
            }

            if (sideBarUI != null)
            {
                sideBarUI.RevealNextWithIcon(iconSprite, playAnim: true);
            }
        }

        if (showNextArrow && unlockedIds.Count >= 1)
            ForceShowNextArrow();

        return isNew;
    }

    public void ShowOnPatient(DraggableItem2D item)
    {
        if (item == null) return;

        if (replaceOnPatient && currentEquippedItem != null && currentEquippedItem != item)
        {
            currentEquippedItem.ShowInWorldAtHome();
        }

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

        OrganizerSpecialItem special = item.GetComponent<OrganizerSpecialItem>();
        if (special != null && special.GetPatientDisplayPrefab() != null)
        {
            placed = Instantiate(special.GetPatientDisplayPrefab(), attach.position, attach.rotation);
            placed.transform.localScale = item.transform.localScale;
        }
        else
        {
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

    private void ShowNextArrow()
    {
        if (nextArrow == null) return;
        if (canFinish) return;

        canFinish = true;

        if (arrowCo != null) StopCoroutine(arrowCo);
        arrowCo = StartCoroutine(FadeCanvasGroup(nextArrow, nextArrow.alpha, 1f, arrowFadeTime, true));
    }

    public void ForceShowNextArrow()
    {
        if (nextArrow == null) return;

        canFinish = true;
        resultShown = false;

        if (arrowCo != null) StopCoroutine(arrowCo);
        arrowCo = StartCoroutine(FadeCanvasGroup(nextArrow, nextArrow.alpha, 1f, arrowFadeTime, true));
    }

    public void ActivateFerrariOverride(string resultName = "FerrariHorse")
    {
        ferrariOverrideActive = true;
        ferrariOverrideResultName = resultName;
        ForceShowNextArrow();
    }

    public void OnClickNextArrow()
    {
        if (!canFinish) return;
        if (resultShown) return;

        // Ferrari 彩蛋模式：不再开 ResultPanel，直接结算进新闻
        if (ferrariOverrideActive)
        {
            CompleteDayWithSpecialResult(ferrariOverrideResultName);
            return;
        }

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
        // Ferrari 彩蛋模式：不再开 ItemSelectionPanel，直接结算进新闻
        if (ferrariOverrideActive)
        {
            CompleteDayWithSpecialResult(ferrariOverrideResultName);
            return;
        }

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

    public void SetFinalSelectedItem(DraggableItem2D item)
    {
        if (item == null) return;

        finalSelectedItem = item;

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
        else if (currentDay == 5)
        {
            GameProgress_JFM.day5SelectedItemName = item.name;
            GameProgress_JFM.currentNewsDay = 5;
            GameProgress_JFM.nextSceneAfterNews = "day6_clinic";
            Debug.Log("Day 5 selected item: " + item.name);
        }
        else if (currentDay == 6)
        {
            GameProgress_JFM.day6SelectedItemName = item.name;
            GameProgress_JFM.currentNewsDay = 6;
            GameProgress_JFM.nextSceneAfterNews = "day7_clinic";
            Debug.Log("Day 6 selected item: " + item.name);
        }

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

    public void CompleteDayWithSpecialResult(string resultName)
    {
        if (string.IsNullOrEmpty(resultName))
        {
            Debug.LogWarning("Special result name is empty.");
            return;
        }

        if (currentDay == 1)
        {
            GameProgress_JFM.day1SelectedItemName = resultName;
            GameProgress_JFM.currentNewsDay = 1;
            GameProgress_JFM.nextSceneAfterNews = "day2_clinic";
        }
        else if (currentDay == 2)
        {
            GameProgress_JFM.day2SelectedItemName = resultName;
            GameProgress_JFM.currentNewsDay = 2;
            GameProgress_JFM.nextSceneAfterNews = "day3_clinic";
        }
        else if (currentDay == 3)
        {
            GameProgress_JFM.day3SelectedItemName = resultName;
            GameProgress_JFM.currentNewsDay = 3;
            GameProgress_JFM.nextSceneAfterNews = "day4_clinic";
        }
        else if (currentDay == 4)
        {
            GameProgress_JFM.day4SelectedItemName = resultName;
            GameProgress_JFM.currentNewsDay = 4;
            GameProgress_JFM.nextSceneAfterNews = "day5_clinic";
        }
        else if (currentDay == 5)
        {
            GameProgress_JFM.day5SelectedItemName = resultName;
            GameProgress_JFM.currentNewsDay = 5;
            GameProgress_JFM.nextSceneAfterNews = "day6_clinic";
        }
        else if (currentDay == 6)
        {
            GameProgress_JFM.day6SelectedItemName = resultName;
            GameProgress_JFM.currentNewsDay = 6;
            GameProgress_JFM.nextSceneAfterNews = "day7_clinic";
        }

        Debug.Log("Special result recorded: " + resultName);

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

    public void CloseResultPanel()
    {
        if (resultPanel != null)
            StartCoroutine(FadeCanvasGroup(resultPanel, resultPanel.alpha, 0f, 0.15f, false));

        resultShown = false;

        if (nextArrow != null && canFinish)
            StartCoroutine(FadeCanvasGroup(nextArrow, nextArrow.alpha, 1f, 0.15f, true));
    }

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