using System;
using System.Collections.Generic;
using MonsterDerby.Domain.Monster;
using MonsterDerby.Domain.SharedKernel;
using UnityEngine;

namespace MonsterDerby.Infrastructure.MasterData
{
    [CreateAssetMenu(menuName = "MonsterDerby/MasterData/Race")]
    public sealed class RaceDefinitionSO : ScriptableObject
    {
        [Serializable]
        public sealed class MonsterStatsEntry
        {
            public int topSpeed;
            public int accel;
            public int stamina;

            public MonsterStats ToDomain()
            {
                return new MonsterStats(topSpeed, accel, stamina);
            }
        }

        [Serializable]
        public sealed class SkillEntry
        {
            public string skillId;
            public int unlockLevel = 1;

            public MonsterSkill ToDomain()
            {
                return new MonsterSkill(new SkillId(skillId), new Level(unlockLevel));
            }
        }

        [Serializable]
        public sealed class ParticipantEntry
        {
            public string monsterId;
            public string speciesId;
            public string nickname;
            public int experience;
            public MonsterStatsEntry[] growthIncrements;
            public SkillEntry[] skills;
            public string[] parentMonsterIds;

            public MonsterInstance ToDomain()
            {
                if (growthIncrements == null || growthIncrements.Length != GrowthIncrements.LevelCount)
                    throw new InvalidOperationException($"growthIncrements は {GrowthIncrements.LevelCount} 件必要です。実際: {growthIncrements?.Length ?? 0}");

                if (skills == null)
                    throw new InvalidOperationException("skills が未設定です。");

                if (parentMonsterIds == null)
                    throw new InvalidOperationException("parentMonsterIds が未設定です。");

                var increments = new List<MonsterStats>(growthIncrements.Length);
                for (int i = 0; i < growthIncrements.Length; i++)
                {
                    var entry = growthIncrements[i];
                    if (entry == null)
                        throw new InvalidOperationException($"growthIncrements[{i}] が null です。");
                    increments.Add(entry.ToDomain());
                }

                var monsterSkills = new MonsterSkill[skills.Length];
                for (int i = 0; i < skills.Length; i++)
                {
                    var skill = skills[i];
                    if (skill == null)
                        throw new InvalidOperationException($"skills[{i}] が null です。");
                    monsterSkills[i] = skill.ToDomain();
                }

                var parentIds = new MonsterId[parentMonsterIds.Length];
                for (int i = 0; i < parentMonsterIds.Length; i++)
                {
                    parentIds[i] = new MonsterId(parentMonsterIds[i]);
                }

                return new MonsterInstance(
                    new MonsterId(monsterId),
                    new SpeciesId(speciesId),
                    nickname ?? string.Empty,
                    new Experience(experience),
                    new GrowthIncrements(increments),
                    monsterSkills,
                    parentIds);
            }
        }

        [Header("Identity")]
        public string raceId;
        public string raceName;

        [Header("Presentation")]
        public Sprite goldMedalImage;
        public Sprite silverMedalImage;
        public Sprite bronzeMedalImage;

        [Header("Rules")]
        public int entryFee;
        [Min(1)] public int minLevel = 1;
        [Min(1)] public int maxLevel = 10;

        [Header("Course")]
        public CourseDefinitionSO course;

        [Header("Participants")]
        public ParticipantEntry[] participants;

        public CourseId GetCourseId()
        {
            if (course == null)
                throw new InvalidOperationException("course が未設定です。");

            return new CourseId(course.courseId);
        }

        public MonsterInstance[] BuildParticipants()
        {
            if (participants == null)
                return Array.Empty<MonsterInstance>();

            var built = new MonsterInstance[participants.Length];
            for (int i = 0; i < participants.Length; i++)
            {
                var participant = participants[i];
                if (participant == null)
                    throw new InvalidOperationException($"participants[{i}] が null です。");
                built[i] = participant.ToDomain();
            }

            return built;
        }

        private void OnValidate()
        {
            if (entryFee < 0)
                entryFee = 0;
        }
    }
}
