namespace MonsterDerby.Domain.MasterData
{
    using System;
    using MonsterDerby.Domain.SharedKernel;

    public sealed class SpeciesLearnset
    {
        public LearnsetEntry[] Entries { get; }

        public SpeciesLearnset(LearnsetEntry[] entries)
        {
            Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }
    }

    public readonly struct LearnsetEntry
    {
        public SkillId SkillId { get; }
        public Level UnlockLevel { get; }

        public LearnsetEntry(SkillId skillId, Level unlockLevel)
        {
            SkillId = skillId;
            UnlockLevel = unlockLevel;
        }
    }
}
