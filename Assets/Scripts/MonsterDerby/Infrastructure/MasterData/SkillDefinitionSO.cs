// Unity用 MasterData 追加実装
// 目的:
//  - ScriptableObject で編集可能にする
//  - Domain には依存させない（Infrastructure -> Domain 方向のみ）

using UnityEngine;
using MonsterDerby.Domain.Skill;
using MonsterDerby.Domain.SharedKernel;

namespace MonsterDerby.Infrastructure.MasterData
{
    [CreateAssetMenu(menuName = "MonsterDerby/MasterData/Skill")]
    public sealed class SkillDefinitionSO : ScriptableObject
    {
        public enum EffectSide
        {
            Target,
            Attacker,
            Both
        }

        public enum EffectKind
        {
            None,
            SpeedMultiplier,
            AccelerationMultiplier,
            SpeedAdditive,
            SlowMultiplier,
            StaminaDrainMultiplier
        }

        [System.Serializable]
        public sealed class EffectEntry
        {
            public EffectKind kind;
            public float magnitude = 1f;
            public float durationSeconds;
            public EffectSide side = EffectSide.Target;
        }

        [Header("Identity")]
        public string skillId;
        public string skillName;
        public string description;

        [Header("Icon")]
        public Sprite icon;

        public SkillCategory category;

        [Header("ActiveAttack")]
        public float cooldownSeconds;
        [Range(0f,1f)]
        public float hitChance01 = 1f;
        public TargetingMode targetingMode = TargetingMode.RandomOne;
        public int maximumTargets = 1;

        [Header("PassiveTerrain")]
        public string terrainTag;

        [Header("Effects")]
        public EffectEntry[] effects;

        public SkillDefinition ToDomain()
        {
            var id = new SkillId(skillId);
            var resolvedName = string.IsNullOrEmpty(skillName) ? skillId : skillName;

            if (SkillPresetFactory.TryCreatePreset(this, id, resolvedName, out var presetDefinition))
            {
                return presetDefinition;
            }

            var resolvedEffects = BuildEffectDefinitions();

            switch (category)
            {
                case SkillCategory.PassiveTerrain:
                    return new PassiveTerrainSkillDefinition(
                        id,
                        resolvedName,
                        string.IsNullOrEmpty(terrainTag) ? "grassland" : terrainTag,
                        resolvedEffects);

                case SkillCategory.PassiveCondition:
                    return new PassiveConditionSkillDefinition(
                        id,
                        resolvedName,
                        new ConditionTrigger("Always", 0f),
                        resolvedEffects);

                case SkillCategory.ActiveAttack:
                    return SkillPresetFactory.BuildActiveAttack(
                        id,
                        resolvedName,
                        cooldownSeconds > 0f ? cooldownSeconds : 5f,
                        hitChance01,
                        new TargetingDefinition(targetingMode, maximumTargets < 0 ? 0 : maximumTargets),
                        resolvedEffects);

                default:
                    throw new System.NotImplementedException("Category not implemented in SO.");
            }
        }

        private EffectDefinition[] BuildEffectDefinitions()
        {
            if (effects == null || effects.Length == 0)
            {
                return new EffectDefinition[0];
            }

            var built = new System.Collections.Generic.List<EffectDefinition>(effects.Length);
            for (int i = 0; i < effects.Length; i++)
            {
                var effect = BuildEffect(effects[i]);
                if (effect != null)
                {
                    var duration = effects[i] != null ? effects[i].durationSeconds : 0f;
                    built.Add(new EffectDefinition(effect, duration, ToTargetSide(effects[i].side)));
                }
            }

            return built.ToArray();
        }

        private static EffectTargetSide ToTargetSide(EffectSide side)
        {
            switch (side)
            {
                case EffectSide.Attacker:
                    return EffectTargetSide.Attacker;
                case EffectSide.Both:
                    return EffectTargetSide.Both;
                case EffectSide.Target:
                default:
                    return EffectTargetSide.Target;
            }
        }

        private static IEffect BuildEffect(EffectEntry entry)
        {
            if (entry == null) return null;

            switch (entry.kind)
            {
                case EffectKind.SpeedMultiplier:
                    return new SpeedMultiplierEffect(entry.magnitude);
                case EffectKind.AccelerationMultiplier:
                    return new AccelerationMultiplierEffect(entry.magnitude);
                case EffectKind.SpeedAdditive:
                    return new SpeedAdditiveEffect(entry.magnitude);
                case EffectKind.SlowMultiplier:
                    return new SlowMultiplierEffect(entry.magnitude);
                case EffectKind.StaminaDrainMultiplier:
                    return new StaminaDrainMultiplierEffect(entry.magnitude);
                default:
                    return null;
            }
        }
    }
}
