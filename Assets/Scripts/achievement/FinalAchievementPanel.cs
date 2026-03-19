using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinalAchievementPanel : MonoBehaviour
{
    [Header("Selected Item Slots")]
    public Image day1Image;
    public Image day2Image;
    public Image day3Image;
    public Image day4Image;
    public Image day5Image;
    public Image day6Image;
    public Image day7Image;

    [Header("Next Arrow Controller")]
    public FinalAchievementButtons nextButtons;

    [Header("Optional Placeholder")]
    public Sprite emptyPlaceholderSprite;

    [Header("Achievement Text")]
    public TMP_Text achievementText;

    [Header("Animation")]
    public float imageFadeDuration = 0.35f;
    public float delayBetweenImages = 0.12f;
    public float delayBeforeTraitText = 0.4f;
    public float typewriterSpeed = 0.03f;

    private readonly List<string> unlocked = new List<string>();

    void Start()
    {
        SetupImage(day1Image, GameProgress_JFM.day1SelectedSprite);
        SetupImage(day2Image, GameProgress_JFM.day2SelectedSprite);
        SetupImage(day3Image, GameProgress_JFM.day3SelectedSprite);
        SetupImage(day4Image, GameProgress_JFM.day4SelectedSprite);
        SetupImage(day5Image, GameProgress_JFM.day5SelectedSprite);
        SetupImage(day6Image, GameProgress_JFM.day6SelectedSprite);
        SetupImage(day7Image, GameProgress_JFM.day7SelectedSprite);

        PrepareImagesForFade();
        BuildAchievements();

        if (achievementText != null)
        {
            achievementText.text = "";
        }

        StartCoroutine(PlaySequence());
    }

    void SetupImage(Image targetImage, Sprite recordedSprite)
    {
        if (targetImage == null) return;

        if (recordedSprite != null)
        {
            targetImage.sprite = recordedSprite;
            targetImage.enabled = true;
            targetImage.preserveAspect = true;
        }
        else if (emptyPlaceholderSprite != null)
        {
            targetImage.sprite = emptyPlaceholderSprite;
            targetImage.enabled = true;
            targetImage.preserveAspect = true;
        }
        else
        {
            targetImage.enabled = false;
        }
    }

    void PrepareImagesForFade()
    {
        SetImageAlpha(day1Image, 0f);
        SetImageAlpha(day2Image, 0f);
        SetImageAlpha(day3Image, 0f);
        SetImageAlpha(day4Image, 0f);
        SetImageAlpha(day5Image, 0f);
        SetImageAlpha(day6Image, 0f);
        SetImageAlpha(day7Image, 0f);
    }

    IEnumerator PlaySequence()
    {
        yield return FadeImage(day1Image);
        yield return new WaitForSeconds(delayBetweenImages);

        yield return FadeImage(day2Image);
        yield return new WaitForSeconds(delayBetweenImages);

        yield return FadeImage(day3Image);
        yield return new WaitForSeconds(delayBetweenImages);

        yield return FadeImage(day4Image);
        yield return new WaitForSeconds(delayBetweenImages);

        yield return FadeImage(day5Image);
        yield return new WaitForSeconds(delayBetweenImages);

        yield return FadeImage(day6Image);
        yield return new WaitForSeconds(delayBetweenImages);

        yield return FadeImage(day7Image);
        yield return new WaitForSeconds(delayBeforeTraitText);

        yield return ShowTraitAndAchievements();
    }

    IEnumerator FadeImage(Image img)
    {
        if (img == null || !img.enabled) yield break;

        float timer = 0f;
        Color c = img.color;
        c.a = 0f;
        img.color = c;

        while (timer < imageFadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / imageFadeDuration);

            c.a = Mathf.Lerp(0f, 1f, t);
            img.color = c;

            yield return null;
        }

        c.a = 1f;
        img.color = c;
    }

    IEnumerator ShowTraitAndAchievements()
    {
        if (achievementText == null) yield break;

        string prefix = "your hidden trait:\n";
        string finalText;

        if (unlocked.Count == 0)
        {
            finalText = prefix + "Perfectly Average\nyou perfectly missed every trait.";
        }
        else
        {
            finalText = prefix + string.Join("\n\n", unlocked);
        }
  

        achievementText.text = "";
        yield return TypeText(finalText);
    }

    IEnumerator TypeText(string fullText)
    {
        if (achievementText == null) yield break;

        achievementText.text = "";

        foreach (char c in fullText)
        {
            achievementText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        if (nextButtons != null)
        {
            nextButtons.ShowArrowAfterTyping();
        }
    }

    void SetImageAlpha(Image img, float alpha)
    {
        if (img == null || !img.enabled) return;

        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    void BuildAchievements()
    {
        unlocked.Clear();

        string d1 = GameProgress_JFM.day1SelectedItemName;
        string d2 = GameProgress_JFM.day2SelectedItemName;
        string d3 = GameProgress_JFM.day3SelectedItemName;
        string d4 = GameProgress_JFM.day4SelectedItemName;
        string d5 = GameProgress_JFM.day5SelectedItemName;
        string d6 = GameProgress_JFM.day6SelectedItemName;
        string d7 = GameProgress_JFM.day7SelectedItemName;

        // 1. Birthday
        if (d1 == "BirthdayCap" &&
            (d2 == "Balloon" || d5 == "Balloon_inflated") &&
            d6 == "GumballJar")
        {
            unlocked.Add("Always be ready to celebrate a birthday.");
        }

        // 2. God Mode
        if (d2 == "Cloud_01" &&
            d6 == "LowBrightness" &&
            d7 == "crocodile_menu")
        {
            unlocked.Add("God Mode");
        }

        // 3. Yellow Sponge
        if ((d1 == "Barrier" || d7 == "Barrier") &&
            d2 == "Broom")
        {
            unlocked.Add("As hardworking as a yellow sponge!");
        }

        // 4. Sweet Tooth
        if (d1 == "Cone" &&
            d6 == "GumballJar")
        {
            unlocked.Add("Even Doctors Visit the Dentist");
        }

        // 5. Secret Circus
        if (d2 == "Balloon" &&
            d3 == "Organizer" &&
            d5 == "lamp_0")
        {
            unlocked.Add("Secret Circus");
        }

        // 6. Square and Proper
        if (d3 == "keyboard" &&
            d4 == "IceTray")
        {
            unlocked.Add("Square and Proper");
        }

        // 7. F1 fans
        if (d3 == "FerrariEasterEgg" &&
            d5 == "lamp_0" &&
            d6 == "telescope")
        {
            unlocked.Add("Potential F1 racing fans");
        }
    }
}