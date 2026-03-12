using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DarknessHoleController : MonoBehaviour
{
    [Header("References")]
    public Image darknessImage;
    public RectTransform darknessRect;

    [Header("Mouse Light")]
    [Range(0.01f, 1f)] public float radius = 0.18f;
    [Range(0.001f, 1f)] public float softness = 0.12f;

    [Header("Lightning")]
    [Range(0f, 1f)] public float normalAlpha = 0.78f;
    [Range(0f, 1f)] public float lightningAlpha = 0.45f;
    public float lightningFadeTime = 0.20f;

    private Material runtimeMaterial;
    private Coroutine lightningRoutine;

    void Start()
    {
        if (darknessImage == null)
            darknessImage = GetComponent<Image>();

        if (darknessRect == null)
            darknessRect = darknessImage.rectTransform;

        // make runtime instance so you don't edit shared material asset directly
        runtimeMaterial = new Material(darknessImage.material);
        darknessImage.material = runtimeMaterial;

        SetDarknessAlpha(normalAlpha);
        runtimeMaterial.SetFloat("_Radius", radius);
        runtimeMaterial.SetFloat("_Softness", softness);
    }

    void Update()
    {
        if (runtimeMaterial == null || darknessRect == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            darknessRect,
            Input.mousePosition,
            null,
            out localPoint
        );

        Rect rect = darknessRect.rect;

        float u = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float v = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

        runtimeMaterial.SetVector("_HoleCenter", new Vector4(u, v, 0f, 0f));
        runtimeMaterial.SetFloat("_Radius", radius);
        runtimeMaterial.SetFloat("_Softness", softness);
    }

    public void TriggerLightning()
    {
        if (lightningRoutine != null)
            StopCoroutine(lightningRoutine);

        lightningRoutine = StartCoroutine(LightningRoutine());
    }

    IEnumerator LightningRoutine()
    {
        SetDarknessAlpha(lightningAlpha);

        float t = 0f;
        while (t < lightningFadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(lightningAlpha, normalAlpha, t / lightningFadeTime);
            SetDarknessAlpha(a);
            yield return null;
        }

        SetDarknessAlpha(normalAlpha);
        lightningRoutine = null;
    }

    void SetDarknessAlpha(float alpha)
    {
        if (runtimeMaterial == null) return;

        Color c = runtimeMaterial.GetColor("_Color");
        c.a = alpha;
        runtimeMaterial.SetColor("_Color", c);
    }
}