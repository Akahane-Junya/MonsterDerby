using System;
using System.Collections.Generic;
using System.Linq;
using MonsterDerby.Domain.MasterData;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Infrastructure.MasterData;

namespace MonsterDerby.Infrastructure.Repositories
{
    /// <summary>
    /// ScriptableObjectベースのSpeciesRepository実装
    /// </summary>
    public sealed class ScriptableObjectSpeciesRepository : ISpeciesRepository
    {
        private readonly Dictionary<SpeciesId, SpeciesDefinition> _cache;
        private readonly List<SpeciesDefinition> _ordered;

        public ScriptableObjectSpeciesRepository(MasterDataCatalog catalog)
        {
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            _ordered = catalog.Species
                .Where(so => so != null)
                .Select(so => so.ToDomain())
                .ToList();

            _cache = _ordered.ToDictionary(def => def.SpeciesId);

            if (_cache.Count == 0)
                UnityEngine.Debug.LogWarning("ScriptableObjectSpeciesRepository: 種族が1つも登録されていません。");
        }

        public SpeciesDefinition GetSpeciesDefinition(SpeciesId speciesId)
        {
            if (!_cache.TryGetValue(speciesId, out var definition))
                throw new KeyNotFoundException($"Species not found: {speciesId}");

            return definition;
        }

        public IReadOnlyList<SpeciesDefinition> GetAllSpeciesDefinitions()
        {
            return _ordered;
        }
    }
}
