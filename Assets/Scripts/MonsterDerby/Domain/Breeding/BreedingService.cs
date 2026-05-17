namespace MonsterDerby.Domain.Breeding
{
    using System;
    using MonsterDerby.Domain.Monster;
    using MonsterDerby.Domain.SharedKernel;

    /// <summary>
    /// 交配（世代交代）の純粋ルール。
    /// - 現役個体は Application 層が破棄（アーカイブ化）する
    /// - Domain は「子を生成する」だけ
    /// </summary>
    public sealed class BreedingService
    {
        private readonly IGrowthBlendPolicy _growthBlendPolicy;
        private readonly IRng _rng;

        public BreedingService(IGrowthBlendPolicy growthBlendPolicy, IRng rng)
        {
            _growthBlendPolicy = growthBlendPolicy ?? throw new ArgumentNullException(nameof(growthBlendPolicy));
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        public MonsterInstance CreateChild(
            MonsterId childId,
            MonsterInstance current,
            BreedingPartner partner,
            string nickname,
            MonsterSkill[] inheritedMonsterSkills)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            if (partner == null) throw new ArgumentNullException(nameof(partner));
            if (inheritedMonsterSkills == null) throw new ArgumentNullException(nameof(inheritedMonsterSkills));

            var childSpeciesId = ResolveChildSpecies(current.SpeciesId, partner.SpeciesId);
            var childGrowth = _growthBlendPolicy.Blend(current.GrowthIncrements, partner.GrowthIncrements);

            return new MonsterInstance(
                childId,
                childSpeciesId,
                nickname,
                new Experience(0),
                childGrowth,
                inheritedMonsterSkills,
                new[] { current.MonsterId, partner.MonsterId });
        }

        private SpeciesId ResolveChildSpecies(SpeciesId a, SpeciesId b)
        {
            if (a.Equals(b)) return a;
            // 異種はどちらか（確率）
            return _rng.NextInt(0, 2) == 0 ? a : b;
        }
    }

    /// <summary>
    /// 交配屋の候補（相手）。
    /// </summary>
    public sealed class BreedingPartner
    {
        public MonsterId MonsterId { get; }
        public SpeciesId SpeciesId { get; }
        public GrowthIncrements GrowthIncrements { get; }

        public BreedingPartner(MonsterId monsterId, SpeciesId speciesId, GrowthIncrements growthIncrements)
        {
            MonsterId = monsterId;
            SpeciesId = speciesId;
            GrowthIncrements = growthIncrements ?? throw new ArgumentNullException(nameof(growthIncrements));
        }
    }
}
