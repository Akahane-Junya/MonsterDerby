using MonsterDerby.Application.UseCases;

namespace MonsterDerby.Application.Context
{
    /// <summary>
    /// Breeding機能に必要な依存を提供。
    /// </summary>
    public interface IBreedingContext
    {
        PlanBreedingCandidatesUseCase PlanBreedingCandidatesUseCase { get; }
        BreedingSessionUseCase BreedingSessionUseCase { get; }
    }
}