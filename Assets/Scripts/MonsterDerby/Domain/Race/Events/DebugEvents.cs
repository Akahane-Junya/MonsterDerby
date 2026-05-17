// MonsterDerby.Domain.Race（C# 9 / クリーン版）
// 方針: Domain の型は「任意」を null で表現しない。
//       任意のコレクションは空配列（Array.Empty<T>()）で表現する。
//       公開コンストラクタは必須入力を検証し、不正なら即例外（Fail-Fast）。


namespace MonsterDerby.Domain.Race
{
    using MonsterDerby.Domain.SharedKernel;

    public abstract class DebugEvent
    {
        public float TimeSeconds { get; }

        protected DebugEvent(float timeSeconds)
        {
            Guard.NonNegative(timeSeconds, nameof(timeSeconds));
            TimeSeconds = timeSeconds;
        }
    }

    public sealed class AttackResolvedAtLaunchDebugEvent : DebugEvent
    {
        public int AttackerLaneIndex { get; }
        public SkillId SkillId { get; }
        public int TargetLaneIndex { get; }
        public float RandomRoll01 { get; }
        public float HitChanceThreshold01 { get; }
        public bool WillHit { get; }
        public float ImpactTimeSeconds { get; }

        public AttackResolvedAtLaunchDebugEvent(
            float timeSeconds,
            int attackerLaneIndex,
            SkillId skillId,
            int targetLaneIndex,
            float randomRoll01,
            float hitChanceThreshold01,
            bool willHit,
            float impactTimeSeconds)
            : base(timeSeconds)
        {
            AttackerLaneIndex = attackerLaneIndex;
            SkillId = skillId;
            TargetLaneIndex = targetLaneIndex;
            RandomRoll01 = randomRoll01;
            HitChanceThreshold01 = hitChanceThreshold01;
            WillHit = willHit;
            ImpactTimeSeconds = impactTimeSeconds;
        }
    }
}
