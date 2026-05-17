using System;
using System.Linq;
using UnityEngine;
using MonsterDerby.Domain.MasterData;
using MonsterDerby.Domain.SharedKernel;

namespace MonsterDerby.Infrastructure.MasterData
{
    [CreateAssetMenu(menuName = "MonsterDerby/MasterData/Species")]
    public sealed class SpeciesDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string speciesId;
        public string speciesName;
        public string visualId;

        [Header("Growth")]
        public SpeciesGrowthIncrementEntrySO[] speciesGrowthIncrements;

        [Header("Learnset")]
        public LearnsetEntrySO[] speciesLearnsetEntries;

        private void Reset()
        {
            EnsureSerializedDefaults();
        }

        private void OnValidate()
        {
            EnsureSerializedDefaults();
        }

        public SpeciesDefinition ToDomain()
        {
            if (speciesGrowthIncrements == null)
                throw new System.InvalidOperationException($"SpeciesDefinitionSO '{name}': speciesGrowthIncrements が未設定です。空配列を明示設定してください。");
            if (speciesLearnsetEntries == null)
                throw new System.InvalidOperationException($"SpeciesDefinitionSO '{name}': speciesLearnsetEntries が未設定です。空配列を明示設定してください。");
            if (speciesGrowthIncrements.Length != MonsterDerby.Domain.Monster.GrowthIncrements.LevelCount)
                throw new System.InvalidOperationException($"SpeciesDefinitionSO '{name}': speciesGrowthIncrements は {MonsterDerby.Domain.Monster.GrowthIncrements.LevelCount} 件必要です。実際: {speciesGrowthIncrements.Length}");

            var increments = speciesGrowthIncrements
                .Select(x => x.ToMonsterStats())
                .ToArray();

            var speciesLearnset = speciesLearnsetEntries
                .Select(x => x.ToDomain())
                .ToArray();

            return new SpeciesDefinition(
                new SpeciesId(speciesId),
                speciesName,
                visualId,
                new MonsterDerby.Domain.Monster.GrowthIncrements(increments),
                new SpeciesLearnset(speciesLearnset));
        }

        private void EnsureSerializedDefaults()
        {
            if (speciesGrowthIncrements == null)
            {
                speciesGrowthIncrements = new SpeciesGrowthIncrementEntrySO[MonsterDerby.Domain.Monster.GrowthIncrements.LevelCount];
            }
            else if (speciesGrowthIncrements.Length != MonsterDerby.Domain.Monster.GrowthIncrements.LevelCount)
            {
                var resized = new SpeciesGrowthIncrementEntrySO[MonsterDerby.Domain.Monster.GrowthIncrements.LevelCount];
                var copyLength = Math.Min(speciesGrowthIncrements.Length, resized.Length);
                Array.Copy(speciesGrowthIncrements, resized, copyLength);
                speciesGrowthIncrements = resized;
            }

            if (speciesLearnsetEntries == null)
                speciesLearnsetEntries = Array.Empty<LearnsetEntrySO>();
        }
    }

    [Serializable]
    public struct SpeciesGrowthIncrementEntrySO
    {
        [Min(0)] public int topSpeed;
        [Min(0)] public int accel;
        [Min(0)] public int stamina;

        public MonsterStats ToMonsterStats()
            => new MonsterStats(topSpeed, accel, stamina);
    }

    [Serializable]
    public struct LearnsetEntrySO
    {
        public string skillId;
        [Min(1)] public int unlockLevel;

        public LearnsetEntry ToDomain()
            => new LearnsetEntry(new SkillId(skillId), new Level(unlockLevel));
    }
}
