using MonsterDerby.Application.UseCases;

namespace MonsterDerby.Application.Context
{
    /// <summary>
    /// Status機能に必要な依存を提供。
    /// </summary>
    public interface IStatusContext
    {
        GetCurrentMonsterStatusUseCase GetCurrentMonsterStatusUseCase { get; }
    }
}
