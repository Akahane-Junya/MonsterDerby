namespace MonsterDerby.Domain.Monster
{
    using System;
    using MonsterDerby.Domain.SharedKernel;

    /// <summary>
    /// モンスターが所持するスキル1枠。
    /// - ID と解禁レベル（1〜10）を保持する
    /// </summary>
    public readonly struct MonsterSkill : IEquatable<MonsterSkill>
    {
        public SkillId Id { get; }
        public Level UnlockLevel { get; }

        public MonsterSkill(SkillId id, Level unlockLevel)
        {
            Id = id;
            if (unlockLevel.Value < 1 || unlockLevel.Value > GrowthIncrements.LevelCount)
                throw new ArgumentOutOfRangeException(nameof(unlockLevel),
                    $"解禁レベルは 1〜{GrowthIncrements.LevelCount} の範囲が必要です。実際: {unlockLevel.Value}");
            UnlockLevel = unlockLevel;
        }

        public bool Equals(MonsterSkill other) => Id.Equals(other.Id) && UnlockLevel.Value == other.UnlockLevel.Value;
        public override bool Equals(object obj) => obj is MonsterSkill other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Id.GetHashCode(), UnlockLevel.Value);
        public override string ToString() => $"{Id} (Lv{UnlockLevel})";
    }
}