using System;
using System.Collections.Generic;
using MonsterDerby.Domain.World;
using MonsterDerby.Domain.Monster;
using MonsterDerby.Domain.Records;


namespace MonsterDerby.Application.Game
{
    /// <summary>
    /// ゲーム進行の中核。
    /// Domain(WorldState)を保持し、
    /// 更新・通知を管理する。
    /// </summary>
    public sealed class GameSession
    {
        public WorldState State { get; private set; }

        /// <summary>
        /// Worldが更新されたときに発火
        /// </summary>
        public event Action<WorldState> OnWorldChanged;

        public bool HasWorld => State != null;

        /// <summary>
        /// 新規ゲーム開始
        /// </summary>
        public void StartNew(int initialMoney, MonsterInstance currentMonster)
        {
            StartNew(initialMoney, currentMonster, Array.Empty<RaceAwardState>());
        }

        public void StartNew(int initialMoney, MonsterInstance currentMonster, IReadOnlyList<RaceAwardState> awardEntries)
        {
            State = new WorldState(initialMoney, currentMonster, awardEntries);
            NotifyChanged();
        }

        /// <summary>
        /// セーブデータから復元
        /// </summary>
        public void Restore(WorldState restored)
        {
            State = restored;
            NotifyChanged();
        }

        /// <summary>
        /// 状態更新（Reducerパターン）
        /// </summary>
        public void Apply(Func<WorldState, WorldState> reducer)
        {
            if (State == null) return;

            State = reducer(State);
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            OnWorldChanged?.Invoke(State);
        }
    }
}
