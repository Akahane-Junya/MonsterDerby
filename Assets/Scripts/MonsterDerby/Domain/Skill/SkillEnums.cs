namespace MonsterDerby.Domain.Skill
{
    public enum SkillCategory
    {
        PassiveTerrain,
        PassiveCondition,
        ActiveAttack
    }

    public enum RangeDirection
    {
        Front,
        Back,
        Both
    }

    public enum TargetingMode
    {
        Nearest,
        RandomOne,
        All
    }

    public enum TravelTimeModel
    {
        ProportionalToDistance
    }
}
