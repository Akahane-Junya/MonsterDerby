using System;
using System.Collections.Generic;
using System.Linq;
using MonsterDerby.Domain.Skill;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Infrastructure.MasterData;

namespace MonsterDerby.Infrastructure.Repositories
{
    /// <summary>
    /// ScriptableObjectベースのSkillRepository実装
    /// </summary>
    public sealed class ScriptableObjectSkillRepository : ISkillRepository
    {
        private readonly Dictionary<SkillId, SkillDefinition> _cache;
        private readonly Dictionary<string, SkillDefinitionSO> _soCache;

        public ScriptableObjectSkillRepository(MasterDataCatalog catalog)
        {
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            var sos = catalog.Skills.Where(so => so != null).ToArray();

            _cache = sos
                .Select(so => so.ToDomain())
                .ToDictionary(def => def.SkillId);

            _soCache = sos
                .Where(so => !string.IsNullOrEmpty(so.skillId))
                .ToDictionary(so => so.skillId, StringComparer.Ordinal);

            if (_cache.Count == 0)
                UnityEngine.Debug.LogWarning("ScriptableObjectSkillRepository: スキルが1つも登録されていません。");
        }

        public SkillDefinition GetSkillDefinition(SkillId skillId)
        {
            if (skillId == null)
                throw new ArgumentNullException(nameof(skillId));

            if (!_cache.TryGetValue(skillId, out var definition))
                throw new KeyNotFoundException($"Skill not found: {skillId}");

            return definition;
        }

        public SkillDefinitionSO TryGetSO(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return null;
            _soCache.TryGetValue(skillId, out var so);
            return so;
        }

        public SkillId[] GetAllSkillIds()
        {
            return _cache.Keys.ToArray();
        }
    }
}