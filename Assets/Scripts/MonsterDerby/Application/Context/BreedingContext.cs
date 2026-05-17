using System;
using MonsterDerby.Application.Game;
using MonsterDerby.Application.UseCases;
using MonsterDerby.Domain.MasterData;

namespace MonsterDerby.Application.Context
{
    internal sealed class BreedingContext : IBreedingContext
    {
        public PlanBreedingCandidatesUseCase PlanBreedingCandidatesUseCase { get; }
        public BreedingSessionUseCase BreedingSessionUseCase { get; }

        public BreedingContext(GameSession gameSession, ISpeciesRepository speciesRepository)
        {
            if (gameSession == null)
                throw new ArgumentNullException(nameof(gameSession));
            if (speciesRepository == null)
                throw new ArgumentNullException(nameof(speciesRepository));

            PlanBreedingCandidatesUseCase = new PlanBreedingCandidatesUseCase(speciesRepository);
            BreedingSessionUseCase = new BreedingSessionUseCase(gameSession);
        }
    }
}