namespace MonsterDerby.Domain.Monster
{
    using System;
    using System.Collections.Generic;
    using MonsterDerby.Domain.SharedKernel;

    /// <summary>
    /// 各レベルでのステータス上昇量テーブル（Lv1の基礎値を含む）。
    /// - 3ステータス × 10レベル分のリスト
    /// - 現在ステータスの計算ロジックを内包する
    /// </summary>
    public sealed class GrowthIncrements
    {
        public const int LevelCount = 10;

        private readonly IReadOnlyList<MonsterStats> _entries;

        public GrowthIncrements(IReadOnlyList<MonsterStats> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            if (entries.Count != LevelCount)
                throw new ArgumentException($"entries は {LevelCount} 件必要です（Lv1〜Lv10）。実際: {entries.Count}", nameof(entries));

            _entries = entries;
        }

        public IReadOnlyList<MonsterStats> Entries => _entries;

        /// <summary>
        /// 現在の経験値に対応するステータスを返す。
        /// </summary>
        public MonsterStats CalculateStats(Experience experience)
        {
            int level = Math.Min(experience.Value / 100 + 1, LevelCount);

            int totalTopSpeed = 0;
            int totalAccel = 0;
            int totalStamina = 0;

            for (int lv = 1; lv <= level; lv++)
            {
                var entry = _entries[lv - 1];
                totalTopSpeed += entry.TopSpeed;
                totalAccel += entry.Accel;
                totalStamina += entry.Stamina;
            }

            return new MonsterStats(
                Clamp(totalTopSpeed),
                Clamp(totalAccel),
                Clamp(totalStamina));
        }

        private static int Clamp(int value) => Math.Min(100, Math.Max(0, value));
    }
}
