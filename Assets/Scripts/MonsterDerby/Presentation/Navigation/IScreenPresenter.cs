namespace MonsterDerby.Presentation.Navigation
{
    /// <summary>
    /// すべての画面Presenterが実装するインターフェース
    /// </summary>
    public interface IScreenPresenter
    {
        void BindView(IScreenView view);
        void Show();
        void Hide();
    }
}