using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewsSceneController : MonoBehaviour
{
    [Header("UI")]
    public Image tvScreenImage;
    public TMP_Text dialogueText;
    public RectTransform continueArrow;
    public CanvasGroup continueArrowCanvasGroup;

    [Header("Fallback Scene Flow")]
    public string fallbackNextSceneName = "day4_clinic";

    [Header("Optional News Images")]

    [Header("Default Image")]
    public Sprite defaultNewsSprite;

    [Header("Branch Images - Day2 News")]
    public Sprite doctorSprite;
    public Sprite coneSprite;
    public Sprite birthdayCapSprite;
    public Sprite barrierSprite;

    [Header("Branch Images - Day3 News")]
    public Sprite cloudSprite;
    public Sprite balloonSprite;
    public Sprite broomSprite;
    public Sprite reportSprite;

    [Header("Branch Images - Day4 News")]
    public Sprite FerrariSprite;
    public Sprite organizerSprite;
    public Sprite keyboardSprite;

    [Header("Branch Images - Day5 News")]
    public Sprite BallonSprite;
    public Sprite LampSprite;
    public Sprite TieSprite;

    [Header("Branch Images - Day6 News")]
    public Sprite TelescopeSprite;
    public Sprite lowBrightnessSprite;
    public Sprite GumSprite;

    [Header("Typewriter")]
    public float typeSpeed = 0.03f;

    [Header("Arrow Animation")]
    public float arrowFadeSpeed = 2.2f;
    public float arrowFloatSpeed = 2f;
    [Range(0f, 1f)] public float arrowMinAlpha = 0.3f;
    [Range(0f, 1f)] public float arrowMaxAlpha = 1f;
    public float arrowFloatAmount = 3f;

    [Header("Arrow Position")]
    public float arrowOffsetX = -2f;
    public float arrowOffsetY = -2f;

    private List<string> currentLines = new List<string>();
    private int currentIndex = 0;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool sceneReady = false;
    private bool isTransitioning = false; // 锁
    private bool isExiting = false;       // 防止重复切场景
    private bool canAdvance = false;

    private Vector2 arrowBasePos;

    void Start()
    {
        if (continueArrow != null)
            continueArrow.gameObject.SetActive(false);

        SetupBranchContent();
        ShowCurrentLine();
        sceneReady = true;
    }

    void Update()
    {
        if (isExiting) return;
        if (!sceneReady) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTransitioning)
            {
                return;
            }

            HandleClick();
        }

        if (!isTyping && continueArrow != null && continueArrow.gameObject.activeSelf)
        {
            AnimateArrow();
            UpdateArrowPosition();
        }
    }

    // ⭐⭐⭐ 新增：统一换图方法
    void SetNewsImage(Sprite sprite)
    {
        if (tvScreenImage == null) return;

        if (sprite != null)
            tvScreenImage.sprite = sprite;
        else if (defaultNewsSprite != null)
            tvScreenImage.sprite = defaultNewsSprite;
    }

    void SetupBranchContent()
    {
        currentLines.Clear();

        int newsDay = GameProgress_JFM.currentNewsDay;

        // 默认图
        SetNewsImage(defaultNewsSprite);

        switch (newsDay)
        {
            case 1:
                SetupDay1News();
                break;
            case 2:
                SetupDay2News();
                break;
            case 3:
                SetupDay3News();
                break;
            case 4:
                SetupDay4News();
                break;

            case 5:
                SetupDay5News();
                break;

            case 6:
                SetupDay6News();
                break;

            default:
                SetupFallbackNews();
                break;
        }
    }

    void SetupDay1News()
    {
        SetNewsImage(doctorSprite);
        currentLines.Add("With transportation expected to be disrupted for some time, ");
        currentLines.Add("Dr. Random’s clinic is currently the only one open in our forest.");
        currentLines.Add("Whether you are struggling with physical health issues or psychological distress, ");
        currentLines.Add("turning to Dr. Random is your best option.");
    }

    void SetupDay2News()
    {
        string day1 = GameProgress_JFM.day1SelectedItemName;

        currentLines.Add("Tonight's special report: unusual treatment methods continue to draw public attention.");

        if (day1 == "Cone")
        {
            SetNewsImage(coneSprite);
            
            currentLines.Add("In a surprising turn at today’s ceremony, ");
            currentLines.Add("the hero Mr. Rhino appeared with an ice cream cone as a horn decoration.");
            currentLines.Add("Mr. Fox vowed to pay for the Rhino family’s ice cream supply forever.");
            currentLines.Add("We have witnessed the sweetest victory.");
        }

        else if (day1 == "BirthdayCap")
        {
            SetNewsImage(birthdayCapSprite);

            currentLines.Add("Today’s ceremony turned into a special birthday celebration ");
            currentLines.Add("as Mr. Rhino appeared wearing a birthday hat.");
            currentLines.Add("Mr. Fox considered today as the symbol of his own rebirth day.");
            currentLines.Add("Mr. Fox shed tears of gratitude, triggering a massive sobbing from a Crocodile guest.");
        }
        else if (day1 == "Barrier")
        {
            SetNewsImage(barrierSprite);

            currentLines.Add("At the ceremony, Mr. Rhino appeared with a barrier marked STAFF,");
            currentLines.Add("announcing his plan to become a lifeguard to help those in need. ");
            currentLines.Add("Inspired by this idea, Mr. Fox donated a massive fund to establish and operate a new rescue team.");
        }
    }

    void SetupDay3News()
    {
        string day2 = GameProgress_JFM.day2SelectedItemName;
        string day3 = GameProgress_JFM.day3SelectedItemName;

        // ⭐ Day2 结果影响图片
        if (day2 == "Cloud_01")
        {
            SetNewsImage(cloudSprite);
            currentLines.Add("The winner of the new Lion King election has sparked widespread discussion ");
            currentLines.Add("due to his incredibly fluffy mane. ");
            currentLines.Add("When asked for his key, he replied with a smile: “Back to nature”. ");
            currentLines.Add("Being softer and real is also why he has gained so much support.");

            currentLines.Add("Attention: Due to power shortages, ");
            currentLines.Add("some areas might experience temporary darkness tomorrow. Don't panic. ");
            currentLines.Add("Get your candles or flashlights ready. ");
            currentLines.Add("Inviting your firefly friends for a party is also a good choice.");
        }

        else if (day2 == "Balloon")
        {
            SetNewsImage(balloonSprite);
            currentLines.Add("A terrifying explosion shook the Lion King election, but fortunately, no one was hurt. ");
            currentLines.Add("The candidate who remained remarkably calm on stage won. ");
            currentLines.Add("His mane appeared thinner than the others, ");
            currentLines.Add("voters believe it’s because he spends more time worrying for others.");

            currentLines.Add("Attention: Due to power shortages, ");
            currentLines.Add("some areas might experience temporary darkness tomorrow. Don't panic. ");
            currentLines.Add("Get your candles or flashlights ready. ");
            currentLines.Add("Inviting your firefly friends for a party is also a good choice.");
        }

        
        else if (day2 == "Broom")
        {
            SetNewsImage(broomSprite);
            currentLines.Add("Attention: Due to power shortages, ");
            currentLines.Add("some areas might experience temporary darkness tomorrow. Don't panic. ");
            currentLines.Add("Get your candles or flashlights ready. ");
            currentLines.Add("Inviting your firefly friends for a party is also a good choice.");
        }
        
        
    }

    void SetupDay4News()
    {
        string day3 = GameProgress_JFM.day3SelectedItemName;

        if (day3 == "keyboard")
        {
            SetNewsImage(keyboardSprite);
            currentLines.Add("Miss Dark Horse has stunned the forest,");
            currentLines.Add("performing Flight of the Bumblebee through tap dance.");
            currentLines.Add("The Grand Theater announced a special deal: successfully mimic four bars to win a free ticket.");
            currentLines.Add("As the video goes viral, the streets are filled with fans practicing their footwork.");
        }

        else if (day3 == "Organizer")
        {
            SetNewsImage(organizerSprite);
            currentLines.Add("Miss Dark Horse, a new NPC at the amusement park’s kid zone, ");
            currentLines.Add("is believed to be magical. She juggles the gashapon handed over by kids with ease. ");
            currentLines.Add("Followers on the internet hope the traffic gets back to normal quickly ");
            currentLines.Add("so they can revisit their childhood.");
        }

        else if (day3 == "FerrariEasterEgg")
        {
            SetNewsImage(FerrariSprite);
            currentLines.Add("After a period of struggle, Team F secured a legendary P1 and P2 finish,");
            currentLines.Add("proving that their glory never faded. ");
            currentLines.Add("Fans had prayed for better engines and good luck for their perfect drivers,");
            currentLines.Add("and they gained half of what they asked for.");
            currentLines.Add("The team now believes this turnaround is related to their new model and new logo:");
            currentLines.Add("“MUST BE THE DARK HORSE!”");
        }
    }

    void SetupDay5News()
    {
        string day4 = GameProgress_JFM.day4SelectedItemName;
        string day5 = GameProgress_JFM.day5SelectedItemName;

        if (day5 == "Balloon_inflated")
        {
            SetNewsImage(BallonSprite);
            currentLines.Add("Despite Mr. Seagull's technical precision,");
            currentLines.Add("fans think he needs to take better care of his vocal folds. ");
            currentLines.Add("Last time he sounded like a bull, and this time, he was more like a mouse or an alien.");
            currentLines.Add("Some are even threatening to shout for a refund ");
            currentLines.Add("if he keeps mimicking other animals during his performances.");

            currentLines.Add("Corn is the new gold! Recently, corn becomes extremely popular the among gym lovers.");
            currentLines.Add("They are using “Make the Corn Great Again” as their slogan，");
            currentLines.Add("the motivations behind remain a mystery.");
        }

        else if (day5 == "lamp_0")
        {
            SetNewsImage(LampSprite);
            currentLines.Add("Fans were shocked when Mr. Seagull appeared on stage ");
            currentLines.Add("singing through a megaphone instead of his professional mic. ");
            currentLines.Add("Fans are accusing the management of withholding a fan-customized microphone. ");
            currentLines.Add("The hashtag #FreeTheMic is now trending.");

            currentLines.Add("Corn is the new gold! Recently, corn becomes extremely popular the among gym lovers.");
            currentLines.Add("They are using “Make the Corn Great Again” as their slogan，");
            currentLines.Add("the motivations behind remain a mystery.");
        }

        else if (day5 == "tie")
        {
            SetNewsImage(TieSprite);
            currentLines.Add("Corn is the new gold! Recently, corn becomes extremely popular the among gym lovers.");
            currentLines.Add("They are using “Make the Corn Great Again” as their slogan，");
            currentLines.Add("the motivations behind remain a mystery.");
        }


    }

    void SetupDay6News()
    {
        string day5 = GameProgress_JFM.day5SelectedItemName;
        string day6 = GameProgress_JFM.day6SelectedItemName;

        if (day6 == "GumballJar")
        {
            SetNewsImage(GumSprite);
            currentLines.Add("Miss Chameleon has nearly succeeded in constructing her own hot air balloon.");
            currentLines.Add("The secret behind it is her exceptionally long tongue,");
            currentLines.Add("which helps her blow incredibly large, durable bubbles. ");
            currentLines.Add("Miss Chameleon plans to conduct her maiden flight as soon as the weather clears.");
        }

        else if (day6 == "LowBrightness")
        {
            SetNewsImage(lowBrightnessSprite);
            currentLines.Add("Multiple forest residents claim they lost their sight for several hours today.");
            currentLines.Add("It was initially mistaken for an unusually early sunset,");
            currentLines.Add("as residents suddenly found themselves plunged into total darkness in the mid-afternoon. ");
            currentLines.Add("Coupled with the recent non-stop rain, many wonder:");
            currentLines.Add("Is our climate becoming London-style?");

        }

        else if (day6 == "telescope")
        {
            SetNewsImage(TelescopeSprite);
        }

        currentLines.Add("Mr. Seagull’s new single has moved countless listeners to tears,");
        currentLines.Add("with many claiming “this is true music”. ");
        currentLines.Add("One viral comment states, “This song saved my life. ");
        currentLines.Add("I was about to be eaten by a crocodile, but then this song played.");
        currentLines.Add("Instead of attacking, he just sat there and started crying with me.");


    }


    void SetupFallbackNews()
    {
        currentLines.Add("Tonight's report is currently unavailable.");
        currentLines.Add("Please stand by for further updates.");
    }

    void ShowCurrentLine()
    {
        if (dialogueText == null) return;
        if (currentLines.Count == 0) return;

        isTransitioning = true; //加锁

        StopTypingOnly();

        if (continueArrow != null)
            continueArrow.gameObject.SetActive(false);

        typingCoroutine = StartCoroutine(TypeLine(currentLines[currentIndex]));
    }

    IEnumerator UnlockNextFrame()
    {
        yield return null; // 等一帧

        isTransitioning = false;
    }

    void HandleClick()
    {
        if (isTyping)
        {
            StopTypingOnly();
            dialogueText.text = currentLines[currentIndex];
            isTyping = false;

            ShowArrow();

            canAdvance = true;
            return;
        }

        if (!canAdvance) return;

        canAdvance = false;

        AdvanceDialogue();
    }

    void AdvanceDialogue()
    {
        if (isExiting) return;

        isTransitioning = true;

        // 先判断是不是最后一句
        if (currentIndex == currentLines.Count - 1)
        {
            StartCoroutine(ExitWithDelay());
            return;
        }

        currentIndex++;

        ShowCurrentLine();

        StartCoroutine(UnlockNextFrame());
    }

    void GoToNextScene()
    {
        StopTypingOnly();

        string targetScene = GameProgress_JFM.nextSceneAfterNews;

        if (string.IsNullOrEmpty(targetScene))
            targetScene = fallbackNextSceneName;

        SceneManager.LoadScene(targetScene);
    }

    IEnumerator ExitWithDelay()
    {
        if (isExiting) yield break;

        isExiting = true;
        isTransitioning = true;

        yield return new WaitForSeconds(0.1f);

        GoToNextScene();
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;

        ShowArrow();
        canAdvance = true;

        // 锁一帧（关键）
        isTransitioning = true;
        StartCoroutine(UnlockNextFrame());


    }

    void ShowArrow()
    {
        if (continueArrow == null) return;

        continueArrow.gameObject.SetActive(true);
        UpdateArrowPosition();

        arrowBasePos = continueArrow.anchoredPosition;

        if (continueArrowCanvasGroup != null)
            continueArrowCanvasGroup.alpha = 1f;
    }

    void UpdateArrowPosition()
    {
        if (dialogueText == null || continueArrow == null) return;
        if (string.IsNullOrEmpty(dialogueText.text)) return;

        dialogueText.ForceMeshUpdate();

        TMP_TextInfo textInfo = dialogueText.textInfo;
        if (textInfo.characterCount == 0) return;

        int lastVisibleCharIndex = textInfo.characterCount - 1;

        for (int i = textInfo.characterCount - 1; i >= 0; i--)
        {
            if (textInfo.characterInfo[i].isVisible)
            {
                lastVisibleCharIndex = i;
                break;
            }
        }

        TMP_CharacterInfo charInfo = textInfo.characterInfo[lastVisibleCharIndex];

        Vector3 charTopRight = charInfo.topRight;
        Vector3 worldPos = dialogueText.transform.TransformPoint(charTopRight);
        Vector2 localPos;

        RectTransform parentRect = continueArrow.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out localPos
        );

        continueArrow.anchoredPosition = localPos + new Vector2(arrowOffsetX, arrowOffsetY);
        arrowBasePos = continueArrow.anchoredPosition;
    }

    void AnimateArrow()
    {
        if (continueArrow == null) return;

        float fadeT = (Mathf.Sin(Time.time * arrowFadeSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(arrowMinAlpha, arrowMaxAlpha, fadeT);

        if (continueArrowCanvasGroup != null)
            continueArrowCanvasGroup.alpha = alpha;

        float y = Mathf.Sin(Time.time * arrowFloatSpeed) * arrowFloatAmount;
        continueArrow.anchoredPosition = arrowBasePos + new Vector2(0f, y);
    }

    void StopTypingOnly()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }


    }
}