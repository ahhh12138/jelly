using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioClip clickSound;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}