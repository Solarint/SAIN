
namespace SAIN
{
    public enum SAINLogicDecision
    {
        None = 0,
        Surgery = 1,
        Reload = 2,
        Fight = 3,
        RunForCover = 4,
        Search = 5,
        HoldInCover = 6,
        RunAway = 7,
        FirstAid = 8,
        Suppress = 9,
        DogFight = 10,
        Skirmish = 11,
        Stims = 12,
        MoveToCover = 13,
        RunAwayGrenade = 14,
    }

    public enum CoverStatus
    {
        None = 0,
        InCover = 1,
        FarFromCover = 3,
        CloseToCover = 4,
        MidRangeToCover = 5,
    }

    public enum LeanSetting
    {
        None = 0,
        Left = 1,
        HalfLeft = 2,
        Right = 3,
        HalfRight = 4,
    }

    public enum SideStepSetting
    {
        None = 0,
        Left = 1,
        Right = 2
    }

    public enum BlindFireSetting
    {
        None = 0,
        Up = 1,
        Right = 2,
    }

    public enum BotPersonality
    {
        None = 0,
        Timmy = 1,
        Coward = 2,
        Rat = 3,
        SweatLord = 4,
        Chad = 5,
        GigaChad = 6
    }

    public enum BotAggression
    {
        Low = 0,
        Normal = 1,
        High = 2,
        VeryHigh = 3
    }

    public enum GrenadeThrowDirection
    {
        None = 0,
        Over = 1,
        Around = 2
    }

    public enum GrenadeThrowType
    {
        None = 0,
        Close = 1,
        Mid = 2,
        Far = 3
    }

    public enum SAINSquadDecisions
    {
        None = 0,
        Surround = 1,
        Retreat = 2,
        Suppress = 3,
        BoundingAttack = 4,
        BoundingRetreat = 5,
        Regroup = 6,
        SpreadOut = 7,
        HoldPositions = 8
    }
}
