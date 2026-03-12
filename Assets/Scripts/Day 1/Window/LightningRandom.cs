using System.Collections;
using UnityEngine;

public class LightningRandom : MonoBehaviour
{
    public float minDelay = 5f;
    public float maxDelay = 12f;

    public DarknessHoleController darknessController;

    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(FlashLoop());
    }

    IEnumerator FlashLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            TriggerLightning();

            // Å¼¶ûË«ÉÁ
            if (Random.value < 0.35f)
            {
                yield return new WaitForSeconds(0.2f);
                TriggerLightning();
            }
        }
    }

    void TriggerLightning()
    {
        anim.SetTrigger("Flash");

        if (darknessController != null)
            darknessController.TriggerLightning();
    }
}