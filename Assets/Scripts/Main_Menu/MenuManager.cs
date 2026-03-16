using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public SceneFade sceneFade;

    [Header("Level Select Buttons")]
    public Button day1Button;
    public Button day2Button;
    public Button day3Button;
    public Button day4Button;
    public Button day5Button;
    public Button day6Button;
    public Button day7Button;

    void Start()
    {
        RefreshLevelButtons();
    }

    void Update()
    {
        // 如果你希望主菜单打开期间实时刷新解锁状态，就保留
        // 如果不需要每帧刷新，也可以删掉，只在 Start 里刷新
        RefreshLevelButtons();
    }

    public void RefreshLevelButtons()
    {
        if (day1Button != null)
            day1Button.interactable = true;

        if (day2Button != null)
            day2Button.interactable = !string.IsNullOrEmpty(GameProgress_JFM.day1SelectedItemName);

        if (day3Button != null)
            day3Button.interactable = !string.IsNullOrEmpty(GameProgress_JFM.day2SelectedItemName);

        if (day4Button != null)
            day4Button.interactable = !string.IsNullOrEmpty(GameProgress_JFM.day3SelectedItemName);

        if (day5Button != null)
            day5Button.interactable = !string.IsNullOrEmpty(GameProgress_JFM.day4SelectedItemName);

        if (day6Button != null)
            day6Button.interactable = !string.IsNullOrEmpty(GameProgress_JFM.day5SelectedItemName);

        if (day7Button != null)
            day7Button.interactable = !string.IsNullOrEmpty(GameProgress_JFM.day6SelectedItemName);
    }

    public void StartGame()
    {
        sceneFade.FadeToScene("day1_clinic");
    }

    public void StartDAY1()
    {
        sceneFade.FadeToScene("day1_clinic");
    }

    public void StartDAY2()
    {
        if (!string.IsNullOrEmpty(GameProgress_JFM.day1SelectedItemName))
            sceneFade.FadeToScene("day2_clinic");
    }

    public void StartDAY3()
    {
        if (!string.IsNullOrEmpty(GameProgress_JFM.day2SelectedItemName))
            sceneFade.FadeToScene("day3_clinic");
    }

    public void StartDAY4()
    {
        if (!string.IsNullOrEmpty(GameProgress_JFM.day3SelectedItemName))
            sceneFade.FadeToScene("day4_clinic");
    }

    public void StartDAY5()
    {
        if (!string.IsNullOrEmpty(GameProgress_JFM.day4SelectedItemName))
            sceneFade.FadeToScene("day5_clinic");
    }

    public void StartDAY6()
    {
        if (!string.IsNullOrEmpty(GameProgress_JFM.day5SelectedItemName))
            sceneFade.FadeToScene("day6_clinic");
    }

    public void StartDAY7()
    {
        if (!string.IsNullOrEmpty(GameProgress_JFM.day6SelectedItemName))
            sceneFade.FadeToScene("day7_clinic");
    }

    public void ContinueGame()
    {
        Debug.Log("Continue clicked (not implemented yet)");
    }

    public void OpenOptions()
    {
        Debug.Log("Options clicked (not implemented yet)");
    }

    public void QuitGame()
    {
        Debug.Log("Quit clicked");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}