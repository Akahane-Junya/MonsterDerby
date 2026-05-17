// 方針: Domain の型は「任意」を null で表現しない。
//       任意のコレクションは空配列（Array.Empty<T>()）で表現する。
//       公開コンストラクタは必須入力を検証し、不正なら即例外（Fail-Fast）。

namespace MonsterDerby.Domain.Race
{
    using MonsterDerby.Domain.SharedKernel;
    internal static class Sampling
    {
        public static RaceSample Capture(RaceState state, ImpactResolutionFrame[] impactResolutions)
        {
            var n = state.Runners.Length;

            var runners = new RunnerFrame[n];
            for (int i = 0; i < n; i++)
            {
                var r = state.Runners[i];

                var cooldowns = (r.Cooldowns.Count == 0) ? Empty<CooldownFrame>.Array : new CooldownFrame[r.Cooldowns.Count];
                for (int c = 0; c < r.Cooldowns.Count; c++)
                {
                    var cd = r.Cooldowns[c];
                    cooldowns[c] = new CooldownFrame(cd.SkillId, cd.RemainingSeconds);
                }

                var effects = (r.ActiveEffects.Count == 0) ? Empty<ActiveEffectFrame>.Array : new ActiveEffectFrame[r.ActiveEffects.Count];
                for (int e = 0; e < r.ActiveEffects.Count; e++)
                {
                    var ef = r.ActiveEffects[e];
                    effects[e] = new ActiveEffectFrame(ef.Effect.Kind, ef.Effect.Magnitude, ef.RemainingSeconds);
                }

                runners[i] = new RunnerFrame(
                    laneIndex: r.LaneIndex,
                    monsterId: r.Snapshot.MonsterId,
                    distanceMeters: r.DistanceMeters,
                    speedMetersPerSecond: r.SpeedMetersPerSecond,
                    remainingStamina: r.RemainingStamina,
                    hasFinished: r.HasFinished,
                    finishTimeSeconds: r.HasFinished ? r.FinishTimeSeconds : -1f,
                    cooldowns: cooldowns,
                    activeEffects: effects);
            }

            var projectiles = (state.Projectiles.Count == 0) ? Empty<ProjectileFrame>.Array : new ProjectileFrame[state.Projectiles.Count];
            for (int p = 0; p < state.Projectiles.Count; p++)
            {
                var pr = state.Projectiles[p];
                projectiles[p] = new ProjectileFrame(
                    projectileId: pr.ProjectileId,
                    skillId: pr.SkillId,
                    attackerLaneIndex: pr.AttackerLaneIndex,
                    targetLaneIndex: pr.TargetLaneIndex,
                    launchTimeSeconds: pr.LaunchTimeSeconds,
                    impactTimeSeconds: pr.ImpactTimeSeconds);
            }

            return new RaceSample(
                timeSeconds: state.TimeSeconds,
                tick: state.Tick,
                runners: runners,
                projectiles: projectiles,
                impactResolutions: impactResolutions ?? Empty<ImpactResolutionFrame>.Array);
        }
    }
}
