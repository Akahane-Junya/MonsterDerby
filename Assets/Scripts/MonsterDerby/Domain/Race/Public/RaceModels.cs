// MonsterDerby.Domain.Race（C# 9 / クリーン版）
// 方針: Domain の型は「任意」を null で表現しない。
//       任意のコレクションは空配列（Array.Empty<T>()）で表現する。
//       公開コンストラクタは必須入力を検証し、不正なら即例外（Fail-Fast）。

namespace MonsterDerby.Domain.Race
{
    using System;
    using System.Collections.Generic;

    using MonsterDerby.Domain.Monster;
    using MonsterDerby.Domain.SharedKernel;
    /// <summary>
    /// シミュレーション設定。
    /// </summary>
    public sealed class SimulationConfiguration
    {
        public float SampleIntervalSeconds { get; }
        public float MaximumSimulationSeconds { get; }
        public float SpeedMultiplier { get; }

        public SimulationConfiguration(float sampleIntervalSeconds, float maximumSimulationSeconds, float speedMultiplier = 1.0f)
        {
            Guard.Positive(sampleIntervalSeconds, nameof(sampleIntervalSeconds));
            Guard.Positive(maximumSimulationSeconds, nameof(maximumSimulationSeconds));
            Guard.Positive(speedMultiplier, nameof(speedMultiplier));
            SampleIntervalSeconds = sampleIntervalSeconds;
            MaximumSimulationSeconds = maximumSimulationSeconds;
            SpeedMultiplier = speedMultiplier;
        }
    }

    /// <summary>
    /// レースの入力。
    /// ※RandomSeed は常に保持する（「未指定」を null で表現しない）。
    /// </summary>
    public sealed class RaceInput
    {
        public CourseId CourseId { get; }
        public MonsterSnapshot[] Participants { get; }
        public long RandomSeed { get; }
        public SimulationConfiguration SimulationConfiguration { get; }

        public RaceInput(CourseId courseId, MonsterSnapshot[] participants, long randomSeed, SimulationConfiguration simulationConfiguration)
        {
            CourseId = courseId;
            Participants = participants ?? throw new ArgumentNullException(nameof(participants));
            RandomSeed = randomSeed;
            SimulationConfiguration = Guard.NotNull(simulationConfiguration, nameof(simulationConfiguration));
        }

        public static RaceInput WithAutoSeed(CourseId courseId, MonsterSnapshot[] participants, SimulationConfiguration simulationConfiguration)
        {
            var seed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return new RaceInput(courseId, participants, seed, simulationConfiguration);
        }
    }

    /// <summary>
    /// レースに参加するモンスターの「レース用スナップショット」。
    /// RaceDomain は育成/遺伝/レベル等を知らず、この形に落とした入力だけを受け取る。
    /// </summary>
    public sealed class MonsterSnapshot
    {
        public MonsterId MonsterId { get; }
        public SpeciesId SpeciesId { get; }
        public RaceStats RaceStats { get; }
        public SkillId[] SkillIds { get; }

        public MonsterSnapshot(MonsterId monsterId, SpeciesId speciesId, RaceStats raceStats, SkillId[] skillIds)
        {
            MonsterId = monsterId;
            SpeciesId = speciesId;
            RaceStats = Guard.NotNull(raceStats, nameof(raceStats));
            SkillIds = skillIds ?? throw new ArgumentNullException(nameof(skillIds));
        }
    }

    /// <summary>
    /// 1サンプル（時刻t）における「状況全部」の観測用DTO。
    /// Unity 側はこれをもとに演出を生成できる。
    /// </summary>
    public sealed class RaceSample
    {
        public float TimeSeconds { get; }
        public int Tick { get; }

        public RunnerFrame[] Runners { get; }
        public ProjectileFrame[] Projectiles { get; }

        /// <summary>
        /// このTickで解決された着弾結果（命中/ミス/無効化）一覧。
        /// 「演出イベント」ではなく、観測ログとして提供する。
        /// </summary>
        public ImpactResolutionFrame[] ImpactResolutions { get; }

        public RaceSample(float timeSeconds, int tick, RunnerFrame[] runners, ProjectileFrame[] projectiles, ImpactResolutionFrame[] impactResolutions)
        {
            Guard.NonNegative(timeSeconds, nameof(timeSeconds));
            TimeSeconds = timeSeconds;
            Tick = tick;
            Runners = runners ?? throw new ArgumentNullException(nameof(runners));
            Projectiles = projectiles ?? throw new ArgumentNullException(nameof(projectiles));
            ImpactResolutions = impactResolutions ?? throw new ArgumentNullException(nameof(impactResolutions));
        }
    }

    /// <summary>
    /// 1ランナー（1レーン）の状況。
    /// </summary>
    public sealed class RunnerFrame
    {
        public int LaneIndex { get; }
        public MonsterId MonsterId { get; }

        public float DistanceMeters { get; }
        public float SpeedMetersPerSecond { get; }
        public float RemainingStamina { get; }

        public bool HasFinished { get; }
        public float FinishTimeSeconds { get; } // 未完走は -1

        public CooldownFrame[] Cooldowns { get; }
        public ActiveEffectFrame[] ActiveEffects { get; }

        public RunnerFrame(
            int laneIndex,
            MonsterId monsterId,
            float distanceMeters,
            float speedMetersPerSecond,
            float remainingStamina,
            bool hasFinished,
            float finishTimeSeconds,
            CooldownFrame[] cooldowns,
            ActiveEffectFrame[] activeEffects)
        {
            LaneIndex = laneIndex;
            MonsterId = monsterId;

            DistanceMeters = distanceMeters;
            SpeedMetersPerSecond = speedMetersPerSecond;
            RemainingStamina = remainingStamina;

            HasFinished = hasFinished;
            FinishTimeSeconds = finishTimeSeconds;

            Cooldowns = cooldowns ?? throw new ArgumentNullException(nameof(cooldowns));
            ActiveEffects = activeEffects ?? throw new ArgumentNullException(nameof(activeEffects));
        }
    }

    public sealed class CooldownFrame
    {
        public SkillId SkillId { get; }
        public float RemainingSeconds { get; }

        public CooldownFrame(SkillId skillId, float remainingSeconds)
        {
            Guard.NonNegative(remainingSeconds, nameof(remainingSeconds));
            SkillId = skillId;
            RemainingSeconds = remainingSeconds;
        }
    }

    public sealed class ActiveEffectFrame
    {
        public string EffectKind { get; }
        public float Magnitude { get; }
        public float RemainingSeconds { get; }

        public ActiveEffectFrame(string effectKind, float magnitude, float remainingSeconds)
        {
            Guard.NonNegative(remainingSeconds, nameof(remainingSeconds));
            EffectKind = Guard.NotNullOrEmpty(effectKind, nameof(effectKind));
            Magnitude = magnitude;
            RemainingSeconds = remainingSeconds;
        }
    }

    /// <summary>
    /// 飛翔体の状況（発射時ターゲット固定・着弾時判定）。
    /// </summary>
    public sealed class ProjectileFrame
    {
        public long ProjectileId { get; }
        public SkillId SkillId { get; }

        public int AttackerLaneIndex { get; }
        public int TargetLaneIndex { get; }

        public float LaunchTimeSeconds { get; }
        public float ImpactTimeSeconds { get; }

        public ProjectileFrame(
            long projectileId,
            SkillId skillId,
            int attackerLaneIndex,
            int targetLaneIndex,
            float launchTimeSeconds,
            float impactTimeSeconds)
        {
            ProjectileId = projectileId;
            SkillId = skillId;
            AttackerLaneIndex = attackerLaneIndex;
            TargetLaneIndex = targetLaneIndex;
            LaunchTimeSeconds = launchTimeSeconds;
            ImpactTimeSeconds = impactTimeSeconds;
        }
    }

    public enum ImpactResolutionKind
    {
        Hit,
        Miss,
        InvalidTargetFinished
    }

    public sealed class ImpactResolutionFrame
    {
        public float TimeSeconds { get; }
        public long ProjectileId { get; }
        public SkillId SkillId { get; }
        public int AttackerLaneIndex { get; }
        public int TargetLaneIndex { get; }
        public ImpactResolutionKind Kind { get; }
        public float RandomRoll01 { get; } // 命中判定に使った乱数（Miss/Hitで意味あり）
        public float HitChanceThreshold01 { get; }

        public ImpactResolutionFrame(
            float timeSeconds,
            long projectileId,
            SkillId skillId,
            int attackerLaneIndex,
            int targetLaneIndex,
            ImpactResolutionKind kind,
            float randomRoll01,
            float hitChanceThreshold01)
        {
            Guard.NonNegative(timeSeconds, nameof(timeSeconds));
            TimeSeconds = timeSeconds;
            ProjectileId = projectileId;
            SkillId = skillId;
            AttackerLaneIndex = attackerLaneIndex;
            TargetLaneIndex = targetLaneIndex;
            Kind = kind;
            RandomRoll01 = randomRoll01;
            HitChanceThreshold01 = hitChanceThreshold01;
        }
    }

    /// <summary>
    /// レースの最終結果 + 観測用サンプル。
    /// </summary>
    public sealed class RaceResult
    {
        public MonsterId[] LaneToMonsterId { get; }
        public MonsterId[] FinishOrder { get; }
        public IReadOnlyDictionary<MonsterId, float> FinishTimeSecondsByMonsterId { get; }
        public RaceSample[] Samples { get; }
        public long RandomSeedUsed { get; }

        public RaceResult(
            MonsterId[] laneToMonsterId,
            MonsterId[] finishOrder,
            IReadOnlyDictionary<MonsterId, float> finishTimeSecondsByMonsterId,
            RaceSample[] samples,
            long randomSeedUsed)
        {
            LaneToMonsterId = laneToMonsterId ?? throw new ArgumentNullException(nameof(laneToMonsterId));
            FinishOrder = finishOrder ?? throw new ArgumentNullException(nameof(finishOrder));
            FinishTimeSecondsByMonsterId = finishTimeSecondsByMonsterId ?? throw new ArgumentNullException(nameof(finishTimeSecondsByMonsterId));
            Samples = samples ?? throw new ArgumentNullException(nameof(samples));
            RandomSeedUsed = randomSeedUsed;
        }
    }

    /// <summary>
    /// 公開APIの戻り値。
    /// DebugEvents は常に配列（nullなし）。
    /// </summary>
    public sealed class RaceRunOutput
    {
        public RaceResult RaceResult { get; }
        public DebugEvent[] DebugEvents { get; }

        public RaceRunOutput(RaceResult raceResult, DebugEvent[] debugEvents)
        {
            RaceResult = Guard.NotNull(raceResult, nameof(raceResult));
            DebugEvents = debugEvents ?? throw new ArgumentNullException(nameof(debugEvents));
        }
    }
}
