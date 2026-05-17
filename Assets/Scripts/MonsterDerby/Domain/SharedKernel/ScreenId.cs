namespace MonsterDerby.Domain.SharedKernel
{
    /// <summary>
    /// 画面の論理的識別子。
    /// Domain/Application層の概念であり、
    /// 具体的なUXMLやUnity資産には依存しない。
    /// </summary>
    public enum ScreenId
    {
        Title,
        Home,
        Training,
        Shop,
        Breeding,
        Race,
        Status,
        Awards,
        Settings,
    }
}