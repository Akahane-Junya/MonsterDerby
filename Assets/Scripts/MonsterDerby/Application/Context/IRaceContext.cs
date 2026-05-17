using MonsterDerby.Application.UseCases;

namespace MonsterDerby.Application.Context
{
    /// <summary>
    /// Race機能に必要な依存を提供
    /// </summary>
    public interface IRaceContext
    {
        RunRaceUseCase RunRaceUseCase { get; }
    }
}