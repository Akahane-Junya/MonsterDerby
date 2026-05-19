namespace MonsterDerby.Domain.Catalog
{
    /// <summary>
    /// 図鑑アンロック段階
    /// </summary>
    public enum CatalogUnlockStage
    {
        None = 0,        // 未発見
        Encountered = 1, // 出会った
        Raised = 2       // 育てた（全情報解放）
    }
}
