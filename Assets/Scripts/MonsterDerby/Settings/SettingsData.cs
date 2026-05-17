using System;
using UnityEngine;

[Serializable]
public class SettingsData
{
    [Range(0f, 1f)] public float BgmVolume = 1f;
    [Range(0f, 1f)] public float SeVolume = 1f;
    public bool IsFullScreen = true;
    public string Resolution = "1920x1080";
    public string WindowMode = "FullScreen"; // "FullScreen", "Windowed", "Borderless" など
    public string Language = "ja";
    public KeyConfigData KeyConfig = new KeyConfigData();
    // 必要に応じて他の設定項目も追加
    public void ResetToDefault()
    {
        BgmVolume = 1f;
        SeVolume = 1f;
        IsFullScreen = true;
        Resolution = "1920x1080";
        WindowMode = "FullScreen";
        Language = "ja";
        KeyConfig = new KeyConfigData();
    }
}

[Serializable]
public class KeyConfigData
{
    // キー設定内容をここに定義（例：Dictionary<string, KeyCode>など）
}
