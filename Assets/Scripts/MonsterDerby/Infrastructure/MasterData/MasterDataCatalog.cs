using UnityEngine;
using System.Collections.Generic;

namespace MonsterDerby.Infrastructure.MasterData
{
    /// <summary>
    /// すべてのマスターデータを保持するカタログ
    /// GameRootから参照される
    /// </summary>
    [CreateAssetMenu(fileName = "MasterDataCatalog", menuName = "MonsterDerby/MasterData/Catalog")]
    public sealed class MasterDataCatalog : ScriptableObject
    {
        [SerializeField] private List<RaceDefinitionSO> _races = new();
        [SerializeField] private List<CourseDefinitionSO> _courses = new();
        [SerializeField] private List<SkillDefinitionSO> _skills = new();
        [SerializeField] private List<SpeciesDefinitionSO> _species = new();
        [SerializeField] private List<MonsterVisualDefinitionSO> _monsterVisuals = new();

        public IReadOnlyList<RaceDefinitionSO> Races => _races;
        public IReadOnlyList<CourseDefinitionSO> Courses => _courses;
        public IReadOnlyList<SkillDefinitionSO> Skills => _skills;
        public IReadOnlyList<SpeciesDefinitionSO> Species => _species;
        public IReadOnlyList<MonsterVisualDefinitionSO> MonsterVisuals => _monsterVisuals;

        private void OnValidate()
        {
            // 重複チェック
            var courseIds = new HashSet<string>();
            var raceIds = new HashSet<string>();

            foreach (var race in _races)
            {
                if (race == null) continue;
                var id = race.raceId;
                if (string.IsNullOrEmpty(id)) continue;
                if (!raceIds.Add(id))
                    Debug.LogWarning($"MasterDataCatalog: 重複した Race ID '{id}' が存在します。");
            }

            foreach (var course in _courses)
            {
                if (course == null) continue;
                var id = course.courseId;
                if (string.IsNullOrEmpty(id)) continue;
                if (!courseIds.Add(id))
                    Debug.LogWarning($"MasterDataCatalog: 重複した Course ID '{id}' が存在します。");
            }

            var skillIds = new HashSet<string>();
            foreach (var skill in _skills)
            {
                if (skill == null) continue;
                var id = skill.skillId;
                if (string.IsNullOrEmpty(id)) continue;
                if (!skillIds.Add(id))
                    Debug.LogWarning($"MasterDataCatalog: 重複した Skill ID '{id}' が存在します。");
            }

            var speciesIds = new HashSet<string>();
            foreach (var species in _species)
            {
                if (species == null) continue;
                var id = species.speciesId;
                if (string.IsNullOrEmpty(id)) continue;
                if (!speciesIds.Add(id))
                    Debug.LogWarning($"MasterDataCatalog: 重複した Species ID '{id}' が存在します。");
            }

            var visualIds = new HashSet<string>();
            foreach (var visual in _monsterVisuals)
            {
                if (visual == null) continue;
                var id = visual.visualId;
                if (string.IsNullOrEmpty(id)) continue;
                if (!visualIds.Add(id))
                    Debug.LogWarning($"MasterDataCatalog: 重複した Visual ID '{id}' が存在します。");
            }
        }
    }
}