using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MonsterDerby.Domain.Catalog;

namespace MonsterDerby.Infrastructure.Save
{
    /// <summary>
    /// ScriptableObjectベースの図鑑アンロック状態リポジトリ
    /// </summary>
    [CreateAssetMenu(fileName = "CatalogUnlockRepository", menuName = "MonsterDerby/Catalog/UnlockRepository")]
    public sealed class CatalogUnlockRepositoryAsset : ScriptableObject, ICatalogUnlockRepository
    {
        [SerializeField] private CatalogUnlockState[] _monsterCatalogStates;
        [SerializeField] private CatalogUnlockState[] _skillCatalogStates;

        public CatalogUnlockStage GetMonsterUnlockStage(string monsterId)
        {
            var state = _monsterCatalogStates?.FirstOrDefault(x => x.Id == monsterId);
            return state?.Stage ?? CatalogUnlockStage.None;
        }

        public CatalogUnlockStage GetSkillUnlockStage(string skillId)
        {
            var state = _skillCatalogStates?.FirstOrDefault(x => x.Id == skillId);
            return state?.Stage ?? CatalogUnlockStage.None;
        }

        public void SetMonsterUnlockStage(string monsterId, CatalogUnlockStage stage)
        {
            var state = _monsterCatalogStates?.FirstOrDefault(x => x.Id == monsterId);
            if (state != null)
                state.Stage = stage;
        }

        public void SetSkillUnlockStage(string skillId, CatalogUnlockStage stage)
        {
            var state = _skillCatalogStates?.FirstOrDefault(x => x.Id == skillId);
            if (state != null)
                state.Stage = stage;
        }

        public IReadOnlyList<CatalogUnlockState> GetAllMonsterStates() => _monsterCatalogStates;
        public IReadOnlyList<CatalogUnlockState> GetAllSkillStates() => _skillCatalogStates;
    }
}
