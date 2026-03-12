using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RoomLightningFlash : MonoBehaviour
{
    public Image flashImage;
    public float flashAlpha = 0.28f;
    public float fadeOutTime = 0.2f;

    Coroutine flashRoutine;

    void Awake()
    {
        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }
    }

    public void TriggerFlash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        if (flashImage == null) yield break;

        Color c = flashImage.color;
        c.a = flashAlpha;
        flashImage.color = c;

        float t = 0f;

        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(flashAlpha, 0f, t / fadeOutTime);
            c.a = a;
            flashImage.color = c;
            yield return null;
        }

        c.a = 0f;
        flashImage.color = c;
        flashRoutine = null;
    }
}