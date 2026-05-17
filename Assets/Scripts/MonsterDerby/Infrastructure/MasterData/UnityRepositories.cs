// Unity用 MasterData 追加実装
// 目的:
//  - ScriptableObject で編集可能にする
//  - Domain には依存させない（Infrastructure -> Domain 方向のみ）

using System.Collections.Generic;
using MonsterDerby.Domain.Course;
using MonsterDerby.Domain.Skill;
using MonsterDerby.Domain.SharedKernel;

namespace MonsterDerby.Infrastructure.MasterData
{
    public sealed class UnitySkillRepository : ISkillRepository
    {
        private readonly Dictionary<SkillId, SkillDefinition> _map;

        public UnitySkillRepository(SkillDatabaseSO db)
        {
            _map = db.BuildDomainMap();
        }

        public SkillDefinition GetSkillDefinition(SkillId skillId)
        {
            if (!_map.TryGetValue(skillId, out var def))
                throw new KeyNotFoundException("Skill not found: " + skillId);
            return def;
        }
    }

    public sealed class UnityCourseRepository : ICourseRepository
    {
        private readonly Dictionary<CourseId, CourseDefinition> _map;

        public UnityCourseRepository(CourseDefinitionSO[] courses)
        {
            _map = new Dictionary<CourseId, CourseDefinition>();
            foreach (var c in courses)
                _map[new CourseId(c.courseId)] = c.ToDomain();
        }

        public CourseDefinition GetCourseDefinition(CourseId courseId)
        {
            if (!_map.TryGetValue(courseId, out var def))
                throw new KeyNotFoundException("Course not found: " + courseId);
            return def;
        }
    }
}
