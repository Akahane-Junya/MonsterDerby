namespace MonsterDerby.Domain.Skill
{
    using MonsterDerby.Domain.SharedKernel;
    public abstract class TriggerDefinition
    {
        public abstract string Kind { get; }
    }

    public sealed class CooldownTrigger : TriggerDefinition
    {
        public override string Kind => "Cooldown";
        public float CooldownSeconds { get; }

        public CooldownTrigger(float cooldownSeconds)
        {
            Guard.Positive(cooldownSeconds, nameof(cooldownSeconds));
            CooldownSeconds = cooldownSeconds;
        }
    }

    public sealed class ConditionTrigger : TriggerDefinition
    {
        public override string Kind => "Condition";
        public string ConditionTag { get; }
        public float Threshold { get; }

        public ConditionTrigger(string conditionTag, float threshold)
        {
            ConditionTag = Guard.NotNullOrEmpty(conditionTag, nameof(conditionTag));
            Threshold = threshold;
        }
    }
}