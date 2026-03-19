using UnityEngine;
using UnityEngine.UI;

public class Day6BrightnessSecret : MonoBehaviour
{
    [Header("Brightness")]
    public Slider brightnessSlider;

    [Tooltip("如果你的 slider 范围是 0~100，这里填 10；如果是 0~1，这里填 0.1")]
    public float triggerThreshold = 10f;

    [Header("Refs")]
    public GameManager_JFM gameManager;
    public DraggableItem2D hiddenRewardItem;
    public PatientVisualStateController patientStateController;

    [Header("State")]
    public bool hasTriggered = false;

    [Tooltip("要切换到的状态名，必须和 PatientVisualStateController 里的 itemName 一致")]
    public string brightnessStateItemName = "LowBrightness";

    public void CheckBrightnessSecret()
    {
        if (hasTriggered) return;
        if (brightnessSlider == null || gameManager == null || hiddenRewardItem == null) return;

        float currentBrightness = brightnessSlider.value;
        Debug.Log("Current brightness = " + currentBrightness);

        if (currentBrightness <= triggerThreshold)
        {
            hasTriggered = true;
            Debug.Log("Day6 brightness secret triggered!");

            // 1. 解锁隐藏物品
            gameManager.RegisterSpecialItem(hiddenRewardItem, true);

            // 2. 触发病人状态切换
            if (patientStateController != null)
            {
                patientStateController.ApplyStateByItemName(brightnessStateItemName);
            }
        }
    }
}