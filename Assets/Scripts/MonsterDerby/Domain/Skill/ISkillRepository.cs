namespace MonsterDerby.Domain.Skill
{
    using MonsterDerby.Domain.SharedKernel;

    /// <summary>
    /// Skill 定義を取得するための抽象化。
    /// 実体（ScriptableObject / JSON / Remote 等）は Infrastructure で実装する。
    /// </summary>
    public interface ISkillRepository
    {
        SkillDefinition GetSkillDefinition(SkillId skillId);
    }
}
