using System;
using MonsterDerby.Application;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Presentation.Screens.Awards;
using MonsterDerby.Presentation.Screens.Breeding;
using MonsterDerby.Presentation.Screens.Home;
using MonsterDerby.Presentation.Screens.Race;
using MonsterDerby.Presentation.Screens.Settings;
using MonsterDerby.Presentation.Screens.Shop;
using MonsterDerby.Presentation.Screens.Status;
using MonsterDerby.Presentation.Screens.Training;
using MonsterDerby.Presentation.Screens.Title; // 追加

namespace MonsterDerby.Presentation.Navigation
{
    /// <summary>
    /// 各画面のPresenterを生成するFactory
    /// DependencyRootから必要な機能別Contextを取得して注入する
    /// </summary>
    public sealed class PresenterFactory : IPresenterFactory
    {
        private readonly DependencyRoot _dependencies;

        public PresenterFactory(DependencyRoot dependencies)
        {
            _dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
        }

        public IScreenPresenter Create(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.Title:
                    return new TitlePresenter(_dependencies.CreateNavigationContext());

                case ScreenId.Home:
                    return new HomePresenter(
                        _dependencies.CreateNavigationContext(),
                        _dependencies.GetMonsterVisualRepository()
                    );

                case ScreenId.Race:
                    return new RacePresenter(
                        _dependencies.CreateRaceContext(),
                        _dependencies.CreateNavigationContext(),
                        _dependencies.GetMonsterVisualRepository(),
                        _dependencies.GetMasterDataCatalog(),
                        _dependencies.GetGameSession()
                    );

                case ScreenId.Training:
                    return new TrainingPresenter(_dependencies.CreateNavigationContext());

                case ScreenId.Shop:
                    return new ShopPresenter(
                        _dependencies.CreateNavigationContext(),
                        _dependencies.GetGameSession(),
                        _dependencies.GetSkillSORepository());

                case ScreenId.Breeding:
                    return new BreedingPresenter(
                        _dependencies.CreateNavigationContext(),
                        _dependencies.CreateBreedingContext());

                case ScreenId.Status:
                    return new StatusPresenter(
                        _dependencies.CreateNavigationContext(),
                        _dependencies.CreateStatusContext(),
                        _dependencies.GetMonsterVisualRepository(),
                        _dependencies.GetSkillSORepository()
                    );

                case ScreenId.Awards:
                    return new AwardsPresenter(
                        _dependencies.CreateNavigationContext(),
                        _dependencies.GetGameSession(),
                        _dependencies.GetMasterDataCatalog());

                case ScreenId.Settings:
                    return new SettingsPresenter(_dependencies.CreateNavigationContext());

                case ScreenId.Catalog:
                    return new MonsterDerby.Presentation.Screens.Catalog.CatalogPresenter();

                default:
                    throw new ArgumentException($"未知の ScreenId です: {id}", nameof(id));
            }
        }
    }
}