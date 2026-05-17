using System;
using System.Collections.Generic;
using System.Linq;
using MonsterDerby.Application.Game;
using MonsterDerby.Domain.MasterData;
using MonsterDerby.Domain.Skill;

namespace MonsterDerby.Application.UseCases
{
    public sealed class GetCurrentMonsterStatusUseCase
    {
        private readonly GameSession _gameSession;
        private readonly ISpeciesRepository _speciesRepository;
        private readonly ISkillRepository _skillRepository;

        public GetCurrentMonsterStatusUseCase(GameSession gameSession, ISpeciesRepository speciesRepository, ISkillRepository skillRepository)
        {
            _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
            _speciesRepository = speciesRepository ?? throw new ArgumentNullException(nameof(speciesRepository));
            _skillRepository = skillRepository ?? throw new ArgumentNullException(nameof(skillRepository));
        }

        public MonsterStatusDto Execute()
        {
            if (!_gameSession.HasWorld || _gameSession.State?.CurrentMonster == null)
                throw new InvalidOperationException("現在のモンスターが存在しません。");

            var monster = _gameSession.State.CurrentMonster;
            var species = _speciesRepository.GetSpeciesDefinition(monster.SpeciesId);
            var speciesName = species.Name;
            var slots = new SkillSlotDto[4];
            for (int i = 0; i < 4; i++)
            {
                if (i < monster.MonsterSkills.Length)
                {
                    var ownedSkill = monster.MonsterSkills[i];
                    var skillIdStr = ownedSkill.Id.Value;
                    var skillName = _skillRepository.GetSkillDefinition(ownedSkill.Id).Name;
                    var unlockLevel = ownedSkill.UnlockLevel.Value;

                    slots[i] = monster.Level.Value >= unlockLevel
                        ? SkillSlotDto.Unlocked(skillIdStr, skillName)
                        : SkillSlotDto.Locked(skillIdStr, skillName, unlockLevel);
                }
                else
                {
                    slots[i] = SkillSlotDto.Empty();
                }
            }

            return new MonsterStatusDto(
                monster.MonsterId.Value,
                monster.Nickname,
                monster.SpeciesId.Value,
                speciesName,
                monster.Level.Value,
                monster.Experience.Value,
                monster.CurrentStats.TopSpeed,
                monster.CurrentStats.Accel,
                monster.CurrentStats.Stamina,
                slots);
        }
    }

    public sealed class SkillSlotDto
    {
        public string SkillId { get; }
        public string SkillName { get; }
        public bool IsUnlocked { get; }
        public int UnlockLevel { get; }
        public bool IsEmpty { get; }

        private SkillSlotDto(string skillId, string skillName, bool isUnlocked, int unlockLevel, bool isEmpty)
        {
            SkillId = skillId;
            SkillName = skillName;
            IsUnlocked = isUnlocked;
            UnlockLevel = unlockLevel;
            IsEmpty = isEmpty;
        }

        public static SkillSlotDto Unlocked(string skillId, string skillName)
            => new SkillSlotDto(skillId, skillName, true, 0, false);

        public static SkillSlotDto Locked(string skillId, string skillName, int unlockLevel)
            => new SkillSlotDto(skillId, skillName, false, unlockLevel, false);

        public static SkillSlotDto Empty()
            => new SkillSlotDto(string.Empty, string.Empty, false, 0, true);
    }

    public sealed class MonsterStatusDto
    {
        public MonsterStatusDto(
            string monsterId,
            string nickname,
            string speciesId,
            string speciesName,
            int level,
            int experience,
            int topSpeed,
            int acceleration,
            int stamina,
            SkillSlotDto[] skillSlots)
        {
            MonsterId = monsterId;
            Nickname = nickname;
            SpeciesId = speciesId;
            SpeciesName = speciesName;
            Level = level;
            Experience = experience;
            TopSpeed = topSpeed;
            Acceleration = acceleration;
            Stamina = stamina;
            SkillSlots = skillSlots ?? throw new ArgumentNullException(nameof(skillSlots));
        }

        public string MonsterId { get; }
        public string Nickname { get; }
        public string SpeciesId { get; }
        public string SpeciesName { get; }
        public int Level { get; }
        public int Experience { get; }
        public int TopSpeed { get; }
        public int Acceleration { get; }
        public int Stamina { get; }
        public SkillSlotDto[] SkillSlots { get; }
    }
}