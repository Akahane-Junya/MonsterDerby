public interface ISettingsService
{
    SettingsData Current { get; }
    void Apply(SettingsData data);
    void Save();
    void Load();
    void ResetToDefault();
}
