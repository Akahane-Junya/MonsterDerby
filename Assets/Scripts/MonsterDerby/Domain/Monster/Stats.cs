namespace MonsterDerby.Domain.Monster
{
    using MonsterDerby.Domain.SharedKernel;
    public sealed class Stats
    {
        public float MaximumSpeed { get; }
        public float Acceleration { get; }
        public float MaximumStamina { get; }

        public Stats(float maximumSpeed, float acceleration, float maximumStamina)
        {
            Guard.NonNegative(maximumSpeed, nameof(maximumSpeed));
            Guard.NonNegative(acceleration, nameof(acceleration));
            Guard.NonNegative(maximumStamina, nameof(maximumStamina));

            MaximumSpeed = maximumSpeed;
            Acceleration = acceleration;
            MaximumStamina = maximumStamina;
        }
    }
}