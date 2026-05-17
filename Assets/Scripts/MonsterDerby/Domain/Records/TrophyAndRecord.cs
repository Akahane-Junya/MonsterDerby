namespace MonsterDerby.Domain.Records
{
    using System;
    using MonsterDerby.Domain.SharedKernel;

    public enum TrophyMedal
    {
        None = 0,
        Gold = 1,
        Silver = 2,
        Bronze = 3,
    }

    public readonly struct TrophyId
    {
        public string Value { get; }
        public TrophyId(string value) { Value = Guard.NotNullOrEmpty(value, nameof(value)); }
        public override string ToString() => Value;
    }

    public sealed class TrophyOwnership
    {
        public TrophyId TrophyId { get; }
        public string RaceId { get; }
        public TrophyMedal Medal { get; }
        public MonsterId WinnerMonsterId { get; }

        public TrophyOwnership(TrophyId trophyId, string raceId, TrophyMedal medal, MonsterId winnerMonsterId)
        {
            TrophyId = trophyId;
            RaceId = Guard.NotNullOrEmpty(raceId, nameof(raceId));
            Medal = medal;
            WinnerMonsterId = winnerMonsterId;
        }
    }

    public readonly struct RaceTimeMs
    {
        public int Value { get; }
        public RaceTimeMs(int value) { Guard.Positive(value, nameof(value)); Value = value; }
        public override string ToString() => $"{Value}ms";
    }

    public sealed class CourseRecord
    {
        public string RaceId { get; }
        public RaceTimeMs Time { get; }
        public MonsterId HolderMonsterId { get; }

        public CourseRecord(string raceId, RaceTimeMs time, MonsterId holderMonsterId)
        {
            RaceId = Guard.NotNullOrEmpty(raceId, nameof(raceId));
            Time = time;
            HolderMonsterId = holderMonsterId;
        }
    }

    public sealed class RaceAwardState
    {
        public string RaceId { get; }
        public TrophyOwnership TrophyOwnership { get; }
        public CourseRecord CourseRecord { get; }

        public RaceAwardState(string raceId, TrophyOwnership trophyOwnership, CourseRecord courseRecord)
        {
            RaceId = Guard.NotNullOrEmpty(raceId, nameof(raceId));
            TrophyOwnership = trophyOwnership;
            CourseRecord = courseRecord;
        }

        public RaceAwardState WithTrophyOwnership(TrophyOwnership trophyOwnership)
        {
            return new RaceAwardState(RaceId, trophyOwnership, CourseRecord);
        }

        public RaceAwardState WithCourseRecord(CourseRecord courseRecord)
        {
            return new RaceAwardState(RaceId, TrophyOwnership, courseRecord);
        }
    }
}
