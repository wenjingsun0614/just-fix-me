using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    public AudioSource audioSource;

    public AudioClip gameMusic;
    public AudioClip menuMusic;

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
        if (scene.name == "opening")
        {
            StopMusic();
            return;
        }

        // 主菜单
        if (scene.name == "main_menu")
        {
            PlayMusic(menuMusic);
            return;
        }

        // 所有 clinic 场景
        if (scene.name.Contains("_clinic"))
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