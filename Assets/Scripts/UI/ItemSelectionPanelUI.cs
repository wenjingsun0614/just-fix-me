using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSelectionPanelUI : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public Image itemDisplayImage;
    public TMP_Text countText;
    public TMP_Text itemNameText; // 可选，不填也行
    public Button leftButton;
    public Button rightButton;
    public Button confirmButton;

    private List<DraggableItem2D> items = new List<DraggableItem2D>();
    private int currentIndex = 0;
    private GameManager_JFM gameManager;

    void Start()
    {
        HideImmediate();

        if (leftButton != null) leftButton.onClick.AddListener(ShowPrevious);
        if (rightButton != null) rightButton.onClick.AddListener(ShowNext);
        if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmSelection);
    }

    public void Open(List<DraggableItem2D> unlockedItems, GameManager_JFM gm)
    {
        if (unlockedItems == null || unlockedItems.Count == 0) return;

        items = new List<DraggableItem2D>(unlockedItems);
        gameManager = gm;
        currentIndex = 0;

        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        RefreshUI();
    }

    public void Close()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        gameObject.SetActive(false);
    }

    void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        gameObject.SetActive(false);
    }

    void ShowPrevious()
    {
        if (items.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0) currentIndex = items.Count - 1;

        RefreshUI();
    }

    void ShowNext()
    {
        if (items.Count == 0) return;

        currentIndex++;
        if (currentIndex >= items.Count) currentIndex = 0;

        RefreshUI();
    }

    void RefreshUI()
    {
        if (items.Count == 0) return;

        DraggableItem2D currentItem = items[currentIndex];

        if (itemDisplayImage != null)
            itemDisplayImage.sprite = currentItem.GetSprite();

        if (countText != null)
            countText.text = $"{currentIndex + 1} / {items.Count}";

        if (itemNameText != null)
            itemNameText.text = currentItem.name;

        bool canCycle = items.Count > 1;

        if (leftButton != null) leftButton.gameObject.SetActive(canCycle);
        if (rightButton != null) rightButton.gameObject.SetActive(canCycle);
    }

    void ConfirmSelection()
    {
        if (items.Count == 0 || gameManager == null) return;

        gameManager.SetFinalSelectedItem(items[currentIndex]);
        Close();
    }
}