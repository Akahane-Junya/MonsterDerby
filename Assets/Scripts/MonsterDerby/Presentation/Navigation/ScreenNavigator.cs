using System;
using UnityEngine;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Presentation.Screens.Title;
using MonsterDerby.Presentation.Screens.Home;
using MonsterDerby.Presentation.Screens.Training;
using MonsterDerby.Presentation.Screens.Shop;
using MonsterDerby.Presentation.Screens.Breeding;
using MonsterDerby.Presentation.Screens.Race;
using MonsterDerby.Presentation.Screens.Status;
using MonsterDerby.Presentation.Screens.Awards;
using MonsterDerby.Presentation.Screens.Settings;
using MonsterDerby.Presentation.UI;

namespace MonsterDerby.Presentation.Navigation
{
    public sealed class ScreenNavigator : MonoBehaviour
    {
        [Header("Screen Views")]
        [SerializeField] private TitleView _titleView;
        [SerializeField] private HomeView _homeView;
        [SerializeField] private TrainingView _trainingView;
        [SerializeField] private ShopView _shopView;
        [SerializeField] private BreedingView _breedingView;
        [SerializeField] private RaceView _raceView;
        [SerializeField] private StatusView _statusView;
        [SerializeField] private AwardsView _awardsView;
        [SerializeField] private SettingsView _settingsView;

        private IPresenterFactory _presenterFactory;
        private IScreenPresenter _currentPresenter;
        private ScreenId _currentScreenId;

        public void Initialize(IPresenterFactory presenterFactory)
        {
            _presenterFactory = presenterFactory;

            EnsureButtonFeedback(_titleView);
            EnsureButtonFeedback(_homeView);
            EnsureButtonFeedback(_trainingView);
            EnsureButtonFeedback(_shopView);
            EnsureButtonFeedback(_breedingView);
            EnsureButtonFeedback(_raceView);
            EnsureButtonFeedback(_statusView);
            EnsureButtonFeedback(_awardsView);
            EnsureButtonFeedback(_settingsView);
        }

        public void NavigateTo(ScreenId screenId)
        {
            if (_presenterFactory == null)
            {
                Debug.LogError("ScreenNavigator: Initialize() が呼ばれていません。");
                return;
            }

            // 失敗時に元画面へ戻せるよう、現状を保持
            var previousPresenter = _currentPresenter;
            var previousScreenId = _currentScreenId;

            IScreenPresenter nextPresenter;
            try
            {
                // ここで例外が出ることがある（未実装 ScreenId 等）
                nextPresenter = _presenterFactory.Create(screenId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ScreenNavigator: 画面遷移に失敗しました。target={screenId}\n{ex}");
                // 何も変えずに戻る（現画面を維持）
                return;
            }

            // ここまで来たら遷移成功が確定なので、旧画面を隠して新画面に切り替える
            if (previousPresenter != null)
            {
                try { previousPresenter.Hide(); }
                catch (Exception ex)
                {
                    Debug.LogError($"ScreenNavigator: 旧Presenter.Hide() で例外。screen={previousScreenId}\n{ex}");
                }
            }

            HideAllViews();

            _currentPresenter = nextPresenter;
            _currentScreenId = screenId;

            try
            {
                // 対応するViewを表示してPresenterにバインド
                var view = GetViewForScreen(screenId);
                if (view == null)
                {
                    Debug.LogWarning($"ScreenNavigator: Screen {screenId} is not wired to a View. Fallback to previous screen.");
                    _currentPresenter = previousPresenter;
                    _currentScreenId = previousScreenId;
                    ShowPreviousView(previousScreenId);
                    return;
                }

                SetActive(view, true);
                _currentPresenter.BindView(view);

                // Presenterを表示
                _currentPresenter.Show();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ScreenNavigator: View バインドまたは Show() で例外。target={screenId}\n{ex}");

                // ロールバック（旧画面を復旧）
                _currentPresenter = previousPresenter;
                _currentScreenId = previousScreenId;
                HideAllViews();
                ShowPreviousView(previousScreenId);
            }
        }

        private void HideAllViews()
        {
            SetActive(_titleView, false);
            SetActive(_homeView, false);
            SetActive(_trainingView, false);
            SetActive(_shopView, false);
            SetActive(_breedingView, false);
            SetActive(_raceView, false);
            SetActive(_statusView, false);
            SetActive(_awardsView, false);
            SetActive(_settingsView, false);
        }

        private void ShowPreviousView(ScreenId screenId)
        {
            var previousView = GetViewForScreen(screenId);
            if (previousView != null)
            {
                SetActive(previousView, true);
            }
        }

        private IScreenView GetViewForScreen(ScreenId screenId)
        {
            switch (screenId)
            {
                case ScreenId.Title:
                    return _titleView;
                case ScreenId.Home:
                    return _homeView;
                case ScreenId.Training:
                    return _trainingView;
                case ScreenId.Shop:
                    return _shopView;
                case ScreenId.Breeding:
                    return _breedingView;
                case ScreenId.Race:
                    return _raceView;
                case ScreenId.Status:
                    return _statusView;
                case ScreenId.Awards:
                    return _awardsView;
                case ScreenId.Settings:
                    return _settingsView;
                default:
                    return null;
            }
        }

        private static void SetActive(IScreenView view, bool isActive)
        {
            if (view == null)
            {
                return;
            }

            var behaviour = view as MonoBehaviour;
            if (behaviour != null)
            {
                behaviour.gameObject.SetActive(isActive);
            }
        }

        private static void EnsureButtonFeedback(IScreenView view)
        {
            var behaviour = view as MonoBehaviour;
            if (behaviour == null)
            {
                return;
            }

            if (behaviour.GetComponent<GlobalButtonFeedback>() != null)
            {
                return;
            }

            behaviour.gameObject.AddComponent<GlobalButtonFeedback>();
        }
    }
}