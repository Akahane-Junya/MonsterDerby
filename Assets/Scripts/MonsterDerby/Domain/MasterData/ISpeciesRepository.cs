namespace MonsterDerby.Domain.MasterData
{
    using System.Collections.Generic;
    using MonsterDerby.Domain.SharedKernel;

    /// <summary>
    /// 種族マスタの取得インターフェース。
    /// 実体（ScriptableObject / JSON / Remote 等）は Infrastructure で実装する。
    /// </summary>
    public interface ISpeciesRepository
    {
        SpeciesDefinition GetSpeciesDefinition(SpeciesId speciesId);
        IReadOnlyList<SpeciesDefinition> GetAllSpeciesDefinitions();
    }
}
