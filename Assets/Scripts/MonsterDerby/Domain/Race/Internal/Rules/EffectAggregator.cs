namespace MonsterDerby.Domain.Race
{
    using MonsterDerby.Domain.Skill;

    internal sealed class EffectAggregator : IEffectSink
    {
        public float SpeedMultiplier { get; private set; }
        public float AccelerationMultiplier { get; private set; }
        public float SpeedAdditive { get; private set; }
        public float SlowMultiplier { get; private set; }
        public float StaminaDrainMultiplier { get; private set; }

        public EffectAggregator()
        {
            Reset();
        }

        public void Reset()
        {
            SpeedMultiplier = 1f;
            AccelerationMultiplier = 1f;
            SpeedAdditive = 0f;
            SlowMultiplier = 1f;
            StaminaDrainMultiplier = 1f;
        }

        public void Apply(IEffect effect) => effect.Apply(this);

        public void MultiplySpeed(float multiplier) => SpeedMultiplier *= multiplier;

        public void MultiplyAcceleration(float multiplier) => AccelerationMultiplier *= multiplier;

        public void AddSpeed(float metersPerSecond) => SpeedAdditive += metersPerSecond;

        public void MultiplySlow(float multiplier) => SlowMultiplier *= multiplier;

        public void MultiplyStaminaDrain(float multiplier) => StaminaDrainMultiplier *= multiplier;
    }
}
