using UnityEngine;

public class OpenPanelOnClick : MonoBehaviour
{
    public GameObject panelToOpen;

    void OnMouseDown()
    {
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);
        }
    }
}