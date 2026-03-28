using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundAndLoad : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        DontDestroyOnLoad(gameObject);
    }
    //void Start()
    //{
    //    audioSource = GetComponent<AudioSource>();
    //}

    // 开始游戏
    public void PlaySoundAndStart()
    {
        PlaySound();
        Invoke("LoadMainScene", 0.1f); // 延迟 0.1 秒！
    }

    // 重新开始
    public void PlaySoundAndRestart()
    {
        PlaySound();
        Invoke("LoadStartScene", 0.1f); // 延迟 0.1秒！
    }

    void PlaySound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    void LoadMainScene()
    {
        SceneManager.LoadScene("Main");
    }

    void LoadStartScene()
    {
        SceneManager.LoadScene("StartScene");
    }
}