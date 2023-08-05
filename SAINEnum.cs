
namespace SAIN
{
    public enum SoloDecision
    {
        None,
        Retreat,
        Fight,
        RunToCover,
        Search,
        HoldInCover,
        RunAway,
        DogFight,
        Shoot,
        WalkToCover,
        StandAndShoot,
        Investigate,
        ThrowGrenade,
        ShiftCover,
        RushEnemy,
        MoveToEngage,

        UnstuckSearch,
        UnstuckMoveToCover,
        UnstuckDogFight,
    }

    public enum SelfDecision
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
        None,
        Up,
        Right,
    }

    public enum SAINPersonality
    {
        Normal,
        Timmy,
        Coward,
        Rat,
        SweatLord,
        Chad,
        GigaChad
    }

    public enum BotAggression
    {
        Low,
        Normal,
        High,
        VeryHigh
    }

    public enum GrenadeThrowDirection
    {
        None,
        Over,
        Around
    }

    public enum GrenadeThrowType
    {
        None,
        Close,
        Mid,
        Far
    }

    public enum SquadDecision
    {
        None,
        Surround,
        Retreat,
        Suppress,
        BoundingAttack,
        BoundingRetreat,
        Regroup,
        SpreadOut,
        HoldPositions,
        Help,
        Search,
    }

    public enum SAINSoundType
    {
        FootStep,
        Reload,
        Aim,
        GrenadePin,
        Injury,
        Jump,
        Door,
        DoorBreach,
        Gunshot,
        SuppressedGunShot,
        GrenadeDraw,
        None,
        Heal,
    }

    public enum SoundDistanceType
    {
        CloseGun,
        CloseStep,
        FarGun,
        FarStep,
    }
    public enum SAINEnemyPathEnum
    {
        VeryClose,
        Close,
        Mid,
        Far,
        VeryFar,
        NoEnemy,
    }
}
