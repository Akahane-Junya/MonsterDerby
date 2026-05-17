using System;
using MonsterDerby.Application.Context;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Infrastructure.Repositories;
using MonsterDerby.Presentation.Navigation;

namespace MonsterDerby.Presentation.Screens.Home
{
    /// <summary>
    /// Home画面のPresenter
    /// Viewのイベントを受け取り、画面遷移を行う
    /// </summary>
    public sealed class HomePresenter : IScreenPresenter
    {
        private readonly INavigationContext _navigationContext;
        private readonly ScriptableObjectMonsterVisualRepository _monsterVisualRepository;
        private HomeView _view;

        public HomePresenter(INavigationContext navigationContext, ScriptableObjectMonsterVisualRepository monsterVisualRepository)
        {
            _navigationContext = navigationContext ?? throw new ArgumentNullException(nameof(navigationContext));
            _monsterVisualRepository = monsterVisualRepository ?? throw new ArgumentNullException(nameof(monsterVisualRepository));
        }

        /// <summary>
        /// Viewを設定してイベント購読
        /// </summary>
        public void BindView(IScreenView view)
        {
            _view = view as HomeView ?? throw new ArgumentException("HomeView が必要です。", nameof(view));

            // Viewのイベントを購読
            _view.OnTrainingClicked += HandleTrainingClicked;
            _view.OnShopClicked += HandleShopClicked;
            _view.OnBreedingClicked += HandleBreedingClicked;
            _view.OnRaceClicked += HandleRaceClicked;
            _view.OnStatusClicked += HandleStatusClicked;
            _view.OnAwardsClicked += HandleAwardsClicked;
            _view.OnSettingsClicked += HandleSettingsClicked;
        }

        public void Show()
        {
            UnityEngine.Debug.Log("HomePresenter.Show()");
        }

        public void Hide()
        {
            if (_view != null)
            {
                // イベント購読解除
                _view.OnTrainingClicked -= HandleTrainingClicked;
                _view.OnShopClicked -= HandleShopClicked;
                _view.OnBreedingClicked -= HandleBreedingClicked;
                _view.OnRaceClicked -= HandleRaceClicked;
                _view.OnStatusClicked -= HandleStatusClicked;
                _view.OnAwardsClicked -= HandleAwardsClicked;
                _view.OnSettingsClicked -= HandleSettingsClicked;
            }

            UnityEngine.Debug.Log("HomePresenter.Hide()");
        }

        // ===== イベントハンドラ =====

        private void HandleTrainingClicked()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Training);
        }

        private void HandleShopClicked()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Shop);
        }

        private void HandleBreedingClicked()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Breeding);
        }

        private void HandleRaceClicked()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Race);
        }

        private void HandleStatusClicked()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Status);
        }

        private void HandleAwardsClicked()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Awards);
        }

        private void HandleSettingsClicked()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Settings);
        }
    }
}