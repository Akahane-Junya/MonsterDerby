using System;
using MonsterDerby.Application.Context;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Presentation.Navigation;

namespace MonsterDerby.Presentation.Screens.Training
{
    public sealed class TrainingPresenter : IScreenPresenter
    {
        private readonly INavigationContext _navigationContext;
        private TrainingView _view;

        public TrainingPresenter(INavigationContext navigationContext)
        {
            _navigationContext = navigationContext ?? throw new ArgumentNullException(nameof(navigationContext));
        }

        public void BindView(IScreenView view)
        {
            _view = view as TrainingView ?? throw new ArgumentException("TrainingView が必要です。", nameof(view));
            _view.OnBackClicked += HandleBackClicked;
        }

        public void Show()
        {
            UnityEngine.Debug.Log("TrainingPresenter.Show()");
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