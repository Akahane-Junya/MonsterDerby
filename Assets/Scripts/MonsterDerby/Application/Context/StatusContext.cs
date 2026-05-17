using System;
using MonsterDerby.Application.Game;
using MonsterDerby.Application.UseCases;
using MonsterDerby.Domain.MasterData;
using MonsterDerby.Domain.Skill;

namespace MonsterDerby.Application.Context
{
    /// <summary>
    /// Status機能専用のContext実装。
    /// </summary>
    internal sealed class StatusContext : IStatusContext
    {
        public GetCurrentMonsterStatusUseCase GetCurrentMonsterStatusUseCase { get; }

        public StatusContext(GameSession gameSession, ISpeciesRepository speciesRepository, ISkillRepository skillRepository)
        {
            if (gameSession == null)
                throw new ArgumentNullException(nameof(gameSession));
            if (speciesRepository == null)
                throw new ArgumentNullException(nameof(speciesRepository));
            if (skillRepository == null)
                throw new ArgumentNullException(nameof(skillRepository));

            GetCurrentMonsterStatusUseCase = new GetCurrentMonsterStatusUseCase(gameSession, speciesRepository, skillRepository);
        }
    }
}
