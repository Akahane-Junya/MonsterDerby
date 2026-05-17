using System;
using MonsterDerby.Application.Game;

namespace MonsterDerby.Infrastructure.Save
{
    /// <summary>
    /// セーブデータから GameSession を復元するファクトリ。
    /// </summary>
    public sealed class SaveDataGameSessionFactory : IGameSessionFactory
    {
        private readonly ISaveRepository _saveRepository;

        public SaveDataGameSessionFactory(ISaveRepository saveRepository)
        {
            _saveRepository = saveRepository ?? throw new ArgumentNullException(nameof(saveRepository));
        }

        public GameSession Create()
        {
            if (!_saveRepository.TryLoad(out var restoredWorld) || restoredWorld == null)
                throw new InvalidOperationException("セーブデータの読み込みに失敗しました。save.json を作成してから起動してください。");

            if (restoredWorld.CurrentMonster == null)
                throw new InvalidOperationException("セーブデータに CurrentMonster が存在しません。");

            var session = new GameSession();
            session.Restore(restoredWorld);
            return session;
        }
    }
}
