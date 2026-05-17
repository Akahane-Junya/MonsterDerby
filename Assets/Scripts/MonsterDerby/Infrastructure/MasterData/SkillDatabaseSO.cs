// Unity用 MasterData 追加実装
// 目的:
//  - ScriptableObject で編集可能にする
//  - Domain には依存させない（Infrastructure -> Domain 方向のみ）

using UnityEngine;
using System.Collections.Generic;
using MonsterDerby.Domain.Skill;
using MonsterDerby.Domain.SharedKernel;

namespace MonsterDerby.Infrastructure.MasterData
{
    [CreateAssetMenu(menuName = "MonsterDerby/MasterData/SkillDatabase")]
    public sealed class SkillDatabaseSO : ScriptableObject
    {
        public SkillDefinitionSO[] skills;

        public Dictionary<SkillId, SkillDefinition> BuildDomainMap()
        {
            var dict = new Dictionary<SkillId, SkillDefinition>();
            foreach (var so in skills)
            {
                var def = so.ToDomain();
                dict[new SkillId(so.skillId)] = def;
            }
            return dict;
        }
    }
}
