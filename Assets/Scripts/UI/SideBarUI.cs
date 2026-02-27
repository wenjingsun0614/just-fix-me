using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SideBarUI : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public Image bgOff;      // 灰底（未完成）
        public Image bgOn;       // 蓝底（完成点亮）
        public Image itemIcon;   // 物品图标
        public Image checkIcon;  // 绿色勾

        [HideInInspector] public Vector3 bgOnBaseScale;
        [HideInInspector] public Vector3 itemBaseScale;
        [HideInInspector] public Vector2 checkBasePos;
    }

    public Slot[] slots;

    [HideInInspector] public int nextIndex = 0; // 下一个要解锁的槽位

    public int RevealNext(bool playAnim = true)
    {
        if (nextIndex < 0) nextIndex = 0;
        if (nextIndex >= slots.Length) return -1;

        int idx = nextIndex;
        SetFound(idx, true, playAnim);
        nextIndex++;
        return idx;
    }

    [Header("Pop Animation")]
    public float popScale = 1.08f;      // 放大幅度（1.05~1.12都行）
    public float popUpTime = 0.08f;     // 放大时间
    public float popDownTime = 0.10f;   // 回弹时间

    [Header("Check Float In")]
    public float checkFloatY = 10f;     // 上浮像素（UI里用像素单位）
    public float checkFadeTime = 0.18f; // 渐入时长

    void Awake()
    {
        // 记录初始状态，避免每次动画后“越变越大”
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s.bgOn != null) s.bgOnBaseScale = s.bgOn.rectTransform.localScale;
            if (s.itemIcon != null) s.itemBaseScale = s.itemIcon.rectTransform.localScale;
            if (s.checkIcon != null) s.checkBasePos = s.checkIcon.rectTransform.anchoredPosition;
        }
    }

    void Start()
    {
        for (int i = 0; i < slots.Length; i++)
            SetFound(i, false, playAnim: false);
        nextIndex = 0;
    }

    // 对外调用：找到/未找到
    public void SetFound(int index, bool found, bool playAnim = true)
    {

        if (index < 0 || index >= slots.Length) return;
        var s = slots[index];

        // 背景切换
        if (s.bgOff != null) s.bgOff.gameObject.SetActive(!found);
        if (s.bgOn != null) s.bgOn.gameObject.SetActive(found);

        // 物品图标
        if (s.itemIcon != null)
        {
            s.itemIcon.gameObject.SetActive(found);
            var c = s.itemIcon.color; c.a = 1f; s.itemIcon.color = c;
        }

        // 勾号
        if (s.checkIcon != null)
        {
            s.checkIcon.gameObject.SetActive(found);
            // 先设为透明（准备渐入）
            var cc = s.checkIcon.color; cc.a = found ? 0f : 0f; s.checkIcon.color = cc;

            // 把勾号位置重置到“稍微低一点”的起点
            var rt = s.checkIcon.rectTransform;
            rt.anchoredPosition = s.checkBasePos + Vector2.down * checkFloatY;
        }

        if (found && playAnim)
        {
            StopAllCoroutines(); // 如果你担心多个槽同时触发会互相打断，可改成每槽单独协程（后面我也能给你）
            StartCoroutine(PopAndCheckIn(s));
        }
    }

    public int RevealNextWithIcon(Sprite icon, bool playAnim = true)
    {
        if (nextIndex < 0) nextIndex = 0;
        if (nextIndex >= slots.Length) return -1;

        int idx = nextIndex;

        // 把 icon 塞到这个槽位的 ItemIcon
        var s = slots[idx];
        if (s.itemIcon != null && icon != null)
        {
            s.itemIcon.sprite = icon;
            s.itemIcon.preserveAspect = true;
        }

        // 点亮这个槽位（你原来的动画也会跑）
        SetFound(idx, true, playAnim);

        nextIndex++;
        return idx;
    }

    IEnumerator PopAndCheckIn(Slot s)
    {
        // --- POP：蓝底+物品一起 ---
        if (s.bgOn != null) s.bgOn.rectTransform.localScale = s.bgOnBaseScale;
        if (s.itemIcon != null) s.itemIcon.rectTransform.localScale = s.itemBaseScale;

        Vector3 bgTarget = s.bgOnBaseScale * popScale;
        Vector3 itemTarget = s.itemBaseScale * popScale;

        // 放大
        yield return ScaleTwo(s, bgTarget, itemTarget, popUpTime);
        // 回弹
        yield return ScaleTwo(s, s.bgOnBaseScale, s.itemBaseScale, popDownTime);

        // --- 勾号：上浮 + 渐入 ---
        if (s.checkIcon != null)
        {
            RectTransform rt = s.checkIcon.rectTransform;
            Vector2 fromPos = rt.anchoredPosition;
            Vector2 toPos = s.checkBasePos;

            Color fromC = s.checkIcon.color;
            Color toC = s.checkIcon.color;
            fromC.a = 0f;
            toC.a = 1f;
            s.checkIcon.color = fromC;

            float t = 0f;
            while (t < checkFadeTime)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / checkFadeTime);
                // ease out
                float e = 1f - Mathf.Pow(1f - p, 3f);

                rt.anchoredPosition = Vector2.Lerp(fromPos, toPos, e);
                s.checkIcon.color = Color.Lerp(fromC, toC, e);
                yield return null;
            }

            rt.anchoredPosition = toPos;
            s.checkIcon.color = toC;
        }
    }

    IEnumerator ScaleTwo(Slot s, Vector3 bgScale, Vector3 itemScale, float duration)
    {
        if (duration <= 0f)
        {
            if (s.bgOn != null) s.bgOn.rectTransform.localScale = bgScale;
            if (s.itemIcon != null) s.itemIcon.rectTransform.localScale = itemScale;
            yield break;
        }

        Vector3 bgFrom = s.bgOn != null ? s.bgOn.rectTransform.localScale : Vector3.one;
        Vector3 itemFrom = s.itemIcon != null ? s.itemIcon.rectTransform.localScale : Vector3.one;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float e = 1f - Mathf.Pow(1f - p, 3f);

            if (s.bgOn != null) s.bgOn.rectTransform.localScale = Vector3.Lerp(bgFrom, bgScale, e);
            if (s.itemIcon != null) s.itemIcon.rectTransform.localScale = Vector3.Lerp(itemFrom, itemScale, e);

            yield return null;
        }

        if (s.bgOn != null) s.bgOn.rectTransform.localScale = bgScale;
        if (s.itemIcon != null) s.itemIcon.rectTransform.localScale = itemScale;
    }
}