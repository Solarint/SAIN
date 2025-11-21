using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections.Generic;

namespace SAIN.SAINComponent.Classes.Decision;

//public struct FBotDecisionDataData
//{
//    public ECombatDecision CurrentCombatDecision;
//    public ECombatDecision PreviousCombatDecision;
//    public ESquadDecision CurrentSquadDecision;
//    public ESquadDecision PreviousSquadDecision;
//    public ESelfDecision CurrentSelfDecision;
//    public ESelfDecision PreviousSelfDecision;
//    public bool HasDecision;
//    public float ChangeDecisionTime;
//    public float TimeSinceChangeDecision;
//
//    public ETagStatus HealthStatus;
//}

public struct FSainBotDecision
{
    public Enemy Enemy;
    public ECombatDecision CombatDecision;
    public ESquadDecision SquadDecision;
    public ESelfActionType SelfAction;
    public float TimeDecisionMade;

    public override bool Equals(object obj)
    {
        if (obj is FSainBotDecision other)
        {
            return Equals(other);
        }
        return false;
    }

    public bool Equals(FSainBotDecision other)
    {
        return Enemy?.EnemyProfileId == other.Enemy?.EnemyProfileId
            && CombatDecision == other.CombatDecision
            && SquadDecision == other.SquadDecision
            && SelfAction == other.SelfAction;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (Enemy != null ? Enemy.GetHashCode() : 0);
            hash = hash * 23 + CombatDecision.GetHashCode();
            hash = hash * 23 + SquadDecision.GetHashCode();
            hash = hash * 23 + SelfAction.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(FSainBotDecision left, FSainBotDecision right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FSainBotDecision left, FSainBotDecision right)
    {
        return !(left == right);
    }
}