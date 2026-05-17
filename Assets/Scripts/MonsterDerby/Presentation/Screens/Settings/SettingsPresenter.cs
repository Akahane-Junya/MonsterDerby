using System;
using MonsterDerby.Application.Context;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Presentation.Navigation;

namespace MonsterDerby.Presentation.Screens.Settings
{
    public sealed class SettingsPresenter : IScreenPresenter
    {
        private readonly INavigationContext _navigationContext;
        private SettingsView _view;

        public SettingsPresenter(INavigationContext navigationContext)
        {
            _navigationContext = navigationContext ?? throw new ArgumentNullException(nameof(navigationContext));
        }

        public void BindView(IScreenView view)
        {
            _view = view as SettingsView ?? throw new ArgumentException("SettingsView が必要です。", nameof(view));
            _view.OnBackClicked += HandleBackClicked;
        }

        public void Show()
        {
            UnityEngine.Debug.Log("SettingsPresenter.Show()");
        }

        public void Hide()
        {
            if (_view != null)
            {
                _view.OnBackClicked -= HandleBackClicked;
            }
        }

        private void HandleBackClicked()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Home);
        }
    }
}