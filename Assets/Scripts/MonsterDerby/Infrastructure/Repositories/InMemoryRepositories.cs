// MonsterDerby.Infrastructure（C# 9 / 例）
// 目的:
//  - Domain の Repository Interface を、Unity/データ形式の都合で実装する場所
//  - Domain へ依存方向を逆流させない（Infrastructure -> Domain）

namespace MonsterDerby.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using MonsterDerby.Domain.Course;
    using MonsterDerby.Domain.Skill;
    using MonsterDerby.Domain.SharedKernel;

    public sealed class InMemoryCourseRepository : ICourseRepository
    {
        private readonly Dictionary<CourseId, CourseDefinition> _map;

        public InMemoryCourseRepository(Dictionary<CourseId, CourseDefinition> map)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public CourseDefinition GetCourseDefinition(CourseId courseId)
        {
            if (!_map.TryGetValue(courseId, out var def))
                throw new KeyNotFoundException("Course not found: " + courseId);
            return def;
        }
    }

    public sealed class InMemorySkillRepository : ISkillRepository
    {
        private readonly Dictionary<SkillId, SkillDefinition> _map;

        public InMemorySkillRepository(Dictionary<SkillId, SkillDefinition> map)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public SkillDefinition GetSkillDefinition(SkillId skillId)
        {
            if (!_map.TryGetValue(skillId, out var def))
                throw new KeyNotFoundException("Skill not found: " + skillId);
            return def;
        }
    }
}
