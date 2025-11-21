using SAIN.Helpers;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;
using static SAIN.SAINComponent.Classes.Search.SearchReasonsStruct;

namespace SAIN.SAINComponent.Classes.Search;

public class SearchDeciderClass : BotSubClass<SAINSearchClass>
{
    public SearchDeciderClass(SAINSearchClass searchClass) : base(searchClass)
    {
        CanEverTick = false;
    }

    public bool ShallStartSearch(Enemy enemy, out SearchReasonsStruct reasons)
    {
        calcSearchTime();
        reasons = new SearchReasonsStruct();

        if (!WantToSearch(enemy, out reasons.WantSearchReasons))
        {
            reasons.NotSearchReason = ENotSearchReason.DontWantTo;
            return false;
        }

        if (enemy.Events.OnSearch.Value)
        {
            if (BaseClass.PathFinder.TargetPlace == null)
            {
                reasons.NotSearchReason = ENotSearchReason.NullTargetPlace;
                return false;
            }
            return true;
        }

        if (!BaseClass.PathFinder.HasPathToSearchTarget(enemy, out string pathCalcFailReason))
        {
            reasons.NotSearchReason = ENotSearchReason.PathCalcFailed;
            reasons.PathCalcFailReason = pathCalcFailReason;
            return false;
        }
        return true;
    }

    private void calcSearchTime()
    {
        if (Bot.Decision.CurrentCombatDecision != ECombatDecision.Search
            && _nextRecalcSearchTime < Time.time)
        {
            _nextRecalcSearchTime = Time.time + 120f;
            Bot.Info.CalcTimeBeforeSearch();
        }
    }

    public bool WantToSearch(Enemy enemy, out WantSearchReasonsStruct reasons)
    {
        reasons = new WantSearchReasonsStruct();
        if (enemy == null)
        {
            reasons.NotWantToSearchReason = ENotWantToSearchReason.NullEnemy;
            return false;
        }
        var lastKnown = enemy.KnownPlaces.LastKnownPlace;
        if (lastKnown == null)
        {
            reasons.NotWantToSearchReason = ENotWantToSearchReason.NullLastKnown;
            return false;
        }
        if (lastKnown.HasArrivedPersonal || lastKnown.HasArrivedSquad)
        {
            reasons.NotWantToSearchReason = ENotWantToSearchReason.AlreadySearchedLastKnown;
            return false;
        }
        if (!enemy.Seen && !Bot.Info.PersonalitySettings.Search.WillSearchFromAudio)
        {
            reasons.NotWantToSearchReason = ENotWantToSearchReason.WontSearchFromAudio;
            return false;
        }
        if (!canStartSearch(enemy, out reasons.CantStartReason))
        {
            reasons.NotWantToSearchReason = ENotWantToSearchReason.CantStart;
            return false;
        }
        if (!shallSearch(enemy, out reasons.WantToSearchReason))
        {
            reasons.NotWantToSearchReason = ENotWantToSearchReason.ShallNotSearch;
            return false;
        }
        reasons.NotWantToSearchReason = ENotWantToSearchReason.None;
        return true;
    }

    private bool shallSearch(Enemy enemy, out EWantToSearchReason reason)
    {
        if (enemy.Hearing.EnemyHeardFromPeace &&
            Bot.Info.PersonalitySettings.Search.HeardFromPeaceBehavior == EHeardFromPeaceBehavior.SearchNow)
        {
            reason = EWantToSearchReason.HeardFromPeaceSearchNow;
            return true;
        }
        if (ShallBeStealthyDuringSearch(enemy) &&
            Bot.Decision.EnemyDecisions.TimeToUnfreeze > Time.time &&
            enemy.TimeSinceLastKnownUpdated > 10f)
        {
            reason = EWantToSearchReason.BeingStealthy;
            return true;
        }

        float timeBeforeSearch = Bot.Info.TimeBeforeSearch;
        if (enemy.Events.OnSearch.Value)
        {
            return shallContinueSearch(enemy, timeBeforeSearch, out reason);
        }
        return shallBeginSearch(enemy, timeBeforeSearch, out reason);
    }

    public bool ShallBeStealthyDuringSearch(Enemy enemy)
    {
        if (!SAINPlugin.LoadedPreset.GlobalSettings.Mind.SneakyBots)
        {
            return false;
        }
        if (SAINPlugin.LoadedPreset.GlobalSettings.Mind.OnlySneakyPersonalitiesSneaky &&
            !Bot.Info.PersonalitySettings.Search.Sneaky)
        {
            return false;
        }
        if (!enemy.Hearing.EnemyHeardFromPeace)
        {
            return false;
        }
        if (Bot.Info.PersonalitySettings.Search.HeardFromPeaceBehavior == EHeardFromPeaceBehavior.SearchNow)
        {
            return false;
        }

        float maxDist = SAINPlugin.LoadedPreset.GlobalSettings.Mind.MaximumDistanceToBeSneaky;
        return enemy.RealDistance < maxDist;
    }

    private bool shallBeginSearchCauseLooting(Enemy enemy)
    {
        if (!enemy.Status.EnemyIsLooting)
        {
            return false;
        }
        if (_nextCheckLootTime < Time.time)
        {
            _nextCheckLootTime = Time.time + _checkLootFreq;
            return EFTMath.RandomBool(_searchLootChance);
        }
        return false;
    }

    private bool shallBeginSearch(Enemy enemy, float timeBeforeSearch, out EWantToSearchReason reason)
    {
        if (shallBeginSearchCauseLooting(enemy))
        {
            enemy.Status.SearchingBecauseLooting = true;
            reason = EWantToSearchReason.NewSearch_Looting;
            return true;
        }
        float myPower = Bot.Info.Profile.PowerLevel;
        if (enemy.EnemyPlayer.AIData.PowerOfEquipment < myPower * 0.5f)
        {
            reason = EWantToSearchReason.NewSearch_PowerLevel;
            return true;
        }
        if (enemy.Seen &&
            enemy.TimeSinceSeen >= timeBeforeSearch)
        {
            reason = EWantToSearchReason.NewSearch_EnemyNotSeen;
            return true;
        }
        var squadSeenPlace = enemy.KnownPlaces.LastSquadSeenPlace;
        if (squadSeenPlace != null &&
            squadSeenPlace.TimeSincePositionUpdated >= timeBeforeSearch)
        {
            reason = EWantToSearchReason.NewSearch_EnemyNotSeen_Squad;
            return true;
        }

        if (Bot.Info.PersonalitySettings.Search.WillSearchFromAudio)
        {
            if (enemy.Heard &&
                enemy.TimeSinceHeard >= timeBeforeSearch)
            {
                reason = EWantToSearchReason.NewSearch_EnemyNotHeard;
                return true;
            }
            var squadHeardPlace = enemy.KnownPlaces.LastSquadHeardPlace;
            if (squadHeardPlace != null &&
                squadHeardPlace.TimeSincePositionUpdated >= timeBeforeSearch)
            {
                reason = EWantToSearchReason.NewSearch_EnemyNotHeard_Squad;
                return true;
            }
        }
        reason = EWantToSearchReason.None;
        return false;
    }

    private bool canStartSearch(Enemy enemy, out ECantStartReason reason)
    {
        var searchSettings = Bot.Info.PersonalitySettings.Search;
        if (!searchSettings.WillSearchForEnemy)
        {
            reason = ECantStartReason.WontSearchForEnemy;
            return false;
        }
        if (Bot.Suppression.IsHeavySuppressed)
        {
            reason = ECantStartReason.Suppressed;
            return false;
        }
        if (enemy.IsVisible)
        {
            reason = ECantStartReason.EnemyVisible;
            //return false;
            return true;
        }
        reason = ECantStartReason.None;
        return true;
    }

    private bool shallContinueSearch(Enemy enemy, float timeBeforeSearch, out EWantToSearchReason reason)
    {
        if (enemy.IsVisible)
        {
            reason = EWantToSearchReason.ContinueSearch_EnemyVisible;
            return true;
        }
        if (enemy.Seen && enemy.TimeSinceSeen < 2f)
        {
            reason = EWantToSearchReason.ContinueSearch_EnemyVisible;
            return true;
        }
        if (enemy.Status.SearchingBecauseLooting)
        {
            reason = EWantToSearchReason.ContinueSearch_Looting;
            return true;
        }

        float myPower = Bot.Info.Profile.PowerLevel;
        if (enemy.EnemyPlayer.AIData.PowerOfEquipment < myPower * 0.5f)
        {
            reason = EWantToSearchReason.ContinueSearch_PowerLevel;
            return true;
        }


        timeBeforeSearch = Mathf.Clamp(timeBeforeSearch / 3f, 0f, 120f);
        if (enemy.Seen && enemy.TimeSinceSeen >= timeBeforeSearch)
        {
            reason = EWantToSearchReason.ContinueSearch_EnemyNotSeen_Personal;
            return true;
        }
        var squadSeenPlace = enemy.KnownPlaces.LastSquadSeenPlace;
        if (squadSeenPlace != null &&
            squadSeenPlace.TimeSincePositionUpdated >= timeBeforeSearch)
        {
            reason = EWantToSearchReason.ContinueSearch_EnemyNotSeen_Squad;
            return true;
        }

        if (Bot.Info.PersonalitySettings.Search.WillSearchFromAudio)
        {
            if (enemy.Heard)
            {
                reason = EWantToSearchReason.ContinueSearch_EnemyNotHeard;
                return true;
            }
            var squadHeardPlace = enemy.KnownPlaces.LastSquadHeardPlace;
            if (squadHeardPlace != null)
            {
                reason = EWantToSearchReason.ContinueSearch_EnemyNotHeard_Squad;
                return true;
            }
        }

        reason = EWantToSearchReason.None;
        return false;
    }

    private float _nextRecalcSearchTime;
    private float _nextCheckLootTime;
    private float _checkLootFreq = 1f;
    private float _searchLootChance = 40f;
}