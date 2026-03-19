using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class achievcontroller : MonoBehaviour
{
    [Header("Optional Result Images")]
    public Image day1Image;
    public Image day2Image;
    public Image day3Image;
    public Image day4Image;
    public Image day5Image;
    public Image day6Image;

    [Header("Optional Placeholder")]
    public Sprite placeholderSprite;

    [Header("Achievement Text")]
    public TMP_Text achievementText;

    void Start()
    {
        RefreshPanel();
    }

    public void RefreshPanel()
    {
        SetupImage(day1Image, GameProgress_JFM.day1SelectedSprite);
        SetupImage(day2Image, GameProgress_JFM.day2SelectedSprite);
        SetupImage(day3Image, GameProgress_JFM.day3SelectedSprite);
        SetupImage(day4Image, GameProgress_JFM.day4SelectedSprite);
        SetupImage(day5Image, GameProgress_JFM.day5SelectedSprite);
        SetupImage(day6Image, GameProgress_JFM.day6SelectedSprite);

        UpdateAchievements();
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
        else if (placeholderSprite != null)
        {
            targetImage.sprite = placeholderSprite;
            targetImage.enabled = true;
            targetImage.preserveAspect = true;
        }
        else
        {
            targetImage.enabled = false;
        }
    }

    void UpdateAchievements()
    {
        if (achievementText == null) return;

        List<string> unlocked = new List<string>();

        string d1 = GameProgress_JFM.day1SelectedItemName;
        string d2 = GameProgress_JFM.day2SelectedItemName;
        string d3 = GameProgress_JFM.day3SelectedItemName;
        string d4 = GameProgress_JFM.day4SelectedItemName;
        string d5 = GameProgress_JFM.day5SelectedItemName;
        string d6 = GameProgress_JFM.day6SelectedItemName;

        // 1. 该过生日了
        // 1-BirthdayCap 2/5-Balloon 6-GumballJar
        if (
            d1 == "BirthdayCap" &&
            (d2 == "Balloon" || d5 == "Balloon_inflated") &&
            d6 == "GumballJar"
        )
        {
            unlocked.Add("该过生日了\nAlways be ready to celebrate a birthday.");
        }

        // 2. 造物主 / 管理员模式
        // 2-Cloud_01 6-LowBrightness
        if (
            d2 == "Cloud_01" &&
            d6 == "LowBrightness"
        )
        {
            unlocked.Add("造物主 / 管理员模式\nGod Mode");
        }

        // 3. 工作热情堪比黄色海绵
        // 1-Barrier 2-Broom
        if (
            d1 == "Barrier" &&
            d2 == "Broom"
        )
        {
            unlocked.Add("工作热情堪比黄色海绵\nAs hardworking as a yellow sponge!");
        }

        // 4. 爱吃甜食的医生
        // 1-Cone 6-GumballJar
        if (
            d1 == "Cone" &&
            d6 == "GumballJar"
        )
        {
            unlocked.Add("爱吃甜食的医生\nEven Doctors Visit the Dentist");
        }

        // 5. 地下马戏团
        // 2-Balloon 3-Organizer 5-lamp_0
        if (
            d2 == "Balloon" &&
            d3 == "Organizer" &&
            d5 == "lamp_0"
        )
        {
            unlocked.Add("地下马戏团\nSecret Circus");
        }

        // 6. 为人方正
        // 3-keyboard 4-IceTray
        if (
            d3 == "keyboard" &&
            d4 == "IceTray"
        )
        {
            unlocked.Add("为人方正\nSquare and Proper");
        }

        // 7. 赛车迷
        // 3-FerrariEasterEgg 5-lamp_0 6-telescope
        if (
            d3 == "FerrariEasterEgg" &&
            d5 == "lamp_0" &&
            d6 == "telescope"
        )
        {
            unlocked.Add("赛车迷\nPotential F1 racing fans");
        }

        if (unlocked.Count == 0)
        {
            achievementText.text = "No achievements unlocked yet.";
        }
        else
        {
            achievementText.text = string.Join("\n\n", unlocked);
        }
    }
}