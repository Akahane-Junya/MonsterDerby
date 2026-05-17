namespace MonsterDerby.Domain.Race
{
    using System;

    /// <summary>
    /// シミュレーション再現性のための単純な決定的 RNG。
    /// </summary>
    internal sealed class DeterministicRng
    {
        private ulong _state;

        public long Seed { get; }
        public long Counter { get; private set; }

        public DeterministicRng(long seed)
        {
            Seed = seed;
            // SplitMix64-ish seeding
            _state = (ulong)seed + 0x9E3779B97F4A7C15UL;
        }

        public float NextFloat01()
        {
            Counter++;
            // SplitMix64 のステップ
            ulong z = (_state += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            z = z ^ (z >> 31);

            // 上位24bitを [0,1) に変換
            uint top24 = (uint)(z >> 40);
            return top24 / (float)(1 << 24);
        }

        public int NextInt(int exclusiveMax)
        {
            if (exclusiveMax <= 0) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            return (int)(NextFloat01() * exclusiveMax);
        }
    }
}