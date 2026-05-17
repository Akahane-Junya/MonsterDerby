namespace MonsterDerby.Domain.Records
{
    using System;
    using MonsterDerby.Domain.SharedKernel;

    /// <summary>
    /// 消滅（引退）したモンスターの最小スナップショット。
    /// MonsterInstance は保持しない（現役のみ）。
    /// </summary>
    public sealed class MonsterArchiveEntry
    {
        public MonsterId MonsterId { get; }
        public SpeciesId SpeciesId { get; }
        public string Nickname { get; }
        public MonsterId[] ParentMonsterIds { get; }

        public MonsterArchiveEntry(MonsterId monsterId, SpeciesId speciesId, string nickname, MonsterId[] parentMonsterIds)
        {
            MonsterId = monsterId;
            SpeciesId = speciesId;
            Nickname = nickname ?? string.Empty;
            ParentMonsterIds = parentMonsterIds ?? throw new ArgumentNullException(nameof(parentMonsterIds));
        }
    }
}
