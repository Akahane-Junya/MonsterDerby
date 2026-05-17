using MonsterDerby.Domain.Race;

namespace MonsterDerby.Presentation.Animation.Race
{
    /// <summary>
    /// レース中モーション遷移の判定ルールを集約する。
    /// </summary>
    public sealed class RaceMotionPolicy : IRaceMotionPolicy
    {
        private const float DefaultRunSpeedThreshold = 0.05f;
        private readonly float _runSpeedThreshold;

        public RaceMotionPolicy()
            : this(DefaultRunSpeedThreshold)
        {
        }

        public RaceMotionPolicy(float runSpeedThreshold)
        {
            _runSpeedThreshold = runSpeedThreshold;
        }

        public bool ShouldRun(RunnerFrame frame)
        {
            return frame.SpeedMetersPerSecond > _runSpeedThreshold && !frame.HasFinished;
        }

        public bool ShouldPlaySkillOverlay(ProjectileFrame projectile)
        {
            return true;
        }

        public bool ShouldPlayDamageOverlay(ImpactResolutionFrame impact)
        {
            return impact.Kind == ImpactResolutionKind.Hit;
        }
    }
}