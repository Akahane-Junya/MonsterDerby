public interface ISoundService
{
    void PlayBgm(string name);
    void StopBgm();
    void PlaySe(string name);
    void SetBgmVolume(float volume);
    void SetSeVolume(float volume);
}
