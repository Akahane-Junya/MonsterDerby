using System;
using System.Collections.Generic;
using MonsterDerby.Application.Game;
using MonsterDerby.Domain.Records;
using MonsterDerby.Domain.Monster;
using MonsterDerby.Domain.SharedKernel;
using UnityEngine;
using System.Globalization;

namespace MonsterDerby.Infrastructure.Save
{
    [CreateAssetMenu(fileName = "SaveDataGameSessionFactory", menuName = "MonsterDerby/GameSession/SaveDataFactory")]
    public sealed class SaveDataGameSessionFactoryAsset : GameSessionFactoryAsset
    {
        [Header("World")]
        [SerializeField] private int _money = 1000;

        [Header("Awards")]
        [SerializeField] private AwardEntryData[] _awardEntries;

        [Header("Current Monster")]
        [SerializeField] private MonsterSaveData _currentMonster;


        [Header("Catalog")]
        [SerializeField] private MonsterDerby.Domain.Catalog.CatalogUnlockState[] _monsterCatalogStates;
        [SerializeField] private MonsterDerby.Domain.Catalog.CatalogUnlockState[] _skillCatalogStates;

        private void Reset()
        {
            EnsureSerializedDefaults();
        }

        private void OnValidate()
        {
            EnsureSerializedDefaults();
        }

        public override GameSession Create()
        {
            if (_currentMonster == null)
                throw new InvalidOperationException("currentMonster が未設定です。");

            var currentMonster = BuildMonsterInstance(_currentMonster);
            var awardEntries = BuildAwardEntries();

            var session = new GameSession();
            session.StartNew(_money, currentMonster, awardEntries);
            return session;
        }

        private RaceAwardState[] BuildAwardEntries()
        {
            if (_awardEntries == null || _awardEntries.Length == 0)
                return Array.Empty<RaceAwardState>();

            var result = new RaceAwardState[_awardEntries.Length];
            for (int i = 0; i < _awardEntries.Length; i++)
            {
                var entry = _awardEntries[i];
                if (entry == null)
                    continue;

                result[i] = entry.ToDomain();
            }

            return result;
        }

        private static RaceAwardState[] BuildInitialAwardEntries()
        {
            return new[]
            {
                new RaceAwardState(
                    "race_001",
                    new TrophyOwnership(
                        new TrophyId("race_001_Gold_Player"),
                        "race_001",
                        TrophyMedal.Gold,
                        new MonsterId("1")),
                    new CourseRecord(
                        "race_001",
                        new RaceTimeMs(82340),
                        new MonsterId("1"))),
                new RaceAwardState(
                    "race_002",
                    new TrophyOwnership(
                        new TrophyId("race_002_Silver_Player"),
                        "race_002",
                        TrophyMedal.Silver,
                        new MonsterId("1")),
                    new CourseRecord(
                        "race_002",
                        new RaceTimeMs(90510),
                        new MonsterId("1"))),
                new RaceAwardState(
                    "race_003",
                    new TrophyOwnership(
                        new TrophyId("race_003_Bronze_Player"),
                        "race_003",
                        TrophyMedal.Bronze,
                        new MonsterId("1")),
                    new CourseRecord(
                        "race_003",
                        new RaceTimeMs(78120),
                        new MonsterId("1"))),
            };
        }


        private void EnsureSerializedDefaults()
        {
            if (_awardEntries == null || _awardEntries.Length == 0)
            {
                _awardEntries = new[]
                {
                    new AwardEntryData
                    {
                        raceId = "race_001",
                        trophyOwnership = new TrophyOwnershipData
                        {
                            trophyId = "race_001_Gold_Player",
                            raceId = "race_001",
                            medal = TrophyMedal.Gold,
                            winnerMonsterId = "1",
                        },
                        courseRecord = new CourseRecordData
                        {
                            raceId = "race_001",
                            bestTimeMmSsCc = "012234",
                            holderMonsterId = "1",
                        },
                    },
                    new AwardEntryData
                    {
                        raceId = "race_002",
                        trophyOwnership = new TrophyOwnershipData
                        {
                            trophyId = "race_002_Silver_Player",
                            raceId = "race_002",
                            medal = TrophyMedal.Silver,
                            winnerMonsterId = "1",
                        },
                        courseRecord = new CourseRecordData
                        {
                            raceId = "race_002",
                            bestTimeMmSsCc = "013050",
                            holderMonsterId = "1",
                        },
                    },
                    new AwardEntryData
                    {
                        raceId = "race_003",
                        trophyOwnership = new TrophyOwnershipData
                        {
                            trophyId = "race_003_Bronze_Player",
                            raceId = "race_003",
                            medal = TrophyMedal.Bronze,
                            winnerMonsterId = "1",
                        },
                        courseRecord = new CourseRecordData
                        {
                            raceId = "race_003",
                            bestTimeMmSsCc = "011812",
                            holderMonsterId = "1",
                        },
                    },
                };
            }

            if (_monsterCatalogStates == null || _monsterCatalogStates.Length == 0)
                _monsterCatalogStates = new[] {
                    new MonsterDerby.Domain.Catalog.CatalogUnlockState { Id = "Slime", Stage = MonsterDerby.Domain.Catalog.CatalogUnlockStage.Encountered }
                };
            if (_skillCatalogStates == null || _skillCatalogStates.Length == 0)
                _skillCatalogStates = new[] {
                    new MonsterDerby.Domain.Catalog.CatalogUnlockState { Id = "Fireball", Stage = MonsterDerby.Domain.Catalog.CatalogUnlockStage.Encountered }
                };

            if (_currentMonster == null)
                _currentMonster = new MonsterSaveData();

            if (_currentMonster.growthIncrements == null)
            {
                _currentMonster.growthIncrements = new MonsterStatsData[GrowthIncrements.LevelCount];
            }
            else if (_currentMonster.growthIncrements.Length != GrowthIncrements.LevelCount)
            {
                var resized = new MonsterStatsData[GrowthIncrements.LevelCount];
                var copyLength = Math.Min(_currentMonster.growthIncrements.Length, resized.Length);
                Array.Copy(_currentMonster.growthIncrements, resized, copyLength);
                _currentMonster.growthIncrements = resized;
            }

            if (_currentMonster.monsterSkills == null)
                _currentMonster.monsterSkills = Array.Empty<SkillData>();

            if (_currentMonster.parentMonsterIds == null)
                _currentMonster.parentMonsterIds = Array.Empty<string>();
        }

        private static MonsterInstance BuildMonsterInstance(MonsterSaveData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.growthIncrements == null || data.growthIncrements.Length != GrowthIncrements.LevelCount)
                throw new InvalidOperationException($"growthIncrements は {GrowthIncrements.LevelCount} 件必要です。実際: {data.growthIncrements?.Length ?? 0}");
            if (data.monsterSkills == null)
                throw new InvalidOperationException("monsterSkills が未設定です。空配列を明示設定してください。");
            if (data.parentMonsterIds == null)
                throw new InvalidOperationException("parentMonsterIds が未設定です。空配列を明示設定してください。");

            var increments = new List<MonsterStats>(data.growthIncrements.Length);
            for (int i = 0; i < data.growthIncrements.Length; i++)
            {
                increments.Add(data.growthIncrements[i].ToDomain());
            }

            var monsterSkills = new MonsterSkill[data.monsterSkills.Length];
            for (int i = 0; i < data.monsterSkills.Length; i++)
            {
                monsterSkills[i] = new MonsterSkill(new SkillId(data.monsterSkills[i].skillId), new Level(data.monsterSkills[i].unlockLevel));
            }

            var parentMonsterIds = new MonsterId[data.parentMonsterIds.Length];
            for (int i = 0; i < data.parentMonsterIds.Length; i++)
            {
                parentMonsterIds[i] = new MonsterId(data.parentMonsterIds[i]);
            }

            return new MonsterInstance(
                new MonsterId(data.monsterId),
                new SpeciesId(data.speciesId),
                data.nickname,
                new Experience(data.experience),
                new GrowthIncrements(increments),
                monsterSkills,
                parentMonsterIds);
        }

        [Serializable]
        private sealed class MonsterSaveData
        {
            public string monsterId;
            public string speciesId;
            public string nickname;
            public int experience;

            public MonsterStatsData[] growthIncrements;
            public SkillData[] monsterSkills;
            public string[] parentMonsterIds;
        }

        [Serializable]
        private struct MonsterStatsData
        {
            public int topSpeed;
            public int accel;
            public int stamina;

            public MonsterStats ToDomain() => new MonsterStats(topSpeed, accel, stamina);
        }

        [Serializable]
        private struct SkillData
        {
            public string skillId;
            [Min(1)] public int unlockLevel;
        }

        [Serializable]
        private sealed class AwardEntryData
        {
            public string raceId;
            public TrophyOwnershipData trophyOwnership;
            public CourseRecordData courseRecord;

            public RaceAwardState ToDomain()
            {
                return new RaceAwardState(
                    raceId,
                    trophyOwnership != null ? trophyOwnership.ToDomain() : null,
                    courseRecord != null ? courseRecord.ToDomain() : null);
            }
        }

        [Serializable]
        private sealed class TrophyOwnershipData
        {
            public string trophyId;
            public string raceId;
            public TrophyMedal medal;
            public string winnerMonsterId;

            public TrophyOwnership ToDomain()
            {
                return new TrophyOwnership(
                    new TrophyId(trophyId),
                    raceId,
                    medal,
                    new MonsterId(winnerMonsterId));
            }
        }

        [Serializable]
        private sealed class CourseRecordData
        {
            public string raceId;
            public string bestTimeMmSsCc;
            public string holderMonsterId;

            public CourseRecord ToDomain()
            {
                if (!TryParseCompactTimeToMilliseconds(bestTimeMmSsCc, out var resolvedMs))
                    throw new InvalidOperationException($"CourseRecordData.bestTimeMmSsCc の形式が不正です: '{bestTimeMmSsCc}'");

                return new CourseRecord(
                    raceId,
                    new RaceTimeMs(resolvedMs),
                    new MonsterId(holderMonsterId));
            }

            private static bool TryParseCompactTimeToMilliseconds(string text, out int milliseconds)
            {
                milliseconds = 0;
                var trimmed = text.Trim();

                var sixDigits = trimmed.Replace(":", string.Empty).Replace(".", string.Empty);
                if (sixDigits.Length != 6)
                {
                    return false;
                }

                if (!int.TryParse(sixDigits.Substring(0, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var minutes))
                {
                    return false;
                }

                if (!int.TryParse(sixDigits.Substring(2, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var seconds))
                {
                    return false;
                }

                if (!int.TryParse(sixDigits.Substring(4, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var centiseconds))
                {
                    return false;
                }

                if (minutes < 0 || minutes > 59 || seconds < 0 || seconds > 59 || centiseconds < 0 || centiseconds > 99)
                {
                    return false;
                }

                milliseconds = ((minutes * 60) + seconds) * 1000 + (centiseconds * 10);
                return true;
            }
        }
    }
}
