using UnityEngine;

public class FerrariPopupController : MonoBehaviour
{
    public AudioClip successClip;

    public void PlaySuccessSound()
    {
        if (successClip == null) return;

        GameObject tempGO = new GameObject("TempAudio");
        AudioSource source = tempGO.AddComponent<AudioSource>();

        source.clip = successClip;
        source.ignoreListenerPause = true; //TriggerFerrariEasterEgg无视暂停
        source.Play();

        Destroy(tempGO, successClip.length);
    }
}