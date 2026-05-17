using UnityEngine;

namespace MonsterDerby.Application.Game
{
    /// <summary>
    /// Inspector から差し替え可能な GameSession ファクトリ資産。
    /// </summary>
    public abstract class GameSessionFactoryAsset : ScriptableObject, IGameSessionFactory
    {
        public abstract GameSession Create();
    }
}
