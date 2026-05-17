// 目的:
//  - セーブデータの構造を Domain として保持する
//  - RaceDomain へは「MonsterSnapshot」を Application 層で組み立てて渡す（依存逆流を防ぐ）

namespace MonsterDerby.Domain.Monster
{
    using System;
    using MonsterDerby.Domain.SharedKernel;

    /// <summary>
    /// 現役の育成個体（1体）。
    /// 交配すると現個体は消滅するが、記録（Records）は別ドメインで保持する。
    /// </summary>
    public sealed class MonsterInstance
    {
        public MonsterId MonsterId { get; }
        public SpeciesId SpeciesId { get; }
        public string Nickname { get; }
        public Experience Experience { get; }

        /// <summary>
        /// 経験値から一意に計算されるレベル（Lv1〜10、経験値100ごとに上昇）。
        /// </summary>
        public Level Level => Experience.ToLevel();

        /// <summary>
        /// 交配後も親が消滅するため、成長テーブルは個体が保持する。
        /// </summary>
        public GrowthIncrements GrowthIncrements { get; }

        /// <summary>
        /// 現在レベルに対応するステータス（growthIncrements から都度計算）。
        /// </summary>
        public MonsterStats CurrentStats => GrowthIncrements.CalculateStats(Experience);

        /// <summary>
        /// 所持スキル（0〜4個）。各スキルは ID と解禁レベルを持つ。
        /// </summary>
        public MonsterSkill[] MonsterSkills { get; }

        public MonsterId[] ParentMonsterIds { get; }

        public MonsterInstance(
            MonsterId monsterId,
            SpeciesId speciesId,
            string nickname,
            Experience experience,
            GrowthIncrements growthIncrements,
            MonsterSkill[] monsterSkills,
            MonsterId[] parentMonsterIds)
        {
            MonsterId = monsterId;
            SpeciesId = speciesId;
            Nickname = nickname ?? string.Empty;
            Experience = experience;
            GrowthIncrements = growthIncrements ?? throw new ArgumentNullException(nameof(growthIncrements));

            if (monsterSkills == null) throw new ArgumentNullException(nameof(monsterSkills));
            if (monsterSkills.Length > 4) throw new ArgumentException("所持スキルは最大4個です。", nameof(monsterSkills));
            MonsterSkills = monsterSkills;

            ParentMonsterIds = parentMonsterIds ?? throw new ArgumentNullException(nameof(parentMonsterIds));
        }

        public MonsterInstance WithExperience(Experience experience)
        {
            return new MonsterInstance(
                MonsterId,
                SpeciesId,
                Nickname,
                experience,
                GrowthIncrements,
                MonsterSkills,
                ParentMonsterIds);
        }
    }
}
