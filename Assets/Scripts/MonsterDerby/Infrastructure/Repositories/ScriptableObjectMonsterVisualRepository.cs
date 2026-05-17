using System;
using System.Collections.Generic;
using System.Linq;
using MonsterDerby.Domain.MasterData;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Infrastructure.MasterData;

namespace MonsterDerby.Infrastructure.Repositories
{
    /// <summary>
    /// SpeciesId -> VisualId -> VisualAsset を解決するための読み取り専用リポジトリ。
    /// </summary>
    public sealed class ScriptableObjectMonsterVisualRepository
    {
        private readonly ISpeciesRepository _speciesRepository;
        private readonly Dictionary<string, MonsterVisualDefinitionSO> _visualById;

        public ScriptableObjectMonsterVisualRepository(MasterDataCatalog catalog, ISpeciesRepository speciesRepository)
        {
            _speciesRepository = speciesRepository ?? throw new ArgumentNullException(nameof(speciesRepository));

            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            _visualById = catalog.MonsterVisuals
                .Where(x => x != null && !string.IsNullOrEmpty(x.visualId))
                .ToDictionary(x => x.visualId, StringComparer.Ordinal);
        }

        public MonsterVisualDefinitionSO GetByVisualId(string visualId)
        {
            if (string.IsNullOrEmpty(visualId))
                throw new ArgumentException("visualId is required.", nameof(visualId));

            if (!_visualById.TryGetValue(visualId, out var visual))
                throw new KeyNotFoundException($"Monster visual not found: {visualId}");

            return visual;
        }

        public MonsterVisualDefinitionSO GetBySpeciesId(SpeciesId speciesId)
        {
            var species = _speciesRepository.GetSpeciesDefinition(speciesId);
            return GetByVisualId(species.VisualId);
        }

    }
}
