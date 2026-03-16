using UnityEngine;

public class ShowFixButtonAfterIntro : MonoBehaviour
{
    public GameObject fixButtonObject;

    void Start()
    {
        SetFixButtonVisible(false);
    }

    public void ShowFixButton()
    {
        SetFixButtonVisible(true);
    }

    void SetFixButtonVisible(bool visible)
    {
        if (fixButtonObject == null) return;

        SpriteRenderer sr = fixButtonObject.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = visible;

        Collider2D col = fixButtonObject.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = visible;
    }
}