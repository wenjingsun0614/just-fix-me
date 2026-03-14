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

    [Header("State")]
    public bool hasTriggered = false;

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

            gameManager.RegisterSpecialItem(hiddenRewardItem, true);
        }
    }
}