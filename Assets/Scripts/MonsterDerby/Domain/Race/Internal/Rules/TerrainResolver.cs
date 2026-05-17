namespace MonsterDerby.Domain.Race
{
    using MonsterDerby.Domain.Course;

    internal static class TerrainResolver
    {
        public static string GetTerrainTagAt(CourseDefinition course, float distanceMeters)
        {
            // 単純な線形走査（分かりやすさ優先）
            foreach (var seg in course.Segments)
            {
                if (distanceMeters >= seg.StartMeters && distanceMeters < seg.EndMeters)
                    return seg.TerrainTag;
            }
            return string.Empty;
        }
    }
}