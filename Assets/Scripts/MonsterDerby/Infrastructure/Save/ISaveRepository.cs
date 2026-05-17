using MonsterDerby.Domain.World;

namespace MonsterDerby.Infrastructure.Save
{
    /// <summary>
    /// 永続化の抽象。
    /// 保存手段（PlayerPrefs / JSON / File）は差し替え可能。
    /// </summary>
    public interface ISaveRepository
    {
        void Save(WorldState state);
        bool TryLoad(out WorldState state);
        bool Exists();
        void Delete(); // タイトルの「データ削除」が必要なら使う
    }
}
