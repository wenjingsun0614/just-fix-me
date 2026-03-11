using UnityEngine;

public class CloudBackgroundSpawner : MonoBehaviour
{
    public BoxCollider2D area;
    public GameObject cloudPrefab;

    [Header("Spawn Count")]
    public int cloudCount = 3;

    [Header("Scale Range")]
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f);

    [Header("Horizontal Drift Range")]
    public Vector2 horizontalAmplitudeRange = new Vector2(0.15f, 0.45f);
    public Vector2 horizontalSpeedRange = new Vector2(0.25f, 0.7f);

    [Header("Vertical Float Range")]
    public Vector2 floatAmplitudeRange = new Vector2(0.015f, 0.05f);
    public Vector2 floatSpeedRange = new Vector2(0.6f, 1.2f);

    [Header("Breathing Scale")]
    public Vector2 scalePulseAmountRange = new Vector2(0.01f, 0.04f);
    public Vector2 scalePulseSpeedRange = new Vector2(0.5f, 1.2f);

    [Header("Padding Inside Area")]
    public float xPadding = 0.15f;
    public float yPadding = 0.2f;

    void Start()
    {
        if (area == null || cloudPrefab == null) return;

        Bounds bounds = area.bounds;

        for (int i = 0; i < cloudCount; i++)
        {
            float spawnX = Random.Range(bounds.min.x + xPadding, bounds.max.x - xPadding);
            float spawnY = Random.Range(bounds.min.y + yPadding, bounds.max.y - yPadding);

            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

            GameObject cloud = Instantiate(cloudPrefab, spawnPos, Quaternion.identity, transform);

            float scale = Random.Range(scaleRange.x, scaleRange.y);
            cloud.transform.localScale = Vector3.one * scale;

            CloudBackgroundFloat floatScript = cloud.GetComponent<CloudBackgroundFloat>();
            if (floatScript != null)
            {
                floatScript.area = area;

                // ×óÓÒÇá°Ú
                floatScript.horizontalAmplitude = Random.Range(horizontalAmplitudeRange.x, horizontalAmplitudeRange.y);
                floatScript.horizontalSpeed = Random.Range(horizontalSpeedRange.x, horizontalSpeedRange.y);

                // ÉÏÏÂÇá¸¡
                floatScript.floatAmplitude = Random.Range(floatAmplitudeRange.x, floatAmplitudeRange.y);
                floatScript.floatSpeed = Random.Range(floatSpeedRange.x, floatSpeedRange.y);

                // ÇáÎ¢ºôÎüËõ·Å
                floatScript.scalePulseAmount = Random.Range(scalePulseAmountRange.x, scalePulseAmountRange.y);
                floatScript.scalePulseSpeed = Random.Range(scalePulseSpeedRange.x, scalePulseSpeedRange.y);
            }
        }
    }
}