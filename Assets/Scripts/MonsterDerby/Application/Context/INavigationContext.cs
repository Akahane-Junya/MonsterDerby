using MonsterDerby.Presentation.Navigation;

namespace MonsterDerby.Application.Context
{
    /// <summary>
    /// 画面遷移に必要な依存を提供
    /// </summary>
    public interface INavigationContext
    {
        ScreenNavigator Navigator { get; }
    }
}