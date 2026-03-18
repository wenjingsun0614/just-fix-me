using UnityEngine;

public class License_pop_up : MonoBehaviour
{
    [Header("Panel To Open")]
    public GameObject panelToOpen;

    [Header("Intro Controller (拖入 DayIntroController)")]
    public DayIntroController introController;

    void OnMouseDown()
    {
        // 没有绑定，不拦截
        if (introController != null)
        {
            // 对话没结束禁止触发
            if (!introController.IsIntroFinished())
                return;
        }

        // 正常打开
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);
        }
    }
}