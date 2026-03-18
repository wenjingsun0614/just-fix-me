using UnityEngine;
using System.Collections;

public class SideBarExtraDisplay : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public string key;           // 对应 item.name
        public GameObject bigImage;  // 要显示的大图
    }

    public Entry[] entries;

    [Header("Patient")]
    public GameObject patient1;

    public float fadeTime = 0.3f;

    public void OnItemUnlocked(string key)
    {
        // 隐藏 Patient
        if (patient1 != null)
        {
            patient1.SetActive(false);
        }

        // 显示对应图片
        foreach (var e in entries)
        {
            if (e.key == key && e.bigImage != null)
            {
                StartCoroutine(FadeIn(e.bigImage));
            }
        }
    }

    IEnumerator FadeIn(GameObject obj)
    {
        obj.SetActive(true);

        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color c = sr.color;
        c.a = 0;
        sr.color = c;

        float t = 0;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            c.a = t / fadeTime;
            sr.color = c;
            yield return null;
        }

        c.a = 1;
        sr.color = c;
    }
}