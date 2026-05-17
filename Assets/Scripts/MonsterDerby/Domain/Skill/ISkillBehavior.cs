namespace MonsterDerby.Domain.Skill
{

    /// <summary>
    /// スキルの実行時ふるまい。
    /// まだタイプ設計が未確定のため、インターフェースのみ用意する。
    /// 実際のレース処理にどう統合するかは今後確定させる。
    /// </summary>
    public interface ISkillBehavior
    {
        /// <summary>
        /// レース中のTickで呼ばれる想定。
        /// 現時点では契約を確定しない（Application/Infraでアダプト可能にする）。
        /// </summary>
        void OnTick(object context);
    }
}
