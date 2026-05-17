using System.Collections.Generic;
using MonsterDerby.Domain.Race;
using MonsterDerby.Domain.SharedKernel;

namespace MonsterDerby.Presentation.Animation.Race
{
    /// <summary>
    /// レースのサンプルをモーション命令に変換して各Runnerへ配信する。
    /// </summary>
    public sealed class RaceMotionOrchestrator : IRaceMotionOrchestrator
    {
        private readonly IRaceMotionPolicy _policy;
        private readonly Dictionary<MonsterId, IRaceRunnerAnimationController> _controllers = new();
        private readonly Dictionary<int, MonsterId> _laneToMonsterId = new();
        private readonly HashSet<long> _seenProjectileIds = new();

        public RaceMotionOrchestrator(IRaceMotionPolicy policy)
        {
            _policy = policy;
        }

        public void RegisterRunner(MonsterId monsterId, IRaceRunnerAnimationController controller)
        {
            _controllers[monsterId] = controller;
        }

        public void BindLane(int laneIndex, MonsterId monsterId)
        {
            _laneToMonsterId[laneIndex] = monsterId;
        }

        public void ApplySample(RaceSample sample, float raceTimeSeconds)
        {
            foreach (var runnerFrame in sample.Runners)
            {
                if (_controllers.TryGetValue(runnerFrame.MonsterId, out var controller))
                {
                    controller.SetRunState(_policy.ShouldRun(runnerFrame), raceTimeSeconds);
                }
            }

            foreach (var projectile in sample.Projectiles)
            {
                if (_seenProjectileIds.Contains(projectile.ProjectileId))
                {
                    continue;
                }

                _seenProjectileIds.Add(projectile.ProjectileId);

                if (_policy.ShouldPlaySkillOverlay(projectile)
                    && _laneToMonsterId.TryGetValue(projectile.AttackerLaneIndex, out var attackerId)
                    && _controllers.TryGetValue(attackerId, out var attackerController))
                {
                    attackerController.PlaySkillOverlay(raceTimeSeconds);
                }
            }

            foreach (var impact in sample.ImpactResolutions)
            {
                if (_policy.ShouldPlayDamageOverlay(impact)
                    && _laneToMonsterId.TryGetValue(impact.TargetLaneIndex, out var targetId)
                    && _controllers.TryGetValue(targetId, out var targetController))
                {
                    targetController.PlayDamageOverlay(raceTimeSeconds);
                }
            }
        }

        public void Update(float raceTimeSeconds)
        {
            foreach (var controller in _controllers.Values)
            {
                controller.Tick(raceTimeSeconds);
            }
        }

        public void Clear()
        {
            _controllers.Clear();
            _laneToMonsterId.Clear();
            _seenProjectileIds.Clear();
        }
    }
}