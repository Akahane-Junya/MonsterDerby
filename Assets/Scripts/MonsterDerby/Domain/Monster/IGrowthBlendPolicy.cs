namespace MonsterDerby.Domain.Monster
{
    /// <summary>
    /// 両親の成長テーブルから子の成長テーブルを作る。
    /// </summary>
    public interface IGrowthBlendPolicy
    {
        GrowthIncrements Blend(GrowthIncrements a, GrowthIncrements b);
    }
}
