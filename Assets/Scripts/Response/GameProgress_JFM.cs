public static class GameProgress_JFM
{
    // Final selected items for each day
    public static string day1SelectedItemName;
    public static string day2SelectedItemName;
    public static string day3SelectedItemName;
    public static string day4SelectedItemName;
    public static string day5SelectedItemName;
    public static string day6SelectedItemName;

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

        currentNewsDay = 0;
        nextSceneAfterNews = "";
    }
}