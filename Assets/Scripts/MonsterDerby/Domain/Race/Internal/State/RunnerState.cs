namespace MonsterDerby.Domain.Race
{
    using MonsterDerby.Domain.SharedKernel;
    using MonsterDerby.Domain.Skill;
    using System.Collections.Generic;

    internal sealed class RunnerState
    {
        public int LaneIndex { get; }
        public MonsterSnapshot Snapshot { get; }

        public float DistanceMeters { get; set; }
        public float SpeedMetersPerSecond { get; set; }
        public float RemainingStamina { get; set; }

        public List<CooldownState> Cooldowns { get; }
        public List<ActiveEffectState> ActiveEffects { get; }

        public bool HasFinished { get; set; }
        public bool HasFinishTime => _finishTimeSeconds.HasValue;
        public float FinishTimeSeconds => _finishTimeSeconds ?? -1f;

        private float? _finishTimeSeconds;

        public RunnerState(int laneIndex, MonsterSnapshot snapshot)
        {
            LaneIndex = laneIndex;
            Snapshot = Guard.NotNull(snapshot, nameof(snapshot));
            DistanceMeters = 0f;
            SpeedMetersPerSecond = 0f;
            RemainingStamina = snapshot.RaceStats.MaximumStamina;
            Cooldowns = new List<CooldownState>(capacity: 8);
            ActiveEffects = new List<ActiveEffectState>(capacity: 8);
            HasFinished = false;
            _finishTimeSeconds = null;
        }

        public void MarkFinished(float timeSeconds)
        {
            if (HasFinished) return;
            HasFinished = true;
            _finishTimeSeconds = timeSeconds;
        }
    }

    internal sealed class CooldownState
    {
        public SkillId SkillId { get; }
        public float RemainingSeconds { get; set; }

        public CooldownState(SkillId skillId, float remainingSeconds)
        {
            Guard.NonNegative(remainingSeconds, nameof(remainingSeconds));
            SkillId = skillId;
            RemainingSeconds = remainingSeconds;
        }
    }

    internal sealed class ActiveEffectState
    {
        public IEffect Effect { get; }
        public float RemainingSeconds { get; set; }

        public ActiveEffectState(IEffect effect, float remainingSeconds)
        {
            Guard.NonNegative(remainingSeconds, nameof(remainingSeconds));
            Effect = Guard.NotNull(effect, nameof(effect));
            RemainingSeconds = remainingSeconds;
        }
    }
}
