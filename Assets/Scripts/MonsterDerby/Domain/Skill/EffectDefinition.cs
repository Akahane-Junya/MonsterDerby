namespace MonsterDerby.Domain.Skill
{
    using System;
    using MonsterDerby.Domain.SharedKernel;

    public enum EffectTargetSide
    {
        Target,
        Attacker,
        Both
    }

    public sealed class EffectDefinition
    {
        public IEffect Effect { get; }
        public float DurationSeconds { get; } // 0 means instant
        public EffectTargetSide TargetSide { get; }

        public bool IsTimed => DurationSeconds > 0f;

        public EffectDefinition(IEffect effect, float durationSeconds, EffectTargetSide targetSide = EffectTargetSide.Target)
        {
            Guard.NonNegative(durationSeconds, nameof(durationSeconds));
            Effect = effect ?? throw new ArgumentNullException(nameof(effect));
            DurationSeconds = durationSeconds;
            TargetSide = targetSide;
        }
    }
}
