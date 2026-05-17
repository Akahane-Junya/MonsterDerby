using System;
using System.Collections.Generic;
using System.Linq;
using MonsterDerby.Domain.Course;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Infrastructure.MasterData;

namespace MonsterDerby.Infrastructure.Repositories
{
    /// <summary>
    /// ScriptableObjectベースのCourseRepository実装
    /// </summary>
    public sealed class ScriptableObjectCourseRepository : ICourseRepository
    {
        private readonly Dictionary<CourseId, CourseDefinition> _cache;

        public ScriptableObjectCourseRepository(MasterDataCatalog catalog)
        {
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            // ScriptableObject → Domain 変換してキャッシュ
            _cache = catalog.Courses
                .Where(so => so != null)
                .Select(so => so.ToDomain())
                .ToDictionary(def => def.CourseId);

            if (_cache.Count == 0)
                UnityEngine.Debug.LogWarning("ScriptableObjectCourseRepository: コースが1つも登録されていません。");
        }

        public CourseDefinition GetCourseDefinition(CourseId courseId)
        {
            if (courseId == null)
                throw new ArgumentNullException(nameof(courseId));

            if (!_cache.TryGetValue(courseId, out var definition))
                throw new KeyNotFoundException($"Course not found: {courseId}");

            return definition;
        }
    }
}