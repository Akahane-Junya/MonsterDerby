using MonsterDerby.Domain.Race;

namespace MonsterDerby.Presentation.Animation.Race
{
    /// <summary>
    /// レース中モーション判定の抽象。
    /// </summary>
    public interface IRaceMotionPolicy
    {
        bool ShouldRun(RunnerFrame frame);
        bool ShouldPlaySkillOverlay(ProjectileFrame projectile);
        bool ShouldPlayDamageOverlay(ImpactResolutionFrame impact);
    }
}