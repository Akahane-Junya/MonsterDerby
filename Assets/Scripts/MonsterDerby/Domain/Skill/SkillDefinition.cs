// MonsterDerby.Domain.Race（C# 9 / クリーン版）
// 方針: Domain の型は「任意」を null で表現しない。
//       任意のコレクションは空配列（Array.Empty<T>()）で表現する。
//       公開コンストラクタは必須入力を検証し、不正なら即例外（Fail-Fast）。


namespace MonsterDerby.Domain.Skill
{
    using System;
    using MonsterDerby.Domain.SharedKernel;

    public abstract class SkillDefinition
    {
        // 設計メモ:
        //  - 「optionalフィールド（nullable）」を避けるため、サブタイプで表現する。
        //  - これにより「HitModel（命中モデル）なしの ActiveAttack（攻撃スキル）」など不正な組み合わせを防ぐ。

        public SkillId SkillId { get; }
        public string Name { get; }
        public abstract SkillCategory Category { get; }

        protected SkillDefinition(SkillId skillId, string name)
        {
            SkillId = skillId;
            Name = Guard.NotNullOrEmpty(name, nameof(name));
        }
    }

    public sealed class PassiveTerrainSkillDefinition : SkillDefinition
    {
        public override SkillCategory Category => SkillCategory.PassiveTerrain;
        public string TerrainTag { get; }
        public EffectDefinition[] Effects { get; }

        public PassiveTerrainSkillDefinition(SkillId skillId, string name, string terrainTag, EffectDefinition[] effects)
            : base(skillId, name)
        {
            TerrainTag = Guard.NotNullOrEmpty(terrainTag, nameof(terrainTag));
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }
    }

    public sealed class PassiveConditionSkillDefinition : SkillDefinition
    {
        public override SkillCategory Category => SkillCategory.PassiveCondition;
        public TriggerDefinition Trigger { get; }
        public EffectDefinition[] Effects { get; }

        public PassiveConditionSkillDefinition(SkillId skillId, string name, TriggerDefinition trigger, EffectDefinition[] effects)
            : base(skillId, name)
        {
            Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }
    }

    public sealed class ActiveAttackSkillDefinition : SkillDefinition
    {
        public override SkillCategory Category => SkillCategory.ActiveAttack;

        public float CooldownSeconds { get; }
        public RangeDefinition Range { get; }
        public TargetingDefinition Targeting { get; }
        public HitModel HitModel { get; }
        public ProjectileDefinition Projectile { get; }
        public EffectDefinition[] Effects { get; }

        public ActiveAttackSkillDefinition(
            SkillId skillId,
            string name,
            float cooldownSeconds,
            RangeDefinition range,
            TargetingDefinition targeting,
            HitModel hitModel,
            ProjectileDefinition projectile,
            EffectDefinition[] effects)
            : base(skillId, name)
        {
            Guard.NonNegative(cooldownSeconds, nameof(cooldownSeconds));
            CooldownSeconds = cooldownSeconds;
            Range = range ?? throw new ArgumentNullException(nameof(range));
            Targeting = targeting ?? throw new ArgumentNullException(nameof(targeting));
            HitModel = hitModel ?? throw new ArgumentNullException(nameof(hitModel));
            Projectile = projectile ?? throw new ArgumentNullException(nameof(projectile));
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }
    }
}
