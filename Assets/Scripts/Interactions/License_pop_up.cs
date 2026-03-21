using UnityEngine;
using UnityEngine.EventSystems;

public class License_pop_up : MonoBehaviour
{
    [Header("Panel To Open")]
    public GameObject panelToOpen;

    [Header("Intro Controller")]
    public DayIntroController introController;
    void OnMouseDown()
    {
        // 统一交互锁
        if (!GameFlow_JFM.CanDrag)
            return;

        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);
        }
    }

    // 关闭按钮调用
    public void ClosePanel()
    {
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(false);
        }
    }
}