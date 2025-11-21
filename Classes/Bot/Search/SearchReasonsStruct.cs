namespace SAIN.SAINComponent.Classes.Search;

public struct SearchReasonsStruct
{
    public WantSearchReasonsStruct WantSearchReasons;
    public ENotSearchReason NotSearchReason;
    public string PathCalcFailReason;

    public enum ENotSearchReason
    {
        None,
        DontWantTo,
        PathCalcFailed,
        NullTargetPlace,
    }

    public struct WantSearchReasonsStruct
    {
        public ENotWantToSearchReason NotWantToSearchReason;
        public EWantToSearchReason WantToSearchReason;
        public ECantStartReason CantStartReason;
    }

    public enum ENotWantToSearchReason
    {
        None,
        NullEnemy,
        NullLastKnown,
        EnemyNotSeenOrHeard,
        WontSearchFromAudio,
        CantStart,
        ShallNotSearch,
        AlreadySearchedLastKnown,
    }

    public enum EWantToSearchReason
    {
        None,
        HeardFromPeaceSearchNow,
        BeingStealthy,
        NewSearch_Looting,
        NewSearch_PowerLevel,
        NewSearch_EnemyNotSeen,
        NewSearch_EnemyNotSeen_Squad,
        NewSearch_EnemyNotHeard,
        NewSearch_EnemyNotHeard_Squad,
        ContinueSearch_Looting,
        ContinueSearch_PowerLevel,
        ContinueSearch_EnemyNotSeen_Personal,
        ContinueSearch_EnemyNotSeen_Squad,
        ContinueSearch_EnemyNotHeard,
        ContinueSearch_EnemyNotHeard_Squad,
        ContinueSearch_EnemyVisible,
    }

    public enum ECantStartReason
    {
        None,
        Suppressed,
        EnemyVisible,
        WontSearchForEnemy,
    }
}