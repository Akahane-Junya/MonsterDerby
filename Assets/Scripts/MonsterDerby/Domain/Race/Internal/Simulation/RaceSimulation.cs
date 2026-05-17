// MonsterDerby.Domain.Race（C# 9 / クリーン版）
// 方針: Domain の型は「任意」を null で表現しない。
//       任意のコレクションは空配列（Array.Empty<T>()）で表現する。
//       公開コンストラクタは必須入力を検証し、不正なら即例外（Fail-Fast）。

namespace MonsterDerby.Domain.Race
{
    using MonsterDerby.Domain.Course;
    using MonsterDerby.Domain.Skill;
    using MonsterDerby.Domain.SharedKernel;
    using System.Collections.Generic;
    using System.Linq;

    internal static class RaceSimulation
    {
        // 決定性:
        //  - 乱数はすべて DeterministicRng を経由させる
        //  - 同じ入力 + seed なら結果は同一になるべき

        public static RaceResult Run(
            RaceInput input,
            CourseDefinition course,
            IReadOnlyDictionary<SkillId, SkillDefinition> skills,
            DeterministicRng rng,
            bool createDebugEvents,
            out DebugEvent[] debugEvents)
        {
            Validate(input, course);

            var runners = input.Participants
                .Select((p, i) => new RunnerState(i, p))
                .ToArray();

            var state = new RaceState(runners);

            var debug = new List<DebugEvent>(capacity: createDebugEvents ? 256 : 0);
            var samples = new List<RaceSample>(capacity: 1024);

            float dt = input.SimulationConfiguration.SampleIntervalSeconds;
            float max = input.SimulationConfiguration.MaximumSimulationSeconds;

            // Tick=Sample 方針:
            //  - 0秒の初期状態も必ずサンプルとして返す
            //  - 以降は「1Tick進めるたびに必ず1サンプル」を追加する
            samples.Add(Sampling.Capture(state, Empty<ImpactResolutionFrame>.Array));

            // 完走者の追加は「MarkFinishedした順」ではなく、このループで同Tickの tie-break を安定化させる
            //  - 仕様: 同Tickで複数がゴールした場合、LaneIndex 昇順
            var finishOrder = new List<MonsterId>(capacity: runners.Length);

            while (state.TimeSeconds < max && finishOrder.Count < runners.Length)
            {
                var impactResolutions = TickPipeline.Step(
                    state: state,
                    dtSeconds: dt,
                    course: course,
                    skills: skills,
                    rng: rng,
                    debugEvents: debug,
                    createDebugEvents: createDebugEvents,
                    simulationConfig: input.SimulationConfiguration);

                // このTickで新たにゴールしたランナーを集め、LaneIndex昇順で確定
                var finishedThisTick = GetFinishedThisTick(state, finishOrder);
                if (finishedThisTick.Count > 0)
                {
                    finishedThisTick.Sort((a, b) => a.LaneIndex.CompareTo(b.LaneIndex));
                    foreach (var r in finishedThisTick)
                        finishOrder.Add(r.Snapshot.MonsterId);
                }

                // Tick=Sample: 毎Tick必ずサンプルを追加
                samples.Add(Sampling.Capture(state, impactResolutions));
            }

            var laneToMonster = state.Runners.OrderBy(r => r.LaneIndex).Select(r => r.Snapshot.MonsterId).ToArray();

            var finishTimes = new Dictionary<MonsterId, float>();
            for (int i = 0; i < state.Runners.Length; i++)
            {
                var r = state.Runners[i];
                if (r.HasFinished)
                    finishTimes[r.Snapshot.MonsterId] = r.FinishTimeSeconds;
            }

            debugEvents = createDebugEvents ? debug.ToArray() : Empty<DebugEvent>.Array;

            return new RaceResult(
                laneToMonsterId: laneToMonster,
                finishOrder: finishOrder.ToArray(),
                finishTimeSecondsByMonsterId: finishTimes,
                samples: samples.ToArray(),
                randomSeedUsed: rng.Seed);
        }

        private static void Validate(RaceInput input, CourseDefinition course)
        {
            Guard.NotNull(input, nameof(input));
            Guard.NotNull(course, nameof(course));
            if (input.Participants.Length <= 0) throw new System.ArgumentException("Participants must not be empty.", nameof(input));
            if (course.LengthMeters <= 0) throw new System.ArgumentException("Course length must be > 0.", nameof(course));
        }

        private static List<RunnerState> GetFinishedThisTick(RaceState state, List<MonsterId> alreadyOrdered)
        {
            // 既に finishOrder に入っているものを除外しつつ、FinishTime == state.TimeSeconds のものを集める。
            // （補間なし前提なので、このTickでゴールしたら FinishTimeSeconds は state.TimeSeconds になる）
            var list = new List<RunnerState>(capacity: 4);

            for (int i = 0; i < state.Runners.Length; i++)
            {
                var r = state.Runners[i];
                if (!r.HasFinished) continue;
                if (r.FinishTimeSeconds != state.TimeSeconds) continue;

                // 既に順序に入っているかチェック（小規模なので線形で良い）
                var mid = r.Snapshot.MonsterId;
                bool exists = false;
                for (int k = 0; k < alreadyOrdered.Count; k++)
                {
                    if (alreadyOrdered[k].Equals(mid)) { exists = true; break; }
                }
                if (!exists)
                    list.Add(r);
            }

            return list;
        }
    }
}
