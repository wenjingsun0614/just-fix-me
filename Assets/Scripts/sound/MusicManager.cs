using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    
    public static MusicManager instance;
    public AudioSource audioSource;

    public AudioClip gameMusic;
    public AudioClip menuMusic;
    public AudioClip newsMusic;

    public float fadeDuration = 1.5f; // 淡出时间

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // 自动获取，防止丢引用
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 确保当前场景也执行一次
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("进入场景：" + scene.name);

        // opening → 关闭音乐
        if (scene.name == "opening" )
        {
            StopMusic();
            return;
        }

        // day8 → 音乐渐出
        if (scene.name == "day8_clinic")
        {
            StartCoroutine(FadeOutMusic());
            return;
        }

        IEnumerator FadeOutMusic()
        {
            if (audioSource == null) yield break;

            float startVolume = audioSource.volume;

            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = startVolume; // 恢复音量（下次用）
        }

        // 主菜单
        if (scene.name == "main_menu")
        {
            PlayMusic(menuMusic);
            return;
        }

        // news
        if (scene.name == "news_scenes")
        {
            PlayMusic(newsMusic);
            return;
        }

        // 所有 clinic 场景
        if (scene.name.Contains("_clinic") && scene.name != "day8_clinic")
        {
            PlayMusic(gameMusic);
            return;
        }
    }
        

    void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("音乐没拖！");
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource.clip == clip && audioSource.isPlaying) return;

        audioSource.clip = clip;
        audioSource.Play();
    }

    void StopMusic()
    {
        if (audioSource == null) return;

        audioSource.Stop();
    }
}