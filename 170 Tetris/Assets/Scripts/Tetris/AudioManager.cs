using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource soundPlace;
    public AudioSource soundClear;
    public AudioSource soundBuy;
    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySoundPlace()
    {
        soundPlace.Play();
    }

    public void PlaySoundClear()
    {
        soundClear.Play();
    }

    public void PlaySoundBuy()
    {
        soundBuy.Play();
    }
}
