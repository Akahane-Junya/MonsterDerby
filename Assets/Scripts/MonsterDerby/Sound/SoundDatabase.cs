using UnityEngine;

[CreateAssetMenu(menuName = "Sound/SoundDatabase")]
public class SoundDatabase : ScriptableObject
{
    public SoundEntry[] bgms;
    public SoundEntry[] ses;

    public AudioClip GetBgm(string name)
    {
        foreach (var entry in bgms)
            if (entry.name == name) return entry.clip;
        return null;
    }

    public AudioClip GetSe(string name)
    {
        foreach (var entry in ses)
            if (entry.name == name) return entry.clip;
        return null;
    }
}
