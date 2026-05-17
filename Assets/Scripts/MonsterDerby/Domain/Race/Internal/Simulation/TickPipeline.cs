// MonsterDerby.Domain.Race（C# 9 / クリーン版）
// 方針: Domain の型は「任意」を null で表現しない。
//       任意のコレクションは空配列（Array.Empty<T>()）で表現する。
//       公開コンストラクタは必須入力を検証し、不正なら即例外（Fail-Fast）。

namespace MonsterDerby.Domain.Race
{
    using MonsterDerby.Domain.Course;
    using MonsterDerby.Domain.Skill;
    using MonsterDerby.Domain.SharedKernel;
    using System;
    using System.Collections.Generic;

    internal static class TickPipeline
    {
        private const float BaseRaceSpeedScale = 0.4f;
        private const float AccelerationImpactScale = 1.8f;
        private const float ExhaustedMinSpeedRate = 0.30f;
        private const float OnHitSpeedRetention = 0.75f;
        private const float SkillStartDelaySeconds = 2.0f;
        private const float SkillCooldownExtraSeconds = 1.5f;

        // Tick 順序（実質的にゲームルールの一部）:
        //  1) 時間を進める
        //  2) クールダウンと時間制効果を減衰させる
        //  3) 地形パッシブ効果を適用する（短時間効果として）
        // 4) 予約済み攻撃の着弾を解決（命中/ミス/無効化 + 効果適用）
        // 5) アクティブ攻撃を予約する（実体オブジェクトは作らない）
        //  6) 走行を進める（速度/距離/スタミナ）
        //
        // 読みやすさのために、このパイプラインは直列・明示的に保つ。

        public static ImpactResolutionFrame[] Step(
            RaceState state,
            float dtSeconds,
            CourseDefinition course,
            IReadOnlyDictionary<SkillId, SkillDefinition> skills,
            DeterministicRng rng,
            List<DebugEvent> debugEvents,
            bool createDebugEvents,
            SimulationConfiguration simulationConfig)
        {
            state.AdvanceTime(dtSeconds);

            // 1) 減衰
            DecayEffectsAndCooldowns(state, dtSeconds);

            // 2) 地形パッシブ
            ApplyPassiveTerrainEffects(state, course, skills);

            // 3) 予約済み攻撃の着弾を解決
            var resolutions = ResolvePendingAttacks(state, skills, rng, debugEvents, createDebugEvents);

            // 4) 能動攻撃を予約する
            ScheduleActiveAttacks(state, skills, rng);

            // 5) 移動
            AdvanceRunners(state, dtSeconds, course, course.LengthMeters, simulationConfig);

            return resolutions;
        }

        private static void DecayEffectsAndCooldowns(RaceState state, float dtSeconds)
        {
            foreach (var r in state.Runners)
            {
                for (int i = r.Cooldowns.Count - 1; i >= 0; i--)
                {
                    var cd = r.Cooldowns[i];
                    cd.RemainingSeconds -= dtSeconds;
                    if (cd.RemainingSeconds <= 0f)
                        r.Cooldowns.RemoveAt(i);
                }

                for (int i = r.ActiveEffects.Count - 1; i >= 0; i--)
                {
                    var e = r.ActiveEffects[i];
                    e.RemainingSeconds -= dtSeconds;
                    if (e.RemainingSeconds <= 0f)
                        r.ActiveEffects.RemoveAt(i);
                }
            }
        }

        private static void ApplyPassiveTerrainEffects(
            RaceState state,
            CourseDefinition course,
            IReadOnlyDictionary<SkillId, SkillDefinition> skills)
        {
            // 分かりやすさのため、地形パッシブは毎Tick「短時間効果」として再適用する。
            // （将来、区間突入/離脱イベント方式に変えたくなったらここを差し替える。）
            foreach (var r in state.Runners)
            {
                if (r.HasFinished) continue;

                var terrain = TerrainResolver.GetTerrainTagAt(course, r.DistanceMeters);
                if (string.IsNullOrEmpty(terrain)) continue;

                foreach (var sid in r.Snapshot.SkillIds)
                {
                    if (!skills.TryGetValue(sid, out var def))
                    {
                        UnityEngine.Debug.LogWarning($"TickPipeline: Skill definition not found for SkillId '{sid.Value}' in terrain passive phase.");
                        continue;
                    }

                    var terrainSkill = def as PassiveTerrainSkillDefinition;
                    if (terrainSkill == null) continue;
                    if (terrainSkill.TerrainTag != terrain) continue;

                    foreach (var eff in terrainSkill.Effects)
                    {
                        // かなり短い持続時間を与えることで「現在有効な効果」を表現する。
                        r.ActiveEffects.Add(new ActiveEffectState(eff.Effect, remainingSeconds: 0.05f));
                    }
                }
            }
        }

        private static ImpactResolutionFrame[] ResolvePendingAttacks(
            RaceState state,
            IReadOnlyDictionary<SkillId, SkillDefinition> skills,
            DeterministicRng rng,
            List<DebugEvent> debugEvents,
            bool createDebugEvents)
        {
            List<ImpactResolutionFrame> results = null;

            for (int i = state.Projectiles.Count - 1; i >= 0; i--)
            {
                var pending = state.Projectiles[i];
                if (state.TimeSeconds < pending.ImpactTimeSeconds)
                    continue;

                state.Projectiles.RemoveAt(i);

                if (!skills.TryGetValue(pending.SkillId, out var def))
                {
                    UnityEngine.Debug.LogWarning($"TickPipeline: Skill definition not found for SkillId '{pending.SkillId.Value}' in attack resolve phase.");
                    continue;
                }

                var atk = def as ActiveAttackSkillDefinition;
                if (atk == null)
                    continue;

                var target = state.Runners[pending.TargetLaneIndex];
                if (target.HasFinished)
                {
                    (results ?? (results = new List<ImpactResolutionFrame>(4))).Add(
                        new ImpactResolutionFrame(
                            timeSeconds: state.TimeSeconds,
                            projectileId: pending.ProjectileId,
                            skillId: pending.SkillId,
                            attackerLaneIndex: pending.AttackerLaneIndex,
                            targetLaneIndex: pending.TargetLaneIndex,
                            kind: ImpactResolutionKind.InvalidTargetFinished,
                            randomRoll01: 0f,
                            hitChanceThreshold01: atk.HitModel.HitChance01));

                    continue;
                }

                var roll = rng.NextFloat01();
                var willHit = roll <= atk.HitModel.HitChance01;
                var kind = willHit ? ImpactResolutionKind.Hit : ImpactResolutionKind.Miss;

                (results ?? (results = new List<ImpactResolutionFrame>(4))).Add(
                    new ImpactResolutionFrame(
                        timeSeconds: state.TimeSeconds,
                        projectileId: pending.ProjectileId,
                        skillId: pending.SkillId,
                        attackerLaneIndex: pending.AttackerLaneIndex,
                        targetLaneIndex: pending.TargetLaneIndex,
                        kind: kind,
                        randomRoll01: roll,
                        hitChanceThreshold01: atk.HitModel.HitChance01));

                if (createDebugEvents)
                {
                    debugEvents.Add(new AttackResolvedAtLaunchDebugEvent(
                        timeSeconds: state.TimeSeconds,
                        attackerLaneIndex: pending.AttackerLaneIndex,
                        skillId: pending.SkillId,
                        targetLaneIndex: pending.TargetLaneIndex,
                        randomRoll01: roll,
                        hitChanceThreshold01: atk.HitModel.HitChance01,
                        willHit: willHit,
                        impactTimeSeconds: state.TimeSeconds));
                }

                if (!willHit)
                    continue;

                var attacker = state.Runners[pending.AttackerLaneIndex];
                foreach (var eff in atk.Effects)
                {
                    ApplyEffectToSide(attacker, target, eff);
                }

                ApplyOnHitSpeedPenalty(target);
            }

            return (results == null || results.Count == 0)
                ? Empty<ImpactResolutionFrame>.Array
                : results.ToArray();
        }

        private static void ScheduleActiveAttacks(
            RaceState state,
            IReadOnlyDictionary<SkillId, SkillDefinition> skills,
            DeterministicRng rng)
        {
            if (state.TimeSeconds < SkillStartDelaySeconds)
                return;

            foreach (var attacker in state.Runners)
            {
                if (attacker.HasFinished) continue;

                foreach (var sid in attacker.Snapshot.SkillIds)
                {
                    if (!skills.TryGetValue(sid, out var def))
                    {
                        UnityEngine.Debug.LogWarning($"TickPipeline: Skill definition not found for SkillId '{sid.Value}' in active attack phase.");
                        continue;
                    }

                    var atk = def as ActiveAttackSkillDefinition;
                    if (atk == null) continue;

                    if (IsOnCooldown(attacker, sid))
                        continue;

                    var targets = SelectTargets(attacker, state.Runners, atk, rng);
                    if (targets.Count == 0)
                        continue;

                    for (int i = 0; i < targets.Count; i++)
                    {
                        var target = targets[i];

                        var impactDelaySeconds = ComputeImpactDelaySeconds(attacker.DistanceMeters, target.DistanceMeters, atk);
                        var impactTimeSeconds = state.TimeSeconds + impactDelaySeconds;

                        state.Projectiles.Add(new ProjectileState(
                            projectileId: CombineToProjectileId(rng.Seed, rng.Counter),
                            attackerLaneIndex: attacker.LaneIndex,
                            targetLaneIndex: target.LaneIndex,
                            skillId: sid,
                            launchTimeSeconds: state.TimeSeconds,
                            impactTimeSeconds: impactTimeSeconds));
                    }

                    // クールダウン開始（発生間隔を少し広げる）
                    var cooldownSeconds = atk.CooldownSeconds + SkillCooldownExtraSeconds;
                    if (cooldownSeconds > 0f)
                        attacker.Cooldowns.Add(new CooldownState(sid, cooldownSeconds));

                    // 1Tickで1体につき1スキルだけ発動
                    break;
                }
            }
        }

        private static void ApplyEffectToSide(RunnerState attacker, RunnerState target, EffectDefinition effectDefinition)
        {
            float duration = effectDefinition.IsTimed ? effectDefinition.DurationSeconds : 0f;
            if (duration <= 0f) duration = 0.5f;

            if (effectDefinition.TargetSide == EffectTargetSide.Attacker || effectDefinition.TargetSide == EffectTargetSide.Both)
            {
                attacker.ActiveEffects.Add(new ActiveEffectState(effectDefinition.Effect, duration));
            }

            if (effectDefinition.TargetSide == EffectTargetSide.Target || effectDefinition.TargetSide == EffectTargetSide.Both)
            {
                if (!ReferenceEquals(attacker, target) || effectDefinition.TargetSide != EffectTargetSide.Attacker)
                {
                    target.ActiveEffects.Add(new ActiveEffectState(effectDefinition.Effect, duration));
                }
            }
        }

        private static void ApplyOnHitSpeedPenalty(RunnerState target)
        {
            if (target == null) return;

            var reducedSpeed = target.SpeedMetersPerSecond * OnHitSpeedRetention;
            target.SpeedMetersPerSecond = reducedSpeed < 0f ? 0f : reducedSpeed;
        }

        private static List<RunnerState> SelectTargets(
            RunnerState attacker,
            RunnerState[] runners,
            ActiveAttackSkillDefinition skill,
            DeterministicRng rng)
        {
            var candidates = new List<RunnerState>(capacity: runners.Length);

            for (int i = 0; i < runners.Length; i++)
            {
                var target = runners[i];
                if (target == attacker || target.HasFinished)
                    continue;

                var distanceDelta = target.DistanceMeters - attacker.DistanceMeters;
                var absDelta = Math.Abs(distanceDelta);
                if (absDelta > skill.Range.Meters)
                    continue;

                switch (skill.Range.Direction)
                {
                    case RangeDirection.Front:
                        if (distanceDelta > 0f) candidates.Add(target);
                        break;
                    case RangeDirection.Back:
                        if (distanceDelta < 0f) candidates.Add(target);
                        break;
                    case RangeDirection.Both:
                    default:
                        candidates.Add(target);
                        break;
                }
            }

            if (candidates.Count == 0)
                return candidates;

            switch (skill.Targeting.Mode)
            {
                case TargetingMode.RandomOne:
                    return new List<RunnerState>
                    {
                        candidates[rng.NextInt(candidates.Count)]
                    };

                case TargetingMode.Nearest:
                    candidates.Sort((a, b) =>
                    {
                        var da = Math.Abs(a.DistanceMeters - attacker.DistanceMeters);
                        var db = Math.Abs(b.DistanceMeters - attacker.DistanceMeters);
                        return da.CompareTo(db);
                    });
                    return new List<RunnerState> { candidates[0] };

                case TargetingMode.All:
                    if (skill.Targeting.MaximumTargets > 0 && candidates.Count > skill.Targeting.MaximumTargets)
                    {
                        candidates.Sort((a, b) =>
                        {
                            var da = Math.Abs(a.DistanceMeters - attacker.DistanceMeters);
                            var db = Math.Abs(b.DistanceMeters - attacker.DistanceMeters);
                            return da.CompareTo(db);
                        });
                        candidates.RemoveRange(skill.Targeting.MaximumTargets, candidates.Count - skill.Targeting.MaximumTargets);
                    }
                    return candidates;

                default:
                    return new List<RunnerState> { candidates[0] };
            }
        }

        private static void AdvanceRunners(RaceState state, float dtSeconds, CourseDefinition course, float courseLengthMeters, SimulationConfiguration simulationConfig)
        {
            var agg = new EffectAggregator();

            foreach (var r in state.Runners)
            {
                if (r.HasFinished) continue;

                agg.Reset();
                for (int i = 0; i < r.ActiveEffects.Count; i++)
                {
                    var e = r.ActiveEffects[i];
                    agg.Apply(e.Effect);
                }

                // 移動（暫定数式）
                var baseStats = r.Snapshot.RaceStats;

                // 有効最大速度（暫定A）: (baseMax * mult + add) * slow
                var effectiveMaxSpeed =
                    (baseStats.MaximumSpeed * agg.SpeedMultiplier + agg.SpeedAdditive)
                    * agg.SlowMultiplier;

                if (effectiveMaxSpeed < 0f) effectiveMaxSpeed = 0f;

                // スタミナが0なら加速度0
                var accel = (r.RemainingStamina <= 0f)
                    ? 0f
                    : baseStats.Acceleration * agg.AccelerationMultiplier * AccelerationImpactScale;

                var newSpeed = r.SpeedMetersPerSecond + accel * dtSeconds;
                if (newSpeed > effectiveMaxSpeed) newSpeed = effectiveMaxSpeed;
                if (r.RemainingStamina <= 0f)
                {
                    var exhaustedMinSpeed = effectiveMaxSpeed * ExhaustedMinSpeedRate;
                    if (newSpeed < exhaustedMinSpeed) newSpeed = exhaustedMinSpeed;
                }
                if (newSpeed < 0f) newSpeed = 0f;
                var scaledMoveSpeed = newSpeed * BaseRaceSpeedScale;

                r.SpeedMetersPerSecond = newSpeed;
                r.DistanceMeters += scaledMoveSpeed * dtSeconds * simulationConfig.SpeedMultiplier;

                // スタミナ消費: 地形ペナルティとスキル効果の双方を反映
                var terrainTag = TerrainResolver.GetTerrainTagAt(course, r.DistanceMeters);
                var terrainDrainMultiplier = ResolveTerrainDrainMultiplier(terrainTag);
                var drainPerSecond = 1f * terrainDrainMultiplier * agg.StaminaDrainMultiplier;
                r.RemainingStamina -= drainPerSecond * dtSeconds;
                if (r.RemainingStamina < 0f) r.RemainingStamina = 0f;

                if (!r.HasFinished && r.DistanceMeters >= courseLengthMeters)
                {
                    r.MarkFinished(state.TimeSeconds);
                    // 同Tickで複数がゴールした場合は LaneIndex 昇順で確定させたいが、
                    // ここは後段（RaceSimulation側）でまとめて処理するのがクリーン。
                }
            }
        }

        private static bool IsOnCooldown(RunnerState runner, SkillId skillId)
        {
            for (int i = 0; i < runner.Cooldowns.Count; i++)
                if (runner.Cooldowns[i].SkillId == skillId)
                    return true;
            return false;
        }

        private static long CombineToProjectileId(long seed, long counter)
        {
            unchecked
            {
                return (seed * 31L) ^ counter;
            }
        }

        private static float ComputeImpactDelaySeconds(float attackerDistance, float targetDistance, ActiveAttackSkillDefinition skill)
        {
            var meters = Math.Abs(targetDistance - attackerDistance);

            switch (skill.Projectile.TravelTimeModel)
            {
                case TravelTimeModel.ProportionalToDistance:
                default:
                    return meters * 0.05f;
            }
        }

        private static float ResolveTerrainDrainMultiplier(string terrainTag)
        {
            if (string.IsNullOrEmpty(terrainTag)) return 1f;

            switch (terrainTag)
            {
                case "grassland":
                    return 1f;
                case "cave":
                    return 1.15f;
                case "ice":
                    return 1.35f;
                case "magma":
                    return 2.0f;
                default:
                    return 1f;
            }
        }
    }
}
