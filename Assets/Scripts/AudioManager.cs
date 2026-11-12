using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Effects")]
    public AudioClip jumpClip;
    public AudioClip attackClip;
    public AudioClip hurtClip;
    public AudioClip deathClip;
    public AudioClip levelCompleteClip;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}