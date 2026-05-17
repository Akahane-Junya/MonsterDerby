using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Domain.Skill;

namespace MonsterDerby.Infrastructure.MasterData
{
    internal static class SkillPresetFactory
    {
        public static bool TryCreatePreset(SkillDefinitionSO source, SkillId id, string resolvedName, out SkillDefinition definition)
        {
            definition = null;

            switch (id.Value)
            {
                case "GrasslandAdept":
                    definition = new PassiveTerrainSkillDefinition(
                        id,
                        resolvedName,
                        "grassland",
                        new[]
                        {
                            new EffectDefinition(new SpeedMultiplierEffect(1.2f), 0f)
                        });
                    return true;

                case "CaveAdept":
                    definition = new PassiveTerrainSkillDefinition(
                        id,
                        resolvedName,
                        "cave",
                        new[]
                        {
                            new EffectDefinition(new AccelerationMultiplierEffect(1.2f), 0f)
                        });
                    return true;

                case "IceAdept":
                    definition = new PassiveTerrainSkillDefinition(
                        id,
                        resolvedName,
                        "ice",
                        new[]
                        {
                            new EffectDefinition(new StaminaDrainMultiplierEffect(0f), 0f)
                        });
                    return true;

                case "MagmaAdept":
                    definition = new PassiveTerrainSkillDefinition(
                        id,
                        resolvedName,
                        "magma",
                        new[]
                        {
                            new EffectDefinition(new StaminaDrainMultiplierEffect(0f), 0f)
                        });
                    return true;

                case "Fireball":
                    definition = BuildActiveAttack(id, resolvedName, source.cooldownSeconds > 0f ? source.cooldownSeconds : 5f, source.hitChance01, new TargetingDefinition(TargetingMode.RandomOne, 1),
                        new[]
                        {
                            new EffectDefinition(new SlowMultiplierEffect(0.8f), 1.5f, EffectTargetSide.Target)
                        });
                    return true;

                case "IceBall":
                    definition = BuildActiveAttack(id, resolvedName, source.cooldownSeconds > 0f ? source.cooldownSeconds : 6f, source.hitChance01, new TargetingDefinition(TargetingMode.RandomOne, 1),
                        new[]
                        {
                            new EffectDefinition(new SlowMultiplierEffect(0.75f), 2.0f, EffectTargetSide.Target),
                            new EffectDefinition(new StaminaDrainMultiplierEffect(1.25f), 2.0f, EffectTargetSide.Target)
                        });
                    return true;

                case "Caltrops":
                    definition = BuildActiveAttack(id, resolvedName, source.cooldownSeconds > 0f ? source.cooldownSeconds : 4f, source.hitChance01, new TargetingDefinition(TargetingMode.All, 0),
                        new[]
                        {
                            new EffectDefinition(new SlowMultiplierEffect(0.85f), 1.5f, EffectTargetSide.Target)
                        });
                    return true;

                case "Flight":
                    definition = BuildActiveAttack(id, resolvedName, source.cooldownSeconds > 0f ? source.cooldownSeconds : 8f, source.hitChance01, new TargetingDefinition(TargetingMode.RandomOne, 1),
                        new[]
                        {
                            new EffectDefinition(new SpeedMultiplierEffect(1.05f), 2.0f, EffectTargetSide.Target)
                        });
                    return true;

                default:
                    return false;
            }
        }

        public static ActiveAttackSkillDefinition BuildActiveAttack(
            SkillId id,
            string resolvedName,
            float cooldownSeconds,
            float hitChance01,
            TargetingDefinition targeting,
            EffectDefinition[] effects)
        {
            return new ActiveAttackSkillDefinition(
                id,
                resolvedName,
                cooldownSeconds,
                new RangeDefinition(RangeDirection.Both, 999f),
                targeting,
                new HitModel(hitChance01),
                new ProjectileDefinition(TravelTimeModel.ProportionalToDistance),
                effects);
        }
    }
}
