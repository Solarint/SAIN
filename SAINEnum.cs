namespace SAIN;

public enum ECombatDecision
{
    None,
    Retreat,
    Search,
    RunAway,
    DogFight,
    SeekCover,
    StandAndShoot,
    ThrowGrenade,
    ShiftCover,
    RushEnemy,
    MoveToEngage,
    ShootDistantEnemy,
    AvoidGrenade,
    Freeze,
    CreepOnEnemy,
    MeleeAttack,
    FightZombies,
    DebugNoDecision
}

public enum ESelfActionType
{
    None = 0,
    Reload = 1,
    FirstAid = 2,
    Stims = 3,
    Surgery = 4,
}

public enum FriendlyFireStatus
{
    None,
    FriendlyBlock,
    Clear,
}

public enum EHeardFromPeaceBehavior
{
    None,
    Freeze,
    SearchNow,
    Charge,
}

public enum EWeaponClass
{
    Default,
    assaultRifle,
    assaultCarbine,
    machinegun,
    smg,
    pistol,
    marksmanRifle,
    sniperRifle,
    shotgun,
    grenadeLauncher,
    specialWeapon
}

public enum ECaliber
{
    Default,
    Caliber9x18PM,
    Caliber9x19PARA,
    Caliber46x30,
    Caliber9x21,
    Caliber57x28,
    Caliber762x25TT,
    Caliber1143x23ACP,
    Caliber9x33R,
    Caliber545x39,
    Caliber556x45NATO,
    Caliber9x39,
    Caliber762x35,
    Caliber762x39,
    Caliber366TKM,
    Caliber762x51,
    Caliber127x55,
    Caliber762x54R,
    Caliber86x70,
    Caliber20g,
    Caliber12g,
    Caliber23x75,
    Caliber26x75,
    Caliber30x29,
    Caliber40x46,
    Caliber40mmRU,
    Caliber127x108,
    Caliber68x51,
    Caliber20x1,
    Caliber127x33,
    Caliber9x18PMM
}

public enum CoverStatus
{
    None = 0,
    FarFromCover = 1,
    MidRangeToCover = 2,
    CloseToCover = 3,
    InCover = 4,
}

public enum LeanSetting
{
    None = 0,
    Left = 1,
    Right = 2,
}

public enum SideStepSetting
{
    None = 0,
    Left = 1,
    Right = 2
}

public enum EPersonality
{
    None,
    Wreckless,
    SnappingTurtle,
    GigaChad,
    Chad,
    Rat,
    Timmy,
    Coward,
    Normal,
    Custom1,
    Custom2,
    Custom3,
    Custom4,
    Custom5,
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

public enum ESquadDecision
{
    None,
    Surround,
    Retreat,
    Suppress,
    PushSuppressedEnemy,
    BoundingRetreat,
    Regroup,
    SpreadOut,
    HoldPositions,
    Help,
    Search,
    GroupSearch,
}

public enum SAINSoundType
{
    None,
    Generic,
    FootStep,
    Sprint,
    Prone,
    Looting,
    Reload,
    GearSound,
    GrenadePin,
    GrenadeExplosion,
    GrenadeDraw,
    Jump,
    Door,
    DoorBreach,
    Shot,
    SuppressedShot,
    Heal,
    Food,
    Conversation,
    Surgery,
    DryFire,
    TurnSound,
    Breathing,
    Pain,
    Bush,
    BulletImpact,
    Land,
}

public enum ELocation
{
    None = 0,
    Factory = 1,
    FactoryNight = 2,
    Customs = 3,
    GroundZero = 4,
    Reserve = 5,
    Streets = 6,
    Lighthouse = 7,
    Shoreline = 8,
    Labs = 9,
    Woods = 10,
    Interchange = 11,
    Terminal = 12,
    Town = 13,
    Labyrinth = 14,
}

public enum EPathDistance
{
    NoEnemy,
    VeryClose,
    Close,
    Mid,
    Far,
    VeryFar,
}

public enum StyleState
{
    normal,
    onNormal,
    active,
    onActive,
    hover,
    onHover,
    focused,
    onFocused,
}

public enum ESoundCleanupReason
{
    None = 0,
    PlayerNull = 1,
    IPlayerNull = 2,
    TooFar = 3,
    TooOld = 4,
    SoundNull = 5,
    Forced = 6,
}

public enum AILimitSetting
{
    None = 0,
    Far = 1,
    VeryFar = 2,
    Narnia = 3,
}