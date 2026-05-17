namespace MonsterDerby.Domain.SharedKernel
{
    /// <summary>
    /// モンスターが保持する3ステ（整数）。
    /// 仕様として 0～100 の範囲を保持する。
    /// </summary>
    public readonly struct MonsterStats
    {
        public int TopSpeed { get; }
        public int Accel { get; }
        public int Stamina { get; }

        public MonsterStats(int topSpeed, int accel, int stamina)
        {
            Guard.InRangeInclusive(topSpeed, 0, 100, nameof(topSpeed));
            Guard.InRangeInclusive(accel, 0, 100, nameof(accel));
            Guard.InRangeInclusive(stamina, 0, 100, nameof(stamina));

            TopSpeed = topSpeed;
            Accel = accel;
            Stamina = stamina;
        }

        public override string ToString() => $"Top={TopSpeed}, Accel={Accel}, Sta={Stamina}";
    }
}
