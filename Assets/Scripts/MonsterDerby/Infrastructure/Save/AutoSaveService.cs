using UnityEngine;
using MonsterDerby.Application.Game;
using MonsterDerby.Infrastructure.Save;

namespace MonsterDerby.Infrastructure.Save
{
    /// <summary>
    /// World更新を監視し、自動保存を行う。
    /// デバウンス処理で連続保存を防ぐ。
    /// </summary>
    public sealed class AutoSaveService
    {
        private readonly ISaveRepository repository;
        private readonly GameSession session;

        private bool dirty;
        private float nextSaveTime;

        private const float DebounceSeconds = 1.0f;

        public AutoSaveService(ISaveRepository repository, GameSession session)
        {
            this.repository = repository;
            this.session = session;

            session.OnWorldChanged += _ =>
            {
                dirty = true;
                nextSaveTime = Time.unscaledTime + DebounceSeconds;
            };
        }

        /// <summary>
        /// Updateから呼ぶ
        /// </summary>
        public void Tick()
        {
            if (!dirty) return;
            if (Time.unscaledTime < nextSaveTime) return;

            Flush();
        }

        /// <summary>
        /// 即保存
        /// </summary>
        public void Flush()
        {
            if (!dirty) return;
            if (!session.HasWorld) return;

            repository.Save(session.State);
            dirty = false;
        }
    }
}
