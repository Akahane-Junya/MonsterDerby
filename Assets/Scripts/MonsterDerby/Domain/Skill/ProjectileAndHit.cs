namespace MonsterDerby.Domain.Skill
{
    public sealed class ProjectileDefinition
    {
        public TravelTimeModel TravelTimeModel { get; }

        public ProjectileDefinition(TravelTimeModel travelTimeModel)
        {
            TravelTimeModel = travelTimeModel;
        }
    }

    public sealed class HitModel
    {
        public float HitChance01 { get; }

        public HitModel(float hitChance01)
        {
            // 許容範囲は 0..1
            if (hitChance01 < 0f || hitChance01 > 1f)
                throw new System.ArgumentOutOfRangeException(nameof(hitChance01), hitChance01, "Must be within [0,1].");
            HitChance01 = hitChance01;
        }
    }
}