namespace MonsterDerby.Application.Game
{
    /// <summary>
    /// GameSession の生成方法を抽象化するファクトリ。
    /// </summary>
    public interface IGameSessionFactory
    {
        GameSession Create();
    }
}
