namespace MonsterDerby.Domain.Course
{
    using MonsterDerby.Domain.SharedKernel;

    /// <summary>
    /// Course 定義を取得するための抽象化。
    /// 実体（ScriptableObject / JSON / Remote 等）は Infrastructure で実装する。
    /// </summary>
    public interface ICourseRepository
    {
        CourseDefinition GetCourseDefinition(CourseId courseId);
    }
}
