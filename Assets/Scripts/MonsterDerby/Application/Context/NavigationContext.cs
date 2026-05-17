using System;
using MonsterDerby.Presentation.Navigation;

namespace MonsterDerby.Application.Context
{
    /// <summary>
    /// Navigation機能専用のContext実装
    /// </summary>
    internal sealed class NavigationContext : INavigationContext
    {
        private readonly ScreenNavigator _navigator;

        public NavigationContext(ScreenNavigator navigator)
        {
            _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        }

        public ScreenNavigator Navigator => _navigator;
    }
}