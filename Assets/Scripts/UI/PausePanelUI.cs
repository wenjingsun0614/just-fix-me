using UnityEngine;
using UnityEngine.UI;

public class PausePanelUI : MonoBehaviour
{
    [Header("Panel")]
    public CanvasGroup panelGroup;

    [Header("Fade")]
    public SceneFade sceneFade;

    [Header("Scene Names")]
    public string currentLevelSceneName = "day1_clinic";
    public string mainMenuSceneName = "main_menu";

    [Header("Volume")]
    public Slider volumeSlider;

    [Header("Brightness")]
    public Slider brightnessSlider;
    public Image brightnessOverlay;   // 拖 BrightnessOverlay 进来
    public float minOverlayAlpha = 0f;
    public float maxOverlayAlpha = 0.45f;

    private bool isOpen = false;

    void Start()
    {
        // 音量
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.value = savedVolume;
            AudioListener.volume = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // 亮度
        if (brightnessSlider != null)
        {
            float savedBrightness = PlayerPrefs.GetFloat("Brightness", 1f);
            brightnessSlider.value = savedBrightness;
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
            ApplyBrightness(savedBrightness);
        }

        RefreshPanelState();
    }

    public void OpenPanel()
    {
        isOpen = true;
        RefreshPanelState();
    }

    public void ClosePanel()
    {
        isOpen = false;
        RefreshPanelState();
    }

    public void TogglePanel()
    {
        isOpen = !isOpen;
        RefreshPanelState();
    }

    public void ReturnGame()
    {
        ClosePanel();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        // ✅ 告诉下一次进入场景时，跳过“黑->透明”的开场淡入
        SceneFadeIn.skipNextFadeIn = true;

        if (sceneFade != null)
            sceneFade.FadeToScene(currentLevelSceneName);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;

        // 回主菜单一般不需要跳过主菜单自己的显示逻辑
        if (sceneFade != null)
            sceneFade.FadeToScene(mainMenuSceneName);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    public void SetBrightness(float value)
    {
        ApplyBrightness(value);
        PlayerPrefs.SetFloat("Brightness", value);
        PlayerPrefs.Save();
    }

    void ApplyBrightness(float value)
    {
        if (brightnessOverlay == null) return;

        // value = 1 最亮，value = 0 最暗
        float alpha = Mathf.Lerp(maxOverlayAlpha, minOverlayAlpha, value);

        Color c = brightnessOverlay.color;
        c.a = alpha;
        brightnessOverlay.color = c;
    }

    void RefreshPanelState()
    {
        if (panelGroup == null) return;

        panelGroup.alpha = isOpen ? 1f : 0f;
        panelGroup.interactable = isOpen;
        panelGroup.blocksRaycasts = isOpen;

        Time.timeScale = isOpen ? 0f : 1f;
    }
}