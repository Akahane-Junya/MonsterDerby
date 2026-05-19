using System;
using MonsterDerby.Application.Context;
using MonsterDerby.Application.Game;
using MonsterDerby.Domain.Course;
using MonsterDerby.Domain.MasterData;
using MonsterDerby.Domain.Skill;
using MonsterDerby.Infrastructure.MasterData;
using MonsterDerby.Infrastructure.Repositories;
using MonsterDerby.Presentation.Navigation;

namespace MonsterDerby.Application
{
    /// <summary>
    /// アプリケーション全体の依存関係のルート
    /// 基盤Repositoryを保持し、機能別Contextを生成する
    /// </summary>

    using MonsterDerby.Infrastructure.Save;
    using MonsterDerby.Domain.Monster;

    public sealed class DependencyRoot
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ScriptableObjectSkillRepository _skillSORepository;
        private readonly ISpeciesRepository _speciesRepository;
        private readonly ScriptableObjectMonsterVisualRepository _monsterVisualRepository;
        private readonly GameSession _gameSession;
        private readonly ScreenNavigator _navigator;
        private readonly MasterDataCatalog _masterDataCatalog;
        private readonly CatalogUnlockRepositoryAsset _catalogUnlockRepository;
        private readonly MonsterDefinitionRepositoryAsset _monsterDefinitionRepository;

        public DependencyRoot(
            ICourseRepository courseRepository,
            ISkillRepository skillRepository,
            ISpeciesRepository speciesRepository,
            ScriptableObjectMonsterVisualRepository monsterVisualRepository,
            GameSession gameSession,
            ScreenNavigator navigator,
            MasterDataCatalog masterDataCatalog,
            CatalogUnlockRepositoryAsset catalogUnlockRepository,
            MonsterDefinitionRepositoryAsset monsterDefinitionRepository)
        {
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _skillRepository = skillRepository ?? throw new ArgumentNullException(nameof(skillRepository));
            _skillSORepository = skillRepository as ScriptableObjectSkillRepository;
            _speciesRepository = speciesRepository ?? throw new ArgumentNullException(nameof(speciesRepository));
            _monsterVisualRepository = monsterVisualRepository ?? throw new ArgumentNullException(nameof(monsterVisualRepository));
            _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
            _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
            _masterDataCatalog = masterDataCatalog ?? throw new ArgumentNullException(nameof(masterDataCatalog));
            _catalogUnlockRepository = catalogUnlockRepository ?? throw new ArgumentNullException(nameof(catalogUnlockRepository));
            _monsterDefinitionRepository = monsterDefinitionRepository ?? throw new ArgumentNullException(nameof(monsterDefinitionRepository));
        }
        public CatalogUnlockRepositoryAsset GetCatalogUnlockRepository()
        {
            return _catalogUnlockRepository;
        }

        public MonsterDefinitionRepositoryAsset GetMonsterDefinitionRepository()
        {
            return _monsterDefinitionRepository;
        }

        /// <summary>
        /// Race機能用のContextを生成
        /// </summary>
        public IRaceContext CreateRaceContext()
        {
            return new RaceContext(_courseRepository, _skillRepository);
        }

        /// <summary>
        /// Navigation用のContextを生成
        /// </summary>
        public INavigationContext CreateNavigationContext()
        {
            return new NavigationContext(_navigator);
        }

        public IStatusContext CreateStatusContext()
        {
            return new StatusContext(_gameSession, _speciesRepository, _skillRepository);
        }

        public IBreedingContext CreateBreedingContext()
        {
            return new BreedingContext(_gameSession, _speciesRepository);
        }

        public ISpeciesRepository GetSpeciesRepository()
        {
            return _speciesRepository;
        }

        public ScriptableObjectMonsterVisualRepository GetMonsterVisualRepository()
        {
            return _monsterVisualRepository;
        }

        public ScriptableObjectSkillRepository GetSkillSORepository()
        {
            return _skillSORepository;
        }

        public GameSession GetGameSession()
        {
            return _gameSession;
        }

        public MasterDataCatalog GetMasterDataCatalog()
        {
            return _masterDataCatalog;
        }

        // 将来追加される機能
        // public ITrainingContext CreateTrainingContext() { ... }
        // public IShopContext CreateShopContext() { ... }
    }
}