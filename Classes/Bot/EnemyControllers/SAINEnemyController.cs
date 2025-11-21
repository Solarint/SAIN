using EFT;
using SAIN.Components;
using SAIN.Helpers;
using System.Collections.Generic;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class SAINEnemyController : BotComponentClassBase
{
    public override void Init()
    {
        Events.OnEnemyAdded += EnemyAdded;
        Events.OnEnemyRemoved += EnemyRemoved;
        _listController.Init();
        base.Init();
    }

    public Dictionary<string, Enemy> Enemies => _listController.Enemies;
    public HashSet<Enemy> EnemiesArray => _listController.EnemiesArray;

    public EnemyList KnownEnemies { get; private set; } = new("Known Enemies");
    public EnemyList EnemiesInLineOfSight { get; private set; } = new("Enemies In LoS");
    public EnemyList VisibleEnemies { get; private set; } = new("Visible Enemies");

    public EnemyControllerEvents Events { get; }

    public bool AtPeace => Events.OnPeaceChanged.Value && Events.OnPeaceChanged.TimeSinceTrue > 1f;
    public bool ActiveHumanEnemy => Events.ActiveHumanEnemyEvent.Value;
    public bool HumanEnemyInLineofSight => Events.HumanInLineOfSightEvent.Value;

    public SAINEnemyController(BotComponent sain) : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyBotActive;
        _listController = new EnemyListController(this);
        Events = new EnemyControllerEvents(this);
    }

    public override void ManualUpdate()
    {
        UpdateEnemies(Bot, UnityEngine.Time.time);
        _listController.ManualUpdate();
        checkDiscrepency();
        base.ManualUpdate();
    }

    public void LateUpdate()
    {
        //if (Bot == null || Bot.EnemyController == null || !Bot.BotActive)
        //{
        //    return;
        //}
        //
        //foreach (var item in EnemiesDictionary)
        //{
        //    Enemy enemy = item.Value;
        //    if (enemy == null || !enemy.CheckValid())
        //    {
        //        _invalidIdsToRemove.Add(item.Key);
        //    }
        //}
        //
        //if (_invalidIdsToRemove.Count > 0)
        //{
        //    foreach (var id in _invalidIdsToRemove)
        //    {
        //        Bot.EnemyController.RemoveEnemy(id);
        //    }
        //    Logger.LogWarning($"Removed {_invalidIdsToRemove.Count} Invalid Enemies");
        //    _invalidIdsToRemove.Clear();
        //}
    }

    public override void Dispose()
    {
        _listController.Dispose();
        Events.OnEnemyAdded -= EnemyAdded;
        Events.OnEnemyRemoved -= EnemyRemoved;
        KnownEnemies.Clear();
        EnemiesInLineOfSight.Clear();
        VisibleEnemies.Clear();
        base.Dispose();
    }

    public Enemy GetEnemy(string profileID, bool mustBeActive) => _listController.GetEnemy(profileID, mustBeActive);

    public Enemy CheckAddEnemy(IPlayer IPlayer) => _listController.CheckAddEnemy(IPlayer);

    public void RemoveEnemy(string profileID) => _listController.RemoveEnemy(profileID);

    public bool IsPlayerAnEnemy(string profileID) => _listController.IsPlayerAnEnemy(profileID);

    public bool IsPlayerFriendly(IPlayer iPlayer) => _listController.IsPlayerFriendly(iPlayer);

    public bool IsBotInBotsGroup(BotOwner botOwner) => _listController.IsBotInBotsGroup(botOwner);

    private void EnemyAdded(Enemy enemy)
    {
        EnemyEvents events = enemy.Events;
        KnownEnemies.Subscribe(ref events.OnEnemyKnownChanged.OnToggle);
        EnemiesInLineOfSight.Subscribe(ref events.OnVisionChange.OnToggle);
        VisibleEnemies.Subscribe(ref events.OnEnemyLineOfSightChanged.OnToggle);
    }

    private void EnemyRemoved(string profileID, Enemy enemy)
    {
        EnemyEvents events = enemy.Events;
        KnownEnemies.Unsubscribe(ref events.OnEnemyKnownChanged.OnToggle, enemy);
        EnemiesInLineOfSight.Unsubscribe(ref events.OnVisionChange.OnToggle, enemy);
        VisibleEnemies.Unsubscribe(ref events.OnEnemyLineOfSightChanged.OnToggle, enemy);
        KnownEnemies.RemoveEnemy(enemy);
        EnemiesInLineOfSight.RemoveEnemy(enemy);
        VisibleEnemies.RemoveEnemy(enemy);
        if (GoalEnemy != null && GoalEnemy == enemy)
        {
            GoalEnemy = null;
        }
        if (LastGoalEnemy != null && LastGoalEnemy == enemy)
        {
            LastGoalEnemy = null;
        }
    }

    public void UpdateEnemies(BotComponent bot, float currentTime)
    {
        List<IPlayer> Allies = bot.BotOwner?.BotsGroup?.Allies;
        if (Allies == null)
        {
#if DEBUG
            Logger.LogError($"[{bot.name}] No Allies List!");
#endif
            return;
        }
        bool searching = bot.Decision.CurrentCombatDecision == ECombatDecision.Search;
        float forgetEnemyTime = bot.Info.ForgetEnemyTime;
        foreach (Enemy Enemy in EnemiesArray)
        {
            if (!Enemy.CheckValid())
            {
                _invalidIdsToRemove.Add(Enemy.EnemyProfileId);
                continue;
            }
            if (Allies.Contains(Enemy.EnemyPlayer))
            {
#if DEBUG
                if (SAINPlugin.DebugMode)
                    Logger.LogWarning($"{Enemy.EnemyPlayer.name} is an ally of {Bot.Player.name} and will be removed from its enemies collection");
#endif

                _allyIdsToRemove.Add(Enemy.EnemyProfileId);
                continue;
            }
            Enemy.TickEnemy(currentTime, forgetEnemyTime, searching);
        }

        if (_invalidIdsToRemove.Count > 0)
        {
            foreach (var id in _invalidIdsToRemove)
                RemoveEnemy(id);
#if DEBUG
            Logger.LogWarning($"Removed {_invalidIdsToRemove.Count} Invalid Enemies");
#endif
            _invalidIdsToRemove.Clear();
        }

        if (_allyIdsToRemove.Count > 0)
        {
            foreach (var id in _allyIdsToRemove)
                RemoveEnemy(id);
#if DEBUG
            if (SAINPlugin.DebugMode)
                Logger.LogWarning($"Removed {_allyIdsToRemove.Count} allies");
#endif
            _allyIdsToRemove.Clear();
        }
    }

    public Enemy GoalEnemy {
        get
        {
            return _goalEnemy;
        }
        private set
        {
            if (value == _goalEnemy)
            {
                return;
            }
            if (value != null)
            {
                if (value.LastKnownPosition == null)
                {
                    //Logger.LogError($"Bot: [{Bot.name}] cant set enemy [{value.EnemyName}] with null last known![Timesinceseen {value.TimeSinceSeen}: timesinceheard{value.TimeSinceHeard}] [IsAlive:{value.EnemyPlayer?.HealthController.IsAlive} : IsSainBot:[{value.EnemyPlayerComponent?.IsSAINBot}]]");
                    return;
                }
            }

            LastGoalEnemy = _goalEnemy;
            _goalEnemy = value;
            Events.EnemyChanged(value, LastGoalEnemy);
        }
    }

    public Enemy LastGoalEnemy { get; private set; }

    public Enemy ChooseEnemy()
    {
        if (KnownEnemies.Count == 0)
        {
            ClearEnemy();
        }
        else
        {
            GoalEnemy = SelectEnemy();
            setGoalEnemy(GoalEnemy?.EnemyInfo);
        }
        return GoalEnemy;
    }

    public void ClearEnemy()
    {
        GoalEnemy = null;
        setGoalEnemy(null);
    }

    private readonly List<Enemy> _preAllocList = [];

    private Enemy SelectEnemy()
    {
        if (Bot.Decision.DogFightDecision.CheckShallDogFight(KnownEnemies, out Enemy dogFightEnemy))
        {
            return dogFightEnemy;
        }
        const float CHANGE_ENEMY_DIST_RATIO_SHOOTER = 0.75f;
        const float CHANGE_ENEMY_DIST_RATIO_NON_SHOOTER = 0.33f;
        const float CHANGE_ENEMY_KNOWN_KNOWN_DIST_RATIO = 0.5f;
        const float CHANGE_ENEMY_KNOWN_SHOT_AT_ME_DIST_RATIO = 0.5f;
        const float MAX_DISTANCE_NON_ENGAGED_PRIORITIZE_DIST = 50f;

        if (_goalEnemy != null && (!_goalEnemy.CheckValid() || !Enemy.IsEnemyActive(_goalEnemy) || !_goalEnemy.EnemyKnown || _goalEnemy.LastKnownPosition == null))
        {
            LastGoalEnemy = _goalEnemy;
            _goalEnemy = null;
        }

        if (VisibleEnemies.Count > 0)
        {
            return SelectVisibleEnemy(CHANGE_ENEMY_DIST_RATIO_SHOOTER, CHANGE_ENEMY_DIST_RATIO_NON_SHOOTER);
        }

        if (Bot.Medical.TimeSinceShot < 2f)
        {
            Enemy enemy = Bot.Medical.HitByEnemy.EnemyWhoLastShotMe;
            if (enemy != null && enemy.LastKnownPosition != null && KnownEnemies.Contains(enemy))
            {
                return enemy;
            }
        }
        //
        //if (BotOwner.Memory.IsUnderFire)
        //{
        //    Enemy enemy = Bot.Memory.LastUnderFireEnemy;
        //    if (enemy != null && KnownEnemies.Contains(enemy))
        //    {
        //        return enemy;
        //    }
        //}

        if (_goalEnemy != null)
        {
            if (_goalEnemy.Events.OnSearch.Value)
            {
                return _goalEnemy;
            }
            var decision = Bot.Decision.CurrentCombatDecision;
            switch (decision)
            {
                case ECombatDecision.SeekCover:
                case ECombatDecision.Retreat:
                case ECombatDecision.RunAway:
                case ECombatDecision.ShiftCover:
                case ECombatDecision.FightZombies:
                case ECombatDecision.None:
                    break;

                default:
                    return _goalEnemy;
            }
        }

        List<Enemy> seenEnemies = _preAllocList;
        KnownEnemies.FilterByPredicateNonAlloc(seenEnemies, x => x.Seen);
        if (seenEnemies.Count > 0)
        {
            seenEnemies.Sort((x, y) => x.TimeSinceSeen.CompareTo(y.TimeSinceSeen));
            foreach (Enemy seenEnemy in seenEnemies)
            {
                if (seenEnemy.IsShooter() && (seenEnemy.Status.ShotAtMe || seenEnemy.Status.ShotMe))
                {
                    return seenEnemy;
                }
            }
            return seenEnemies[0];
        }

        KnownEnemies.SortBy(EnemyList.EBotListSortType.ByLastKnownDistance);
        Enemy closestKnownEnemy = KnownEnemies[0];
        if (_goalEnemy == null) return closestKnownEnemy;
        if (_goalEnemy == closestKnownEnemy) return closestKnownEnemy;
        for (int i = 0; i < KnownEnemies.Count; i++)
        {
            Enemy knownEnemy = KnownEnemies[i];
            if (knownEnemy.KnownPlaces.BotDistanceFromLastKnown <= MAX_DISTANCE_NON_ENGAGED_PRIORITIZE_DIST) return knownEnemy;
            if (knownEnemy.KnownPlaces.EnemyDistanceFromLastKnown < CHANGE_ENEMY_KNOWN_KNOWN_DIST_RATIO * _goalEnemy.RealDistance) return closestKnownEnemy;
        }

        List<Enemy> enemiesWhoEngagedMe = _preAllocList;
        KnownEnemies.FilterByPredicateNonAlloc(enemiesWhoEngagedMe, x => x.Status.ShotAtMe || x.Status.ShotMe);
        if (enemiesWhoEngagedMe.Count > 0)
        {
            Enemy lastEngagedEnemy = enemiesWhoEngagedMe[0];
            if (_goalEnemy == null) return lastEngagedEnemy;
            if (_goalEnemy == lastEngagedEnemy) return lastEngagedEnemy;
            for (int i = 0; i < KnownEnemies.Count; i++)
            {
                Enemy engagedEnemy = enemiesWhoEngagedMe[i];
                if (engagedEnemy.KnownPlaces.EnemyDistanceFromLastKnown < CHANGE_ENEMY_KNOWN_SHOT_AT_ME_DIST_RATIO * _goalEnemy.RealDistance) return engagedEnemy;
            }
        }
        return closestKnownEnemy;
    }

    private Enemy SelectVisibleEnemy(float CHANGE_ENEMY_DIST_RATIO_SHOOTER, float CHANGE_ENEMY_DIST_RATIO_NON_SHOOTER)
    {
        if (VisibleEnemies.Count > 1) VisibleEnemies.SortBy(EnemyList.EBotListSortType.ByRealDistance);
        Enemy closestVisibleEnemy = VisibleEnemies[0];

        if (_goalEnemy == null) return closestVisibleEnemy;

        List<Enemy> visibleShooters = _preAllocList;
        VisibleEnemies.FilterByPredicateNonAlloc(visibleShooters, x => x.IsShooter());
        if (visibleShooters.Count > 0)
        {
            Enemy visibleShooter = visibleShooters[0];
            if (closestVisibleEnemy != visibleShooter &&
                closestVisibleEnemy.RealDistance < CHANGE_ENEMY_DIST_RATIO_NON_SHOOTER * visibleShooter.RealDistance)
            {
                return closestVisibleEnemy;
            }

            if (visibleShooter == _goalEnemy) return _goalEnemy;
            else if (visibleShooter.RealDistance < CHANGE_ENEMY_DIST_RATIO_SHOOTER * _goalEnemy.RealDistance)
                return visibleShooter;
        }

        for (int i = 0; i < VisibleEnemies.Count; i++)
        {
            Enemy visibleEnemy = VisibleEnemies[i];
            if (visibleEnemy == _goalEnemy)
            {
                return _goalEnemy;
            }
            else if (visibleEnemy.RealDistance < CHANGE_ENEMY_DIST_RATIO_SHOOTER * _goalEnemy.RealDistance)
            {
                return visibleEnemy;
            }
            if (visibleEnemy == _goalEnemy) return _goalEnemy;
            else if (visibleEnemy.RealDistance < CHANGE_ENEMY_DIST_RATIO_SHOOTER * _goalEnemy.RealDistance)
                return visibleEnemy;
        }
        return closestVisibleEnemy;
    }

    private void setGoalEnemy(EnemyInfo enemyInfo)
    {
        if (enemyInfo != null)
            BotOwner.Memory.IsPeace = false;
        if (BotOwner.Memory.GoalEnemy != enemyInfo)
        {
            try
            {
                BotOwner.Memory.GoalEnemy = enemyInfo;
            }
            catch
            {
                // Sometimes bsg code throws an error here :D
            }
        }
    }

    private void checkDiscrepency()
    {
        EnemyInfo goalEnemy = BotOwner.Memory.GoalEnemy;
        if (goalEnemy != null && GoalEnemy == null)
        {
            //if (_nextLogTime < Time.time)
            //{
            //_nextLogTime = Time.time + 1f;

            //Logger.LogError("Bot's Goal Enemy is not null, but SAIN enemy is null.");
            if (goalEnemy.Person == null)
            {
                //Logger.LogError("Bot's Goal Enemy Person is null");
                return;
            }
            if (goalEnemy.ProfileId == Bot.ProfileId)
            {
                //Logger.LogError("goalEnemy.ProfileId == SAINBot.ProfileId");
                return;
            }
            if (goalEnemy.ProfileId == Bot.Player.ProfileId)
            {
                //Logger.LogError("goalEnemy.ProfileId == SAINBot.Player.ProfileId");
                return;
            }
            if (goalEnemy.ProfileId == Bot.BotOwner.ProfileId)
            {
                //Logger.LogError("goalEnemy.ProfileId == SAINBot.Player.ProfileId");
                return;
            }
            Bot.EnemyController.CheckAddEnemy(goalEnemy.Person);
            //}
        }
    }

    private Enemy _goalEnemy;

    private readonly List<string> _allyIdsToRemove = [];
    private readonly List<string> _invalidIdsToRemove = [];

    private readonly EnemyListController _listController;
}