using UnityEngine;

public class OrganizerSpecialItem : MonoBehaviour
{
    [Header("UI")]
    public Sprite sideBarIconSprite;

    [Header("Patient Display")]
    public GameObject patientDisplayPrefab;

    private SpriteRenderer rootRenderer;
    private SpriteRenderer[] allRenderers;
    private Collider2D[] allColliders;

    void Awake()
    {
        rootRenderer = GetComponent<SpriteRenderer>();
        allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        allColliders = GetComponentsInChildren<Collider2D>(true);
    }

    void LateUpdate()
    {
        if (rootRenderer == null) return;

        bool rootEnabled = rootRenderer.enabled;
        float rootAlpha = rootRenderer.color.a;

        // 让子物体的球跟随 root 的显示/隐藏/透明度
        foreach (var r in allRenderers)
        {
            if (r == null || r == rootRenderer) continue;

            r.enabled = rootEnabled;

            Color c = r.color;
            c.a = rootAlpha;
            r.color = c;
        }

        // 让子物体碰撞也跟随 root 开关
        foreach (var c in allColliders)
        {
            if (c == null || c.gameObject == gameObject) continue;
            c.enabled = rootEnabled;
        }
    }

    public Sprite GetSideBarIcon()
    {
        if (sideBarIconSprite != null) return sideBarIconSprite;

        if (rootRenderer != null) return rootRenderer.sprite;

        return null;
    }

    public GameObject GetPatientDisplayPrefab()
    {
        return patientDisplayPrefab;
    }
}