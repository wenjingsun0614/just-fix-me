using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public SceneFade sceneFade;

    public void StartGame()
    {
        sceneFade.FadeToScene("day1_clinic");
    }

    public void StartDAY1()
    {
        sceneFade.FadeToScene("day1_clinic");
    }

    public class ButtonController : MonoBehaviour
    {
        public GameObject DAY2Button;

        void Update()
        {
            if (!string.IsNullOrEmpty(GameProgress_JFM.day1SelectedItemName))
            {
                DAY2Button.SetActive(true);   // ÏÔÊ¾°´Å¥
            }
            else
            {
                DAY2Button.SetActive(false);  // Òþ²Ø°´Å¥
            }
        }
    }

    public void StartDAY2()
    {
        sceneFade.FadeToScene("day2_clinic");
    }

    public void StartDAY3()
    {
        sceneFade.FadeToScene("day3_clinic");
    }

    public void StartDAY4()
    {
        sceneFade.FadeToScene("day4_clinic");
    }

    public void StartDAY5()
    {
        sceneFade.FadeToScene("day5_clinic");
    }

    public void StartDAY6()
    {
        sceneFade.FadeToScene("day6_clinic");
    }

    public void StartDAY7()
    {
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