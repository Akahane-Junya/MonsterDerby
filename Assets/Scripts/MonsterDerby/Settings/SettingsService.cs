
using UnityEngine;
using MonsterDerby.Domain.World;

public class SettingsService : ISettingsService
{
    public SettingsData Current { get; private set; } = new SettingsData();
    private WorldState _worldState;

    public SettingsService(WorldState worldState)
    {
        _worldState = worldState;
        Load();
    }

    public void Apply(SettingsData data)
    {
        Current = data;
        // WorldStateの新インスタンスを生成しSettingsを反映
        _worldState = new WorldState(
            _worldState.Money,
            _worldState.CurrentMonster,
            _worldState.AwardEntries,
            data
        );
        // 各システムに反映（例：音量、画面、言語など）
    }

    public void Save()
    {
        // WorldStateの新インスタンスを生成しSettingsを反映
        _worldState = new WorldState(
            _worldState.Money,
            _worldState.CurrentMonster,
            _worldState.AwardEntries,
            Current
        );
        // WorldStateのセーブ処理を呼ぶ
    }

    public void Load()
    {
        if (_worldState.Settings != null)
            Current = _worldState.Settings;
        else
            Current = new SettingsData();
    }

    public void ResetToDefault()
    {
        Current.ResetToDefault();
        Apply(Current);
    }
}
