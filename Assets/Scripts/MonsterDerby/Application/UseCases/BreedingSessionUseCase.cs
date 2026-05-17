using System;
using MonsterDerby.Application.Game;
using MonsterDerby.Domain.Monster;

namespace MonsterDerby.Application.UseCases
{
    public sealed class BreedingSessionUseCase
    {
        private readonly GameSession _gameSession;

        public BreedingSessionUseCase(GameSession gameSession)
        {
            _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        }

        public bool TryGetCurrentMonster(out MonsterInstance currentMonster)
        {
            currentMonster = null;
            if (!_gameSession.HasWorld || _gameSession.State == null || _gameSession.State.CurrentMonster == null)
                return false;

            currentMonster = _gameSession.State.CurrentMonster;
            return true;
        }

        public void ApplySelectedChild(MonsterInstance selectedChild)
        {
            if (selectedChild == null)
                throw new ArgumentNullException(nameof(selectedChild));
            if (!_gameSession.HasWorld || _gameSession.State == null)
                return;

            var currentWorld = _gameSession.State;
            var updatedWorld = currentWorld.With(currentWorld.Money, selectedChild);
            _gameSession.Apply(_ => updatedWorld);
        }
    }
}