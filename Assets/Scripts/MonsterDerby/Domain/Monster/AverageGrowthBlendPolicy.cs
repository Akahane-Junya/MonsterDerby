namespace MonsterDerby.Domain.Monster
{
    using System;
    using System.Collections.Generic;
    using MonsterDerby.Domain.SharedKernel;

    /// <summary>
    /// 子の成長増分 = 両親の増分の単純平均。
    /// 刻み制限は行わない（要件どおり）。
    /// </summary>
    public sealed class AverageGrowthBlendPolicy : IGrowthBlendPolicy
    {
        public GrowthIncrements Blend(GrowthIncrements a, GrowthIncrements b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));

            int count = GrowthIncrements.LevelCount;
            var list = new List<MonsterStats>(count);
            for (int i = 0; i < count; i++)
            {
                var aInc = a.Entries[i];
                var bInc = b.Entries[i];

                var top = (Fixed.FromInt(aInc.TopSpeed) + Fixed.FromInt(bInc.TopSpeed)) / 2;
                var accel = (Fixed.FromInt(aInc.Accel) + Fixed.FromInt(bInc.Accel)) / 2;
                var stamina = (Fixed.FromInt(aInc.Stamina) + Fixed.FromInt(bInc.Stamina)) / 2;

                var inc = new MonsterStats(
                    top.ToIntBankersRounded(),
                    accel.ToIntBankersRounded(),
                    stamina.ToIntBankersRounded());
                list.Add(inc);
            }

            return new GrowthIncrements(list);
        }
    }
}
