using MonsterDerby.Domain.SharedKernel;

namespace MonsterDerby.Presentation.Navigation
{
    /// <summary>
    /// Presenterを生成するFactoryのインターフェース
    /// </summary>
    public interface IPresenterFactory
    {
        IScreenPresenter Create(ScreenId id);
    }
}