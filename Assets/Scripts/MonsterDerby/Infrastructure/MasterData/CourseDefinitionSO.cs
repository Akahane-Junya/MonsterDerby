// Unity用 MasterData 追加実装
// 目的:
//  - ScriptableObject で編集可能にする
//  - Domain には依存させない（Infrastructure -> Domain 方向のみ）

using UnityEngine;
using MonsterDerby.Domain.Course;
using MonsterDerby.Domain.SharedKernel;

namespace MonsterDerby.Infrastructure.MasterData
{
    [CreateAssetMenu(menuName = "MonsterDerby/MasterData/Course")]
    public sealed class CourseDefinitionSO : ScriptableObject
    {
        public enum TerrainType
        {
            Grassland,
            Cave,
            Ice,
            Magma
        }

        [System.Serializable]
        public sealed class SegmentEntry
        {
            public float startMeters;
            public float endMeters;
            public TerrainType terrain;
        }

        public string courseId;
        public string courseName;
        public float lengthMeters;
        public SegmentEntry[] segments;

        public CourseDefinition ToDomain()
        {
            CourseSegment[] domainSegments;
            if (segments == null || segments.Length == 0)
            {
                domainSegments = new[]
                {
                    new CourseSegment(0f, lengthMeters, "grassland")
                };
            }
            else
            {
                domainSegments = new CourseSegment[segments.Length];
                for (int i = 0; i < segments.Length; i++)
                {
                    var src = segments[i];
                    var start = src != null ? src.startMeters : 0f;
                    var end = src != null ? src.endMeters : lengthMeters;
                    var terrain = src != null ? src.terrain : TerrainType.Grassland;
                    domainSegments[i] = new CourseSegment(start, end, ToTerrainTag(terrain));
                }
            }

            return new CourseDefinition(
                new CourseId(courseId),
                courseName,
                lengthMeters,
                domainSegments
            );
        }

        private static string ToTerrainTag(TerrainType terrain)
        {
            switch (terrain)
            {
                case TerrainType.Cave:
                    return "cave";
                case TerrainType.Ice:
                    return "ice";
                case TerrainType.Magma:
                    return "magma";
                case TerrainType.Grassland:
                default:
                    return "grassland";
            }
        }
    }
}
