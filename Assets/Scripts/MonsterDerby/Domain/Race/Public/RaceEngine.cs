// MonsterDerby.Domain.Race（C# 9 / クリーン版）
// 方針: Domain の型は「任意」を null で表現しない。
//       任意のコレクションは空配列（Array.Empty<T>()）で表現する。
//       公開コンストラクタは必須入力を検証し、不正なら即例外（Fail-Fast）。


namespace MonsterDerby.Domain.Race
{
    using System;
    using System.Collections.Generic;

    using MonsterDerby.Domain.Course;
    using MonsterDerby.Domain.Skill;
    using MonsterDerby.Domain.SharedKernel;


    /// <summary>
    /// 公開 API の入口。依存の向きを内側に保つ:
    ///  - リポジトリ（インターフェース）とシミュレーション（内部）にのみ依存
    ///  - Unity 型に依存しない
    ///  - 出力に null を含めない
    /// </summary>
    public sealed class RaceEngine
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ISkillRepository _skillRepository;

        public RaceEngine(ICourseRepository courseRepository, ISkillRepository skillRepository)
        {
            _courseRepository = Guard.NotNull(courseRepository, nameof(courseRepository));
            _skillRepository = Guard.NotNull(skillRepository, nameof(skillRepository));
        }

        public RaceRunOutput RunRace(RaceInput input, bool createDebugEvents)
        {
            Guard.NotNull(input, nameof(input));

            var course = _courseRepository.GetCourseDefinition(input.CourseId);
            Guard.NotNull(course, nameof(course));

            var skills = CollectSkillDefinitions(input);

            var rng = new DeterministicRng(input.RandomSeed);

            var result = RaceSimulation.Run(
                input: input,
                course: course,
                skills: skills,
                rng: rng,
                createDebugEvents: createDebugEvents,
                out var debugEvents);

            // Never return null
            if (!createDebugEvents)
                debugEvents = Empty<DebugEvent>.Array;

            return new RaceRunOutput(result, debugEvents);
        }

        private IReadOnlyDictionary<SkillId, SkillDefinition> CollectSkillDefinitions(RaceInput input)
        {
            var dict = new Dictionary<SkillId, SkillDefinition>();

            foreach (var p in input.Participants)
            {
                foreach (var sid in p.SkillIds)
                {
                    if (!dict.ContainsKey(sid))
                        dict.Add(sid, _skillRepository.GetSkillDefinition(sid));
                }
            }

            return dict;
        }
    }
}
