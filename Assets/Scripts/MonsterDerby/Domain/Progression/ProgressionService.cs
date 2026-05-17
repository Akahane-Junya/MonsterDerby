namespace MonsterDerby.Domain.Progression
{
    using System;
    using MonsterDerby.Domain.Monster;
    using MonsterDerby.Domain.SharedKernel;

    /// <summary>
    /// 経験値付与を扱う。
    /// レベルは Experience から自動計算されるため、レベルテーブル不要。
    /// </summary>
    public sealed class ProgressionService
    {
        public MonsterInstance AddExperience(MonsterInstance monster, Experience delta)
        {
            if (monster == null) throw new ArgumentNullException(nameof(monster));

            var newExp = monster.Experience + delta;
            return monster.WithExperience(newExp);
        }
    }
}
