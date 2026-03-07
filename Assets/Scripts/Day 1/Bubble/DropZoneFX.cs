using UnityEngine;

public class DropZoneFX : MonoBehaviour
{
    public ParticleSystem successStars;

    public void PlaySuccess()
    {
        if (successStars != null) successStars.Play();
    }
}