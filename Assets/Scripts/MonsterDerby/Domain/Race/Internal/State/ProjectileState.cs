// MonsterDerby.Domain.Race（C# 9 / クリーン版）
// 方針: Domain の型は「任意」を null で表現しない。
//       任意のコレクションは空配列（Array.Empty<T>()）で表現する。
//       公開コンストラクタは必須入力を検証し、不正なら即例外（Fail-Fast）。

namespace MonsterDerby.Domain.Race
{
    using MonsterDerby.Domain.SharedKernel;
    internal sealed class ProjectileState
    {
        public long ProjectileId { get; }
        public int AttackerLaneIndex { get; }
        public int TargetLaneIndex { get; }
        public SkillId SkillId { get; }

        public float LaunchTimeSeconds { get; }
        public float ImpactTimeSeconds { get; }

        public ProjectileState(
            long projectileId,
            int attackerLaneIndex,
            int targetLaneIndex,
            SkillId skillId,
            float launchTimeSeconds,
            float impactTimeSeconds)
        {
            ProjectileId = projectileId;
            AttackerLaneIndex = attackerLaneIndex;
            TargetLaneIndex = targetLaneIndex;
            SkillId = skillId;
            LaunchTimeSeconds = launchTimeSeconds;
            ImpactTimeSeconds = impactTimeSeconds;
        }
    }
}
