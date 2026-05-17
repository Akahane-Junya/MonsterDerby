namespace MonsterDerby.Domain.SharedKernel
{
    public readonly struct Level
    {
        public int Value { get; }

        public Level(int value)
        {
            Guard.Positive(value, nameof(value));
            Value = value;
        }

        public static implicit operator int(Level level) => level.Value;
        public override string ToString() => Value.ToString();
    }

    public readonly struct Experience
    {
        public int Value { get; }

        public Experience(int value)
        {
            Guard.NonNegative(value, nameof(value));
            Value = value;
        }

        /// <summary>
        /// 経験値からレベルを計算する。経験値100ごとにレベル上昇。Lv1〜10。
        /// </summary>
        public Level ToLevel()
        {
            int lv = System.Math.Min(Value / 100 + 1, 10);
            return new Level(lv);
        }

        public static Experience operator +(Experience a, Experience b)
            => new Experience(checked(a.Value + b.Value));

        public static implicit operator int(Experience exp) => exp.Value;
        public override string ToString() => Value.ToString();
    }
}
