using MonsterDerby.Domain.Monster;
using MonsterDerby.Domain.Records;
using MonsterDerby.Domain.SharedKernel;
using System;
using System.Collections.Generic;

namespace MonsterDerby.Domain.World
{
    /// <summary>
    /// ゲーム全体の永続状態。
    /// </summary>

    public sealed class WorldState
    {
        public int Money { get; }
        public MonsterInstance CurrentMonster { get; }
        public IReadOnlyList<RaceAwardState> AwardEntries { get; }
        public IReadOnlyList<TrophyOwnership> TrophyOwnerships { get; }
        public IReadOnlyList<CourseRecord> CourseRecords { get; }
        public SettingsData Settings { get; }


        public WorldState(int money, MonsterInstance currentMonster, SettingsData settings = null)
            : this(money, currentMonster, Array.Empty<RaceAwardState>(), settings)
        {
        }

        public WorldState(
            int money,
            MonsterInstance currentMonster,
            IReadOnlyList<RaceAwardState> awardEntries,
            SettingsData settings = null)
        {
            Money = money;
            CurrentMonster = currentMonster;
            AwardEntries = awardEntries ?? throw new ArgumentNullException(nameof(awardEntries));
            TrophyOwnerships = ProjectTrophyOwnerships(AwardEntries);
            CourseRecords = ProjectCourseRecords(AwardEntries);
            Settings = settings ?? new SettingsData();
        }

        /// <summary>
        /// 不変オブジェクトとして新しい状態を返す
        /// </summary>

        public WorldState With(int money, MonsterInstance currentMonster)
        {
            return new WorldState(
                money: money,
                currentMonster: currentMonster,
                awardEntries: AwardEntries,
                settings: Settings
            );
        }

        public WorldState WithRaceOutcome(
            string raceId,
            MonsterId playerRaceMonsterId,
            MonsterId[] finishOrder,
            IReadOnlyDictionary<MonsterId, float> finishTimeSecondsByMonsterId)
        {
            if (string.IsNullOrWhiteSpace(raceId))
                throw new ArgumentException("raceId は必須です。", nameof(raceId));
            if (finishOrder == null)
                throw new ArgumentNullException(nameof(finishOrder));
            if (finishTimeSecondsByMonsterId == null)
                throw new ArgumentNullException(nameof(finishTimeSecondsByMonsterId));

            // プレイヤー参加IDが無い場合は、アワード情報を更新しない
            if (string.IsNullOrWhiteSpace(playerRaceMonsterId.Value))
            {
                return new WorldState(
                    money: Money,
                    currentMonster: CurrentMonster,
                    awardEntries: AwardEntries);
            }

            var updatedAwards = new List<RaceAwardState>(AwardEntries);
            var playerRank = -1;
            for (int i = 0; i < finishOrder.Length; i++)
            {
                if (finishOrder[i] == playerRaceMonsterId)
                {
                    playerRank = i + 1;
                    break;
                }
            }

            var awardIndex = FindAwardIndex(updatedAwards, raceId);
            if (awardIndex < 0)
            {
                updatedAwards.Add(new RaceAwardState(raceId, null, null));
                awardIndex = updatedAwards.Count - 1;
            }

            // トロフィーは「最高順位のみ」保持（raceIdごと1件）
            if (playerRank > 0 && playerRank <= 3)
            {
                var medal = playerRank == 1 ? TrophyMedal.Gold : (playerRank == 2 ? TrophyMedal.Silver : TrophyMedal.Bronze);
                var updatedTrophy = new TrophyOwnership(
                    new TrophyId($"{raceId}_{medal}"),
                    raceId,
                    medal,
                    playerRaceMonsterId);

                var currentAward = updatedAwards[awardIndex];
                if (currentAward.TrophyOwnership == null || IsBetterMedal(medal, currentAward.TrophyOwnership.Medal))
                {
                    updatedAwards[awardIndex] = currentAward.WithTrophyOwnership(updatedTrophy);
                }
            }

            // レコードは「自分が1位のときのみ」更新
            if (finishOrder.Length > 0 && finishOrder[0] == playerRaceMonsterId && finishTimeSecondsByMonsterId.TryGetValue(playerRaceMonsterId, out var winnerTimeSeconds))
            {
                var bestTimeMsValue = (int)Math.Round(winnerTimeSeconds * 1000f);
                if (bestTimeMsValue <= 0) bestTimeMsValue = 1;

                var bestTime = new RaceTimeMs(bestTimeMsValue);
                var updatedRecord = new CourseRecord(raceId, bestTime, playerRaceMonsterId);
                var currentAward = updatedAwards[awardIndex];
                if (currentAward.CourseRecord == null || bestTime.Value < currentAward.CourseRecord.Time.Value)
                {
                    updatedAwards[awardIndex] = currentAward.WithCourseRecord(updatedRecord);
                }
            }

            return new WorldState(
                money: Money,
                currentMonster: CurrentMonster,
                awardEntries: updatedAwards);
        }

        private static int FindAwardIndex(List<RaceAwardState> awards, string raceId)
        {
            for (int i = 0; i < awards.Count; i++)
            {
                if (string.Equals(awards[i].RaceId, raceId, StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }

        private static bool IsBetterMedal(TrophyMedal candidate, TrophyMedal current)
        {
            return MedalPriority(candidate) < MedalPriority(current);
        }

        private static int MedalPriority(TrophyMedal medal)
        {
            switch (medal)
            {
                case TrophyMedal.Gold:
                    return 1;
                case TrophyMedal.Silver:
                    return 2;
                case TrophyMedal.Bronze:
                    return 3;
                default:
                    return 4;
            }
        }

        private static IReadOnlyList<TrophyOwnership> ProjectTrophyOwnerships(IReadOnlyList<RaceAwardState> awards)
        {
            var result = new List<TrophyOwnership>();
            for (int i = 0; i < awards.Count; i++)
            {
                var trophy = awards[i].TrophyOwnership;
                if (trophy != null)
                    result.Add(trophy);
            }

            return result;
        }

        private static IReadOnlyList<CourseRecord> ProjectCourseRecords(IReadOnlyList<RaceAwardState> awards)
        {
            var result = new List<CourseRecord>();
            for (int i = 0; i < awards.Count; i++)
            {
                var record = awards[i].CourseRecord;
                if (record != null)
                    result.Add(record);
            }

            return result;
        }
    }
}
