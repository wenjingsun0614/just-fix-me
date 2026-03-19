using UnityEngine;
using UnityEngine.UI;

public class Day6BrightnessSecret : MonoBehaviour
{
    [Header("Brightness")]
    public Slider brightnessSlider;

    [Tooltip("Slider range is 0~100, trigger when brightness is <= this value.")]
    public float triggerThreshold = 10f;

    [Tooltip("Default brightness when entering Day6. Slider range is 0~100.")]
    public float defaultBrightness = 50f;

    [Header("Refs")]
    public GameManager_JFM gameManager;
    public DraggableItem2D hiddenRewardItem;
    public PatientVisualStateController patientStateController;

    [Header("State")]
    public bool hasTriggered = false;

    [Tooltip("Must match the itemName in PatientVisualStateController.")]
    public string brightnessStateItemName = "LowBrightness";

    void Start()
    {
        InitializeBrightness();
    }

    void InitializeBrightness()
    {
        if (brightnessSlider == null) return;

        // Important:
        // Set the slider value without notifying listeners,
        // so existing onValueChanged triggers will NOT fire during initialization.
        brightnessSlider.SetValueWithoutNotify(defaultBrightness);

        Debug.Log("Day6 brightness initialized to: " + defaultBrightness);
    }

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

            // 1. Unlock hidden item
            gameManager.RegisterSpecialItem(hiddenRewardItem, true);

            // 2. Trigger patient state switch
            if (patientStateController != null)
            {
                patientStateController.ApplyStateByItemName(brightnessStateItemName);
            }
        }
    }
}