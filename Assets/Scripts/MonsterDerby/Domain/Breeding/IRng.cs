namespace MonsterDerby.Domain.Breeding
{
    /// <summary>
    /// 決定論・テスト容易性のための乱数インターフェース。
    /// </summary>
    public interface IRng
    {
        int NextInt(int minInclusive, int maxExclusive);
    }
}
