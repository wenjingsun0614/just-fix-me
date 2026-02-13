using System.Collections;
using UnityEngine;

public class UIButtonPop : MonoBehaviour
{
    public Transform target;         // ÍÏ Icon ½øÀ´
    public float popScale = 1.12f;
    public float popTime = 0.08f;
    public float returnTime = 0.10f;

    Vector3 baseScale;
    Coroutine co;

    void Awake()
    {
        if (target == null) target = transform;
        baseScale = target.localScale;
    }

    public void Pop()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(PopRoutine());
    }

    IEnumerator PopRoutine()
    {
        float t = 0f;
        Vector3 s1 = baseScale * popScale;

        while (t < popTime)
        {
            t += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(baseScale, s1, t / popTime);
            yield return null;
        }
        target.localScale = s1;

        t = 0f;
        while (t < returnTime)
        {
            t += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(s1, baseScale, t / returnTime);
            yield return null;
        }
        target.localScale = baseScale;
    }
}
