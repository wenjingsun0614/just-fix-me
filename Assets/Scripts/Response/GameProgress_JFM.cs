using UnityEngine;

public static class GameProgress_JFM
{
    // Final selected item names for each day
    public static string day1SelectedItemName = "";
    public static string day2SelectedItemName = "";
    public static string day3SelectedItemName = "";
    public static string day4SelectedItemName = "";
    public static string day5SelectedItemName = "";
    public static string day6SelectedItemName = "";
    public static string day7SelectedItemName = "";

    // Final selected item sprites for each day
    public static Sprite day1SelectedSprite;
    public static Sprite day2SelectedSprite;
    public static Sprite day3SelectedSprite;
    public static Sprite day4SelectedSprite;
    public static Sprite day5SelectedSprite;
    public static Sprite day6SelectedSprite;
    public static Sprite day7SelectedSprite;

    // Which day's news should NewsScene display?
    public static int currentNewsDay = 0;

    // Which scene should load after the news finishes?
    public static string nextSceneAfterNews = "";

    // Optional helper: reset all progress
    public static void ResetProgress()
    {
        day1SelectedItemName = "";
        day2SelectedItemName = "";
        day3SelectedItemName = "";
        day4SelectedItemName = "";
        day5SelectedItemName = "";
        day6SelectedItemName = "";
        day7SelectedItemName = "";

        day1SelectedSprite = null;
        day2SelectedSprite = null;
        day3SelectedSprite = null;
        day4SelectedSprite = null;
        day5SelectedSprite = null;
        day6SelectedSprite = null;
        day7SelectedSprite = null;

        currentNewsDay = 0;
        nextSceneAfterNews = "";
    }
}