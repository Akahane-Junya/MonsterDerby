using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MonsterDerby.Domain.Monster;
using MonsterDerby.Domain.Records;
using MonsterDerby.Domain.SharedKernel;
using UnityEngine;
using MonsterDerby.Domain.World;

namespace MonsterDerby.Infrastructure.Save
{
    /// <summary>
    /// JSONファイル保存実装。
    /// Application.persistentDataPathに保存する。
    /// </summary>
    public sealed class JsonSaveRepository : ISaveRepository
    {
        private const int SaveVersion = 3;
        private const string FileName = "save.json";
        private const string BackupExt = ".bak";
        private const string TempExt = ".tmp";

        private string FilePath => Path.Combine(UnityEngine.Application.persistentDataPath, FileName);
        private string TempPath => FilePath + TempExt;
        private string BackupPath => FilePath + BackupExt;

        /// <summary>
        /// JSONにするためのDTO（Data Transfer Object）。
        /// Domain(WorldState)を直接シリアライズしないことで、
        /// 将来の項目追加やバージョン管理が楽になる。
        /// </summary>
        [Serializable]
        private sealed class SaveData
        {
            public int version = SaveVersion;

            public int money;
            public int remainingDays;
            public MonsterSaveData currentMonster;
            public RaceAwardSaveData[] awards;
            public TrophyOwnershipSaveData[] trophyOwnerships;
            public CourseRecordSaveData[] courseRecords;
            public SettingsData settings;
        }

        [Serializable]
        private sealed class RaceAwardSaveData
        {
            public string raceId;
            public TrophyOwnershipSaveData trophyOwnership;
            public CourseRecordSaveData courseRecord;
        }

        [Serializable]
        private sealed class TrophyOwnershipSaveData
        {
            public string trophyId;
            public string raceId;
            public int medal;
            public string winnerMonsterId;
        }

        [Serializable]
        private sealed class CourseRecordSaveData
        {
            public string raceId;
            public string bestTimeMmSsCc;
            public string holderMonsterId;
        }

        [Serializable]
        private sealed class MonsterSaveData
        {
            public string monsterId;
            public string speciesId;
            public string nickname;
            public int experience;

            public MonsterStatsSaveData[] growthIncrements;
            public SkillSaveData[] monsterSkills;
            public string[] parentMonsterIds;
        }

        [Serializable]
        private sealed class MonsterStatsSaveData
        {
            public int topSpeed;
            public int accel;
            public int stamina;
        }

        [Serializable]
        private sealed class SkillSaveData
        {
            public string skillId;
            public int unlockLevel;
        }

        public bool Exists()
        {
            return File.Exists(FilePath) || File.Exists(BackupPath);
        }

        public void Delete()
        {
            TryDelete(FilePath);
            TryDelete(TempPath);
            TryDelete(BackupPath);
        }

        public void Save(WorldState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (state.CurrentMonster == null)
                throw new InvalidOperationException("CurrentMonster が null の状態は保存できません。");

            var currentMonster = state.CurrentMonster;
            // Domain -> DTO
            var data = new SaveData
            {
                version = SaveVersion,
                money = state.Money,
                currentMonster = new MonsterSaveData
                {
                    monsterId = currentMonster.MonsterId.Value,
                    speciesId = currentMonster.SpeciesId.Value,
                    nickname = currentMonster.Nickname,
                    experience = currentMonster.Experience.Value,
                    growthIncrements = ToStatsSaveDataArray(currentMonster.GrowthIncrements.Entries),
                    monsterSkills = ToMonsterSkillSaveData(currentMonster.MonsterSkills),
                    parentMonsterIds = ToMonsterIdValues(currentMonster.ParentMonsterIds),
                },
                awards = ToRaceAwardSaveDataArray(state.AwardEntries),
                trophyOwnerships = ToTrophyOwnershipSaveDataArray(state.TrophyOwnerships),
                courseRecords = ToCourseRecordSaveDataArray(state.CourseRecords),
                settings = state.Settings,
            };

            var json = JsonUtility.ToJson(data, prettyPrint: true);

            // ---- 破損対策：tempに書いてから置換する ----
            Directory.CreateDirectory(UnityEngine.Application.persistentDataPath);

            File.WriteAllText(TempPath, json);

            // 既存ファイルがあるならバックアップに退避
            if (File.Exists(FilePath))
            {
                TryDelete(BackupPath);
                File.Move(FilePath, BackupPath);
            }

            // tempを本番へ
            TryDelete(FilePath);
            File.Move(TempPath, FilePath);
        }

        public bool TryLoad(out WorldState state)
        {
            // まず本番、ダメならバックアップを読む
            if (TryLoadFrom(FilePath, out state))
                return true;

            if (TryLoadFrom(BackupPath, out state))
                return true;

            state = null;
            return false;
        }

        private bool TryLoadFrom(string path, out WorldState state)
        {
            state = null;

            if (!File.Exists(path))
                return false;

            try
            {
                var json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null)
                    throw new InvalidDataException("SaveData のデシリアライズに失敗しました。");
                if (data.currentMonster == null)
                    throw new InvalidDataException("currentMonster が保存されていません。");

                var monster = BuildMonsterInstance(data.currentMonster);

                // DTO -> Domain（version対応を入れるならここで分岐）
                state = new WorldState(
                    money: data.money,
                    currentMonster: monster,
                    awardEntries: ToDomainRaceAwards(data.awards, data.trophyOwnerships, data.courseRecords),
                    settings: data.settings
                );

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Save load failed ({path}): {e}");
                return false;
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to delete save file '{path}': {ex}");
            }
        }

        private static MonsterInstance BuildMonsterInstance(MonsterSaveData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.growthIncrements == null || data.growthIncrements.Length != GrowthIncrements.LevelCount)
                throw new InvalidDataException($"growthIncrements は {GrowthIncrements.LevelCount} 件必要です。実際: {data.growthIncrements?.Length ?? 0}");
            if (data.monsterSkills == null)
                throw new InvalidDataException("monsterSkills が未設定です。");
            if (data.parentMonsterIds == null)
                throw new InvalidDataException("parentMonsterIds が未設定です。");

            var increments = new List<MonsterStats>(data.growthIncrements.Length);
            for (int i = 0; i < data.growthIncrements.Length; i++)
            {
                increments.Add(ToMonsterStats(data.growthIncrements[i]));
            }

            var monsterSkills = new MonsterSkill[data.monsterSkills.Length];
            for (int i = 0; i < data.monsterSkills.Length; i++)
            {
                var monsterSkill = data.monsterSkills[i];
                if (monsterSkill == null) throw new InvalidDataException($"monsterSkills[{i}] が null です。");
                monsterSkills[i] = new MonsterSkill(new SkillId(monsterSkill.skillId), new Level(monsterSkill.unlockLevel));
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

        private static MonsterStats ToMonsterStats(MonsterStatsSaveData data)
        {
            if (data == null)
                throw new InvalidDataException("MonsterStatsSaveData が null です。");

            return new MonsterStats(data.topSpeed, data.accel, data.stamina);
        }

        private static MonsterStatsSaveData ToStatsSaveData(MonsterStats stats)
        {
            return new MonsterStatsSaveData
            {
                topSpeed = stats.TopSpeed,
                accel = stats.Accel,
                stamina = stats.Stamina,
            };
        }

        private static MonsterStatsSaveData[] ToStatsSaveDataArray(IReadOnlyList<MonsterStats> statsList)
        {
            if (statsList == null)
                throw new ArgumentNullException(nameof(statsList));

            var result = new MonsterStatsSaveData[statsList.Count];
            for (int i = 0; i < statsList.Count; i++)
            {
                result[i] = ToStatsSaveData(statsList[i]);
            }

            return result;
        }

        private static SkillSaveData[] ToMonsterSkillSaveData(MonsterSkill[] monsterSkills)
        {
            if (monsterSkills == null)
                throw new ArgumentNullException(nameof(monsterSkills));

            var result = new SkillSaveData[monsterSkills.Length];
            for (int i = 0; i < monsterSkills.Length; i++)
            {
                result[i] = new SkillSaveData
                {
                    skillId = monsterSkills[i].Id.Value,
                    unlockLevel = monsterSkills[i].UnlockLevel.Value,
                };
            }

            return result;
        }

        private static string[] ToMonsterIdValues(MonsterId[] monsterIds)
        {
            if (monsterIds == null)
                throw new ArgumentNullException(nameof(monsterIds));

            var result = new string[monsterIds.Length];
            for (int i = 0; i < monsterIds.Length; i++)
            {
                result[i] = monsterIds[i].Value;
            }

            return result;
        }

        private static TrophyOwnershipSaveData[] ToTrophyOwnershipSaveDataArray(IReadOnlyList<TrophyOwnership> trophyOwnerships)
        {
            if (trophyOwnerships == null)
                throw new ArgumentNullException(nameof(trophyOwnerships));

            var result = new TrophyOwnershipSaveData[trophyOwnerships.Count];
            for (int i = 0; i < trophyOwnerships.Count; i++)
            {
                var item = trophyOwnerships[i];
                result[i] = new TrophyOwnershipSaveData
                {
                    trophyId = item.TrophyId.Value,
                    raceId = item.RaceId,
                    medal = (int)item.Medal,
                    winnerMonsterId = item.WinnerMonsterId.Value,
                };
            }

            return result;
        }

        private static CourseRecordSaveData[] ToCourseRecordSaveDataArray(IReadOnlyList<CourseRecord> courseRecords)
        {
            if (courseRecords == null)
                throw new ArgumentNullException(nameof(courseRecords));

            var result = new CourseRecordSaveData[courseRecords.Count];
            for (int i = 0; i < courseRecords.Count; i++)
            {
                var item = courseRecords[i];
                result[i] = new CourseRecordSaveData
                {
                    raceId = item.RaceId,
                    bestTimeMmSsCc = ToCompactTime(item.Time.Value),
                    holderMonsterId = item.HolderMonsterId.Value,
                };
            }

            return result;
        }

        private static RaceAwardSaveData[] ToRaceAwardSaveDataArray(IReadOnlyList<RaceAwardState> awardEntries)
        {
            if (awardEntries == null)
                throw new ArgumentNullException(nameof(awardEntries));

            var result = new RaceAwardSaveData[awardEntries.Count];
            for (int i = 0; i < awardEntries.Count; i++)
            {
                var item = awardEntries[i];
                result[i] = new RaceAwardSaveData
                {
                    raceId = item.RaceId,
                    trophyOwnership = item.TrophyOwnership != null
                        ? new TrophyOwnershipSaveData
                        {
                            trophyId = item.TrophyOwnership.TrophyId.Value,
                            raceId = item.TrophyOwnership.RaceId,
                            medal = (int)item.TrophyOwnership.Medal,
                            winnerMonsterId = item.TrophyOwnership.WinnerMonsterId.Value,
                        }
                        : null,
                    courseRecord = item.CourseRecord != null
                        ? new CourseRecordSaveData
                        {
                            raceId = item.CourseRecord.RaceId,
                            bestTimeMmSsCc = ToCompactTime(item.CourseRecord.Time.Value),
                            holderMonsterId = item.CourseRecord.HolderMonsterId.Value,
                        }
                        : null,
                };
            }

            return result;
        }

        private static RaceAwardState[] ToDomainRaceAwards(
            RaceAwardSaveData[] awards,
            TrophyOwnershipSaveData[] trophyOwnerships,
            CourseRecordSaveData[] courseRecords)
        {
            if (awards != null)
            {
                var result = new List<RaceAwardState>(awards.Length);
                for (int i = 0; i < awards.Length; i++)
                {
                    var item = awards[i];
                    if (item == null || string.IsNullOrWhiteSpace(item.raceId))
                        continue;

                    result.Add(new RaceAwardState(
                        item.raceId,
                        item.trophyOwnership != null ? ToDomainTrophyOwnership(item.trophyOwnership) : null,
                        item.courseRecord != null ? ToDomainCourseRecord(item.courseRecord) : null));
                }

                return result.ToArray();
            }

            var trophiesByRaceId = new Dictionary<string, TrophyOwnership>();
            var recordsByRaceId = new Dictionary<string, CourseRecord>();

            var trophies = ToDomainTrophyOwnerships(trophyOwnerships);
            for (int i = 0; i < trophies.Length; i++)
            {
                trophiesByRaceId[trophies[i].RaceId] = trophies[i];
            }

            var records = ToDomainCourseRecords(courseRecords);
            for (int i = 0; i < records.Length; i++)
            {
                recordsByRaceId[records[i].RaceId] = records[i];
            }

            var merged = new List<RaceAwardState>();
            foreach (var trophy in trophiesByRaceId.Values)
            {
                recordsByRaceId.TryGetValue(trophy.RaceId, out var record);
                merged.Add(new RaceAwardState(trophy.RaceId, trophy, record));
            }

            foreach (var record in recordsByRaceId.Values)
            {
                if (trophiesByRaceId.ContainsKey(record.RaceId))
                    continue;

                merged.Add(new RaceAwardState(record.RaceId, null, record));
            }

            return merged.ToArray();
        }

        private static TrophyOwnership ToDomainTrophyOwnership(TrophyOwnershipSaveData item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return new TrophyOwnership(
                new TrophyId(item.trophyId),
                item.raceId,
                (TrophyMedal)item.medal,
                new MonsterId(item.winnerMonsterId));
        }

        private static CourseRecord ToDomainCourseRecord(CourseRecordSaveData item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!TryParseCompactTimeToMilliseconds(item.bestTimeMmSsCc, out var resolvedMs))
                throw new InvalidDataException($"courseRecord.bestTimeMmSsCc の形式が不正です: '{item.bestTimeMmSsCc}'");

            return new CourseRecord(
                item.raceId,
                new RaceTimeMs(resolvedMs),
                new MonsterId(item.holderMonsterId));
        }

        private static TrophyOwnership[] ToDomainTrophyOwnerships(TrophyOwnershipSaveData[] trophyOwnerships)
        {
            if (trophyOwnerships == null)
                return Array.Empty<TrophyOwnership>();

            var result = new List<TrophyOwnership>(trophyOwnerships.Length);
            for (int i = 0; i < trophyOwnerships.Length; i++)
            {
                var item = trophyOwnerships[i];
                if (item == null)
                    throw new InvalidDataException($"trophyOwnerships[{i}] が null です。");

                if (string.IsNullOrWhiteSpace(item.raceId))
                    continue;

                if (item.medal <= (int)TrophyMedal.None || item.medal > (int)TrophyMedal.Bronze)
                    continue;

                result.Add(new TrophyOwnership(
                    new TrophyId(item.trophyId),
                    item.raceId,
                    (TrophyMedal)item.medal,
                    new MonsterId(item.winnerMonsterId)));
            }

            return result.ToArray();
        }

        private static CourseRecord[] ToDomainCourseRecords(CourseRecordSaveData[] courseRecords)
        {
            if (courseRecords == null)
                return Array.Empty<CourseRecord>();

            var result = new List<CourseRecord>(courseRecords.Length);
            for (int i = 0; i < courseRecords.Length; i++)
            {
                var item = courseRecords[i];
                if (item == null)
                    throw new InvalidDataException($"courseRecords[{i}] が null です。");

                if (string.IsNullOrWhiteSpace(item.raceId))
                    continue;

                if (!TryParseCompactTimeToMilliseconds(item.bestTimeMmSsCc, out var resolvedMs))
                    continue;

                if (resolvedMs <= 0)
                    continue;

                result.Add(new CourseRecord(
                    item.raceId,
                    new RaceTimeMs(resolvedMs),
                    new MonsterId(item.holderMonsterId)));
            }

            return result.ToArray();
        }
        private static bool TryParseCompactTimeToMilliseconds(string text, out int milliseconds)
        {
            milliseconds = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

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

        private static string ToCompactTime(int milliseconds)
        {
            var clampedMs = Mathf.Clamp(milliseconds, 0, (59 * 60 * 1000) + (59 * 1000) + 990);
            var minutes = clampedMs / 60000;
            var seconds = (clampedMs % 60000) / 1000;
            var centiseconds = (clampedMs % 1000) / 10;
            return string.Format(CultureInfo.InvariantCulture, "{0:00}{1:00}{2:00}", minutes, seconds, centiseconds);
        }
    }
}