using UnityEngine;
using System.Collections;

public class ShowAfterDelay : MonoBehaviour
{
    public GameObject[] targetObjects;
    public float delay = 5.2f;

    void Start()
    {
        SetObjectsActiveState(false);
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        yield return new WaitForSeconds(delay);
        SetObjectsActiveState(true);
    }

    void SetObjectsActiveState(bool visible)
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj == null) continue;

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = visible;

            Collider2D col = obj.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = visible;
        }
    }
}