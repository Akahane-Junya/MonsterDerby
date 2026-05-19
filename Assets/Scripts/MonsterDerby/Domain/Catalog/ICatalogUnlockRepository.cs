using System.Collections.Generic;

namespace MonsterDerby.Domain.Catalog
{
    /// <summary>
    /// 図鑑アンロック状態の取得・保存リポジトリ
    /// </summary>
    public interface ICatalogUnlockRepository
    {
        CatalogUnlockStage GetMonsterUnlockStage(string monsterId);
        CatalogUnlockStage GetSkillUnlockStage(string skillId);
        void SetMonsterUnlockStage(string monsterId, CatalogUnlockStage stage);
        void SetSkillUnlockStage(string skillId, CatalogUnlockStage stage);
        IReadOnlyList<CatalogUnlockState> GetAllMonsterStates();
        IReadOnlyList<CatalogUnlockState> GetAllSkillStates();
    }
}
