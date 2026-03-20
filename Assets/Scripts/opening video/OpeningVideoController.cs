using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class OpeningVideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "day1_clinic"; 

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.loopPointReached += OnVideoEnd;

        videoPlayer.Play(); // È·±£²¥·Å
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}