using System;
using System.Collections.Generic;
using MonsterDerby.Domain.MasterData;
using MonsterDerby.Domain.Monster;
using MonsterDerby.Domain.SharedKernel;

namespace MonsterDerby.Application.UseCases
{
    public sealed class PlanBreedingCandidatesUseCase
    {
        private const int CandidateCount = 3;

        private readonly ISpeciesRepository _speciesRepository;

        public PlanBreedingCandidatesUseCase(ISpeciesRepository speciesRepository)
        {
            _speciesRepository = speciesRepository ?? throw new ArgumentNullException(nameof(speciesRepository));
        }

        public IReadOnlyList<BreedingBaseOption> GetBaseOptions(SpeciesId currentSpeciesId)
        {
            var result = new List<BreedingBaseOption>(CandidateCount);

            var allSpecies = _speciesRepository.GetAllSpeciesDefinitions();
            for (int i = 0; i < allSpecies.Count && result.Count < CandidateCount; i++)
            {
                var definition = allSpecies[i];
                var speciesId = definition.SpeciesId;
                if (speciesId == currentSpeciesId)
                    continue;

                result.Add(new BreedingBaseOption(
                    speciesId,
                    definition.Name,
                    definition.SpeciesGrowthIncrements,
                    ToDefaultSkills(definition.SpeciesLearnset)));
            }

            return result;
        }

        public IReadOnlyList<BreedingEggCandidate> BuildEggCandidates(MonsterInstance current, BreedingBaseOption partner)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));

            var partnerMonsterId = new MonsterId($"wild_{partner.SpeciesId.Value}");
            var partnerNickname = string.IsNullOrWhiteSpace(partner.SpeciesName) ? partner.SpeciesId.Value : partner.SpeciesName;

            var result = new List<BreedingEggCandidate>(CandidateCount);

            var candidateCurrent = new MonsterInstance(
                CreateChildMonsterId("A"),
                current.SpeciesId,
                $"{current.Nickname}の卵A",
                new Experience(0),
                current.GrowthIncrements,
                CloneSkills(current.MonsterSkills),
                new[] { current.MonsterId, partnerMonsterId });
            result.Add(new BreedingEggCandidate("親1寄り", candidateCurrent, current.Nickname, partnerNickname));

            var candidatePartner = new MonsterInstance(
                CreateChildMonsterId("B"),
                partner.SpeciesId,
                $"{partnerNickname}の卵B",
                new Experience(0),
                partner.Growth,
                CloneSkills(partner.DefaultSkills),
                new[] { current.MonsterId, partnerMonsterId });
            result.Add(new BreedingEggCandidate("原種寄り", candidatePartner, current.Nickname, partnerNickname));

            var candidateMix = new MonsterInstance(
                CreateChildMonsterId("C"),
                partner.SpeciesId,
                "ミックス卵C",
                new Experience(0),
                BlendGrowth(current.GrowthIncrements, partner.Growth),
                BlendSkills(current.MonsterSkills, partner.DefaultSkills),
                new[] { current.MonsterId, partnerMonsterId });
            result.Add(new BreedingEggCandidate("ミックス", candidateMix, current.Nickname, partnerNickname));

            return result;
        }

        private static MonsterSkill[] ToDefaultSkills(SpeciesLearnset learnset)
        {
            if (learnset == null || learnset.Entries == null || learnset.Entries.Length == 0)
                return Array.Empty<MonsterSkill>();

            var count = Math.Min(4, learnset.Entries.Length);
            var result = new MonsterSkill[count];
            for (int i = 0; i < count; i++)
            {
                var entry = learnset.Entries[i];
                result[i] = new MonsterSkill(entry.SkillId, entry.UnlockLevel);
            }

            return result;
        }

        private static GrowthIncrements BlendGrowth(GrowthIncrements a, GrowthIncrements b)
        {
            var list = new MonsterStats[GrowthIncrements.LevelCount];
            for (int i = 0; i < GrowthIncrements.LevelCount; i++)
            {
                var ea = a.Entries[i];
                var eb = b.Entries[i];
                list[i] = new MonsterStats(
                    (ea.TopSpeed + eb.TopSpeed) / 2,
                    (ea.Accel + eb.Accel) / 2,
                    (ea.Stamina + eb.Stamina) / 2);
            }

            return new GrowthIncrements(list);
        }

        private static MonsterSkill[] BlendSkills(MonsterSkill[] a, MonsterSkill[] b)
        {
            var result = new List<MonsterSkill>(4);
            AppendDistinct(result, a);
            AppendDistinct(result, b);
            return result.ToArray();
        }

        private static MonsterSkill[] CloneSkills(MonsterSkill[] source)
        {
            if (source == null || source.Length == 0)
                return Array.Empty<MonsterSkill>();

            var copy = new MonsterSkill[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }

        private static void AppendDistinct(List<MonsterSkill> target, MonsterSkill[] source)
        {
            if (source == null)
                return;

            for (int i = 0; i < source.Length; i++)
            {
                if (target.Count >= 4)
                    break;

                var exists = false;
                for (int j = 0; j < target.Count; j++)
                {
                    if (target[j].Id == source[i].Id)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                    target.Add(source[i]);
            }
        }

        private static MonsterId CreateChildMonsterId(string suffix)
        {
            return new MonsterId($"child_{suffix}_{Guid.NewGuid():N}");
        }
    }

    public sealed class BreedingBaseOption
    {
        public SpeciesId SpeciesId { get; }
        public string SpeciesName { get; }
        public GrowthIncrements Growth { get; }
        public MonsterSkill[] DefaultSkills { get; }

        public BreedingBaseOption(SpeciesId speciesId, string speciesName, GrowthIncrements growth, MonsterSkill[] defaultSkills)
        {
            SpeciesId = speciesId;
            SpeciesName = speciesName ?? string.Empty;
            Growth = growth ?? throw new ArgumentNullException(nameof(growth));
            DefaultSkills = defaultSkills ?? Array.Empty<MonsterSkill>();
        }
    }

    public sealed class BreedingEggCandidate
    {
        public string KindLabel { get; }
        public MonsterInstance Child { get; }
        public string ParentAName { get; }
        public string ParentBName { get; }

        public BreedingEggCandidate(string kindLabel, MonsterInstance child, string parentAName, string parentBName)
        {
            KindLabel = kindLabel ?? string.Empty;
            Child = child ?? throw new ArgumentNullException(nameof(child));
            ParentAName = parentAName ?? string.Empty;
            ParentBName = parentBName ?? string.Empty;
        }
    }
}