
namespace SAIN
{
    public enum SAINSoloDecision
    {
        None,
        Retreat,
        Fight,
        RunForCover,
        Search,
        HoldInCover,
        RunAway,
        DogFight,
        Shoot,
        MoveToCover,
        StandAndShoot,
        Investigate,

        UnstuckSearch,
        UnstuckMoveToCover,
        UnstuckDogFight,
    }

    public enum SAINSelfDecision
    {
        None = 0,
        Reload = 1,
        RunAway = 2,
        FirstAid = 3,
        RunAwayGrenade = 4,
        Stims = 5,
        Surgery = 6,
    }

    public enum FriendlyFireStatus
    {
        None,
        FriendlyBlock,
        FriendlyClose,
        Clear,
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
        Right = -1,
    }

    public enum BotPersonality
    {
        Normal = 0,
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

    public enum SAINSquadDecision
    {
        None = 0,
        Surround = 1,
        Retreat = 2,
        Suppress = 3,
        BoundingAttack = 4,
        BoundingRetreat = 5,
        Regroup = 6,
        SpreadOut = 7,
        HoldPositions = 8,
        Help = 9,
        Search = 10,
    }

    public enum SAINSoundType
    {
        FootStep = 0,
        Reload = 1,
        Aim = 2,
        GrenadePin = 3,
        Injury = 4,
        Jump = 5,
        Door = 6,
        DoorBreach = 7,
        Gunshot = 8,
        SuppressedGunShot = 9,
    }

    public enum SoundDistanceType
    {
        CloseGun = 0,
        CloseStep = 1,
        FarGun = 2,
        FarStep = 3,
    }
    public enum SAINEnemyPath
    {
        VeryClose = 0,
        Close = 1,
        Mid = 2,
        Far = 3,
        VeryFar = 4,
        NoEnemy = 5,
    }
}
