using System;
using MonsterDerby.Application.UseCases;
using MonsterDerby.Domain.Course;
using MonsterDerby.Domain.Race;
using MonsterDerby.Domain.Skill;
using MonsterDerby.Domain.SharedKernel;

namespace MonsterDerby.Application.Context
{
    /// <summary>
    /// Race機能専用のContext実装
    /// </summary>
    internal sealed class RaceContext : IRaceContext
    {
        public RunRaceUseCase RunRaceUseCase { get; }

        public RaceContext(ICourseRepository courseRepository, ISkillRepository skillRepository)
        {
            Guard.NotNull(courseRepository, nameof(courseRepository));
            Guard.NotNull(skillRepository, nameof(skillRepository));

            // RaceEngine にリポジトリを渡す
            var raceEngine = new RaceEngine(courseRepository, skillRepository);

            // RunRaceUseCase に RaceEngine を渡す
            RunRaceUseCase = new RunRaceUseCase(raceEngine);
        }
    }
}