using UnityEngine;
using MonsterDerby.Application;
using MonsterDerby.Application.Game;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Infrastructure.MasterData;
using MonsterDerby.Infrastructure.Repositories;
using MonsterDerby.Infrastructure.Save;
using MonsterDerby.Domain.Monster;
using MonsterDerby.Presentation.Navigation;

namespace MonsterDerby.Presentation
{
    /// <summary>
    /// アプリケーションのエントリーポイント
    /// すべての依存関係を構築する CompositionRoot
    /// </summary>
    public sealed class GameRoot : MonoBehaviour
    {
        [Header("MasterData")]
        [SerializeField] private MasterDataCatalog _masterDataCatalog;

        [Header("Game Session")]
        [SerializeField] private GameSessionFactoryAsset _gameSessionFactoryAsset;

        [Header("Navigation")]
        [SerializeField] private ScreenNavigator _screenNavigator;


        [Header("Catalog Unlock Repository")]
        [SerializeField] private CatalogUnlockRepositoryAsset _catalogUnlockRepository;

        [Header("Monster Definition Repository")]
        [SerializeField] private MonsterDefinitionRepositoryAsset _monsterDefinitionRepository;

        private DependencyRoot _dependencies;

        private void Awake()
        {
            // 必須参照チェック
            if (_masterDataCatalog == null)
                throw new System.InvalidOperationException("MasterDataCatalog が Inspector に設定されていません。");
            if (_gameSessionFactoryAsset == null)
                throw new System.InvalidOperationException("GameSessionFactoryAsset が Inspector に設定されていません。");
            if (_screenNavigator == null)
                throw new System.InvalidOperationException("ScreenNavigator が Inspector に設定されていません。");

            // Repository 生成
            var courseRepository = new ScriptableObjectCourseRepository(_masterDataCatalog);
            var skillRepository = new ScriptableObjectSkillRepository(_masterDataCatalog);
            var speciesRepository = new ScriptableObjectSpeciesRepository(_masterDataCatalog);
            var monsterVisualRepository = new ScriptableObjectMonsterVisualRepository(_masterDataCatalog, speciesRepository);
            var gameSession = _gameSessionFactoryAsset.Create();
            {
                var m = gameSession.State.CurrentMonster;
                var stats = m.CurrentStats;
                Debug.Log($"[GameRoot] Session loaded:\n" +
                          $"  Money           : {gameSession.State.Money}\n" +
                          $"  MonsterId       : {m.MonsterId.Value}\n" +
                          $"  SpeciesId       : {m.SpeciesId.Value}\n" +
                          $"  Nickname        : {m.Nickname}\n" +
                          $"  Level           : {m.Level.Value}\n" +
                          $"  Experience      : {m.Experience.Value}\n" +
                          $"  TopSpeed        : {stats.TopSpeed}\n" +
                          $"  Accel           : {stats.Accel}\n" +
                          $"  Stamina         : {stats.Stamina}\n" +
                          $"  Skills      : {m.MonsterSkills?.Length ?? 0}");
            }

            // DependencyRoot 生成

            if (_catalogUnlockRepository == null)
                throw new System.InvalidOperationException("CatalogUnlockRepositoryAsset が Inspector に設定されていません。");
            if (_monsterDefinitionRepository == null)
                throw new System.InvalidOperationException("MonsterDefinitionRepositoryAsset が Inspector に設定されていません。");

            _dependencies = new DependencyRoot(
                courseRepository,
                skillRepository,
                speciesRepository,
                monsterVisualRepository,
                gameSession,
                _screenNavigator,
                _masterDataCatalog,
                _catalogUnlockRepository,
                _monsterDefinitionRepository
            );

            // Factory 生成
            var presenterFactory = new PresenterFactory(_dependencies);
            _screenNavigator.Initialize(presenterFactory);

            // 初期画面表示
            // Title から開始（Home 直行にしたい場合は ScreenId.Home に変更）
            _screenNavigator.NavigateTo(ScreenId.Title);
        }

    }
}