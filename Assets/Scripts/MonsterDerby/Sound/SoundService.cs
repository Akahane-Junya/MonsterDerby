using UnityEngine;

public class SoundService : MonoBehaviour, ISoundService
{
    [SerializeField] private SoundDatabase soundDatabase;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    public void PlayBgm(string name)
    {
        var clip = soundDatabase.GetBgm(name);
        if (clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    public void StopBgm()
    {
        bgmSource.Stop();
    }

    public void PlaySe(string name)
    {
        var clip = soundDatabase.GetSe(name);
        if (clip != null)
        {
            seSource.PlayOneShot(clip);
        }
    }

    public void SetBgmVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    public void SetSeVolume(float volume)
    {
        seSource.volume = volume;
    }
}
