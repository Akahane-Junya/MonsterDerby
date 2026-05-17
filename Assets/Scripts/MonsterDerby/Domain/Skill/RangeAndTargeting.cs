namespace MonsterDerby.Domain.Skill
{
    using MonsterDerby.Domain.SharedKernel;
    public sealed class RangeDefinition
    {
        public RangeDirection Direction { get; }
        public float Meters { get; }

        public RangeDefinition(RangeDirection direction, float meters)
        {
            Guard.NonNegative(meters, nameof(meters));
            Direction = direction;
            Meters = meters;
        }
    }

    public sealed class TargetingDefinition
    {
        public TargetingMode Mode { get; }
        public int MaximumTargets { get; } // 0 means unlimited / n/a

        public TargetingDefinition(TargetingMode mode, int maximumTargets)
        {
            if (maximumTargets < 0) throw new System.ArgumentOutOfRangeException(nameof(maximumTargets));
            Mode = mode;
            MaximumTargets = maximumTargets;
        }
    }
}