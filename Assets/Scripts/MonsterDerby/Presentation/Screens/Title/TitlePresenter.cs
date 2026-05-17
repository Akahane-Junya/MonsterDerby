using System;
using MonsterDerby.Application.Context;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Presentation.Navigation;

namespace MonsterDerby.Presentation.Screens.Title
{
    /// <summary>
    /// Title画面のPresenter
    /// </summary>
    public sealed class TitlePresenter : IScreenPresenter
    {
        private readonly INavigationContext _navigationContext;
        private TitleView _view;

        public TitlePresenter(INavigationContext navigationContext)
        {
            _navigationContext = navigationContext ?? throw new ArgumentNullException(nameof(navigationContext));
        }

        public void BindView(IScreenView view)
        {
            _view = view as TitleView ?? throw new ArgumentException("TitleView が必要です。", nameof(view));
            _view.OnStartClicked += HandleStartClicked;
        }

        public void Show()
        {
            UnityEngine.Debug.Log("TitlePresenter.Show()");
        }

        public void Hide()
        {
            if (_view != null)
            {
                _view.OnStartClicked -= HandleStartClicked;
            }
            UnityEngine.Debug.Log("TitlePresenter.Hide()");
        }

        private void HandleStartClicked()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Home);
        }
    }
}