namespace MonsterDerby.Domain.Race
{
    using MonsterDerby.Domain.SharedKernel;

    /// <summary>
    /// レースシミュレーション用のステータス（float）。
    /// レース中の変動を考慮し、1～1000の小数値で管理する。
    /// </summary>
    public sealed class RaceStats
    {
        public float MaximumSpeed { get; }
        public float Acceleration { get; }
        public float MaximumStamina { get; }

        public RaceStats(float maximumSpeed, float acceleration, float maximumStamina)
        {
            Guard.InRangeInclusive(maximumSpeed, 1f, 1000f, nameof(maximumSpeed));
            Guard.InRangeInclusive(acceleration, 1f, 1000f, nameof(acceleration));
            Guard.InRangeInclusive(maximumStamina, 1f, 1000f, nameof(maximumStamina));

            MaximumSpeed = maximumSpeed;
            Acceleration = acceleration;
            MaximumStamina = maximumStamina;
        }
    }
}
