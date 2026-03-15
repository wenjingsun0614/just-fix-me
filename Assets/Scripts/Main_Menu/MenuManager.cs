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

    public void StartDAY2()
    {
        sceneFade.FadeToScene("day2_clinic");
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