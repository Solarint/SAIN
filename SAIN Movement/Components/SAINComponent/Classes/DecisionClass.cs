using BepInEx.Logging;
using EFT;
using SAIN.Classes;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Components
{
    public enum SAINLogicDecision
    {
        None = 0,
        Heal = 1,
        Reload = 2,
        Fight = 3,
        RunForCover = 4,
        Search = 5,
        HoldInCover = 6,
        RunAway = 7,
        CombatHeal = 8,
        Suppress = 9,
        DogFight = 10,
        Skirmish = 11,
        Stims = 12,
        WalkToCover = 13,
        RunAwayGrenade = 14
    }

    public class DecisionClass : SAINBotExt
    {
        public DecisionClass(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource($": {bot.name}: " + GetType().Name);
        }

        public SAINLogicDecision CurrentDecision { get; private set; } = SAINLogicDecision.None;

        private const float DecisionFreq = 0.25f;
        private const float UpdateSearchFreq = 30f;
        private const float SearchRandomize = 0.25f;

        public void ManualUpdate()
        {
            if (BotOwner.IsDead)
            {
                return;
            }

            if (UpdateSearchTimer < Time.time)
            {
                UpdateSearchTimer = Time.time + UpdateSearchFreq;

                TimeBeforeSearch = GetTimeBeforeSearch();
            }

            if (DecisionTimer < Time.time)
            {
                DecisionTimer = Time.time + DecisionFreq;

                CurrentDecision = GetDecision();
            }
        }

        private float DecisionTimer = 0f;
        private float UpdateSearchTimer = 0f;

        public float TimeBeforeSearch { get; private set; } = 0f;

        public float GetTimeBeforeSearch()
        {
            var info = SAIN.Core.Info;
            var pers = info.BotPersonality;
            var group = SAIN.Core.BotSquad;

            float searchTime;

            if (info.IsFollower)
            {
                searchTime = 2f;
            }
            else if (info.IsBoss)
            {
                searchTime = 999f;
            }
            else
            {
                switch (pers)
                {
                    case BotPersonality.GigaChad:
                        searchTime = 3f;
                        break;

                    case BotPersonality.Chad:
                        searchTime = 8f;
                        break;

                    case BotPersonality.Timmy:
                        searchTime = 90f;
                        break;

                    case BotPersonality.Rat:
                        searchTime = 120f;
                        break;

                    case BotPersonality.Coward:
                        searchTime = 999f;
                        break;

                    default:
                        searchTime = 30f;
                        break;
                }

                if (group.IsSquadLead && group.BotInGroup)
                {
                    int count = group.SquadMembers.Length;
                    searchTime *= (count / 2f);
                }
            }

            searchTime *= Random.Range(1f - SearchRandomize, 1f + SearchRandomize);

            Logger.LogDebug($"Search Time = [{searchTime}] because: IsBoss? [{info.IsBoss}] IsFollower? [{info.IsFollower}] Personality [{pers}] SquadLead? [{group.IsSquadLead}] Squad Members: [{group.SquadMembers.Length}]");

            return searchTime;
        }

        private SAINLogicDecision GetDecision()
        {
            SAINLogicDecision Decision = SAINLogicDecision.None;

            if (BotOwner.BotState == EBotState.Active)
            {
                Decision = SelfActionDecisions();

                if (Decision == SAINLogicDecision.None)
                {
                    if (BotOwner.Memory.GoalEnemy != null)
                    {
                        Decision = GoalEnemyDecision();
                    }
                    else if (BotOwner.Memory.GoalTarget != null)
                    {
                        Decision = GoalTargetDecision();
                    }
                }
            }

            return Decision;
        }

        private SAINLogicDecision SelfActionDecisions()
        {
            SAINLogicDecision Decision = SAINLogicDecision.None;

            if (BotOwner.BewareGrenade?.GrenadeDangerPoint != null)
            {
                Decision = SAINLogicDecision.RunAwayGrenade;
            }
            else if (ShouldBotHeal() || BotOwner.Medecine.Using)
            {
                Decision = SAINLogicDecision.Heal;
            }
            else if (ShouldBotReload() || BotOwner.WeaponManager.Reload.Reloading)
            {
                Decision = SAINLogicDecision.Reload;
            }
            else if (ShouldBotPopStims() || BotOwner.Medecine.Stimulators.Using)
            {
                Decision = SAINLogicDecision.Stims;
            }

            return Decision;
        }

        private SAINLogicDecision GoalTargetDecision()
        {
            SAINLogicDecision Decision = SAINLogicDecision.None;

            if (BotOwner.Memory.GoalTarget?.GoalTarget != null)
            {
                var target = BotOwner.Memory.GoalTarget.GoalTarget;
                if (target.Position != null)
                {
                    if (target.IsDanger && (target.IsThisPointClose(BotOwner.Transform.position) || BotOwner.Memory.IsUnderFire))
                    {
                        Decision = SAINLogicDecision.RunForCover;
                    }
                    else
                    {
                        Decision = SAINLogicDecision.Search;
                    }
                }
            }

            return Decision;
        }

        private SAINLogicDecision GoalEnemyDecision()
        {
            SAINLogicDecision Decision = SAINLogicDecision.None;

            if (BotOwner.Memory.GoalEnemy != null)
            {
                if (StartWalkToCover())
                {
                    Decision = SAINLogicDecision.WalkToCover;
                }
                else if (StartSeekEnemy())
                {
                    Decision = SAINLogicDecision.Search;
                }
                else if (StartHoldInCover())
                {
                    Decision = SAINLogicDecision.HoldInCover;
                }
                else if (StartDogFightAction())
                {
                    Decision = SAINLogicDecision.DogFight;
                }
                else if (StartSuppression())
                {
                    Decision = SAINLogicDecision.Suppress;
                }
                else if (StartFightAction())
                {
                    Decision = SAINLogicDecision.Fight;
                }
                else
                {
                    Decision = SAINLogicDecision.Skirmish;
                }
            }

            return Decision;
        }

        private bool StartSuppression()
        {
            var sainEnemy = SAIN.Core.Enemy;

            if (sainEnemy != null && BotOwner.Memory.GoalEnemy != null && sainEnemy.CurrentPerson != null && sainEnemy.LastSeen.EnemyPosition != Vector3.zero)
            {
                if (BotOwner.WeaponManager.IsReady && BotOwner.WeaponManager.HaveBullets && !LowAmmo)
                {
                    if (!sainEnemy.CanSee)
                    {
                        if (sainEnemy.LastSeen.TimeSinceSeen < 3f)
                        {
                            var realPos = sainEnemy.CurrentPerson.Transform.position;
                            var lastSeenPos = sainEnemy.LastSeen.EnemyPosition;

                            if (Vector3.Distance(realPos, lastSeenPos) < 5f)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool StartRunForCover()
        {
            return false;
        }

        private bool StartWalkToCover()
        {
            return !SAIN.Cover.InCover && Enemy.CanSee && Enemy.TimeVisibleReal > 2;
        }

        private bool StartSeekEnemy()
        {
            return !Enemy.CanSee && Enemy.LastSeen.TimeSinceSeen > TimeBeforeSearch;
        }

        private bool StartHoldInCover()
        {
            return Enemy.CanSee && Enemy.CanShoot && SAIN.Cover.InCover;
        }

        private bool StartRunAway()
        {
            return false;
        }

        private bool StartCombatHeal()
        {
            return false;
        }

        private const float FightIn = 125f;
        private const float FightOut = 150f;

        private const float DogFightIn = 25f;
        private const float DogFightOut = 35f;

        private const float LowAmmoThresh0to1 = 0.4f;

        public bool StartFightAction()
        {
            bool active = false;

            if (BotOwner.Memory.GoalEnemy != null)
            {
                if (SAIN.Core.Enemy.LastSeen.EnemyStraightDistance < 100f)
                {
                    float distance = SAIN.Core.Enemy.Path.Length;

                    active = FightActive;

                    if (distance < FightIn)
                    {
                        active = true;
                    }
                    else if (distance > FightOut)
                    {
                        active = false;
                    }

                    if (DebugMode)
                    {
                        DebugDrawPath(distance, FightOut, active);
                    }
                }
            }

            FightActive = active;
            return active;
        }

        public bool StartDogFightAction()
        {
            bool active = DogFightActive;

            if (BotOwner.Memory.GoalEnemy == null)
            {
                active = false;
            }
            else if (SAIN.Core.Enemy.LastSeen.EnemyStraightDistance < 50f)
            {
                float distance = SAIN.Core.Enemy.Path.Length;

                if (distance < DogFightIn)
                {
                    active = true;
                }
                else if (distance > DogFightOut)
                {
                    active = false;
                }

                if (DebugMode)
                {
                    DebugDrawPath(distance, DogFightOut, active);
                }
            }

            DogFightActive = active;
            return active;
        }

        public bool ShouldBotPopStims()
        {
            bool takeStims = false;
            if (!BotOwner.Medecine.Stimulators.Using && SAIN.Core.Medical.HasStims && LastStimTime < Time.time)
            {
                var status = SAIN.Core.BotStatus;
                if (status.Healthy)
                {
                    takeStims = false;
                }
                else if (BotOwner.Memory.GoalEnemy == null)
                {
                    if (status.Dying || status.BadlyInjured)
                    {
                        takeStims = true;
                    }
                }
                else
                {
                    var enemy = SAIN.Core.Enemy;
                    var path = enemy.Path;

                    if (status.Injured)
                    {
                        if (!enemy.CanSee)
                        {
                            if (!path.RangeVeryClose && !path.RangeClose)
                            {
                                takeStims = true;
                            }
                        }
                        else if (path.RangeFar)
                        {
                            takeStims = true;
                        }
                    }
                    else if (status.BadlyInjured)
                    {
                        if (!enemy.CanSee)
                        {
                            if (!path.RangeVeryClose)
                            {
                                takeStims = true;
                            }
                        }
                        else if (path.RangeFar || path.RangeMid)
                        {
                            takeStims = true;
                        }
                    }
                    else if (status.Dying)
                    {
                        if (!enemy.CanSee)
                        {
                            takeStims = true;
                        }
                        else if (path.RangeFar || path.RangeMid || path.RangeClose)
                        {
                            takeStims = true;
                        }
                    }
                }
            }

            if (takeStims)
            {
                LastStimTime = Time.time + 5f;

                // Debug
                if (BotOwner.Memory.GoalEnemy == null)
                {
                    Logger.LogDebug($"Popped Stims Because: I have no enemy and I'm [{DebugClass.Reason(SAIN.Core.BotStatus)}]");
                }
                else
                {
                    string healthReason = DebugClass.Reason(SAIN.Core.BotStatus);
                    string enemydistReason = DebugClass.Reason(SAIN.Core.Enemy);
                    string canSee = DebugClass.Reason(SAIN.Core.Enemy.CanSee);
                    Logger.LogDebug($"Popped Stims Because: I'm [{healthReason}] and my enemy is [{enemydistReason}] and I [{canSee}] them.");
                }
            }

            return takeStims;
        }

        public bool ShouldBotHeal()
        {
            bool BotShouldHeal = false;
            var status = SAIN.Core.BotStatus;
            if (!BotOwner.Medecine.Using && SAIN.Core.Medical.CanHeal)
            {
                if (BotOwner.Memory.GoalEnemy == null)
                {
                    BotShouldHeal = true;
                }
                else
                {
                    var enemy = SAIN.Core.Enemy;
                    var path = enemy.Path;

                    if (status.Injured)
                    {
                        if (!enemy.CanSee && (path.RangeFar || path.RangeMid))
                        {
                            BotShouldHeal = true;
                        }
                    }
                    else if (status.BadlyInjured)
                    {
                        if (!enemy.CanSee)
                        {
                            if (!path.RangeVeryClose)
                            {
                                BotShouldHeal = true;
                            }
                        }
                        else if (path.RangeFar)
                        {
                            BotShouldHeal = true;
                        }
                    }
                    else if (status.Dying)
                    {
                        if (!enemy.CanSee)
                        {
                            BotShouldHeal = true;
                        }
                        else if (path.RangeFar || path.RangeMid)
                        {
                            BotShouldHeal = true;
                        }
                    }
                }
            }

            if (BotShouldHeal)
            {
                // Debug
                if (BotOwner.Memory.GoalEnemy == null)
                {
                    Logger.LogDebug($"Healed Because: I have no enemy");
                }
                else
                {
                    string healthReason = DebugClass.Reason(SAIN.Core.BotStatus);
                    string enemydistReason = DebugClass.Reason(SAIN.Core.Enemy);
                    string canSee = DebugClass.Reason(SAIN.Core.Enemy.CanSee);
                    Logger.LogDebug($"Healed Because: I'm [{healthReason}] and my enemy is [{enemydistReason}] and I [{canSee}] them.");
                }
            }

            return BotShouldHeal;
        }

        public bool GetShouldBotCancelReload()
        {
            if (!BotOwner.WeaponManager.IsReady)
            {
                return false;
            }

            bool botShouldCancel = false;

            if (BotOwner.Memory.GoalEnemy != null && BotOwner.WeaponManager.Reload.Reloading)
            {
                if (!LowAmmo && SAIN.Core.Enemy.Path.RangeClose)
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"My Enemy is Close, and I have ammo!");
                    }

                    botShouldCancel = true;
                }
                if (BotOwner.WeaponManager.HaveBullets && SAIN.Core.Enemy.Path.RangeVeryClose)
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"My Enemy is Very Close And I have [{AmmoRatio * 100f}] percent of my capacity.");
                    }

                    botShouldCancel = true;
                }
            }

            return botShouldCancel;
        }

        public bool ShouldBotReload()
        {
            if (!BotOwner.WeaponManager.IsReady || !BotOwner.WeaponManager.Reload.CanReload(false))
            {
                return false;
            }

            bool needToReload = false;

            if (!BotOwner.WeaponManager.Reload.Reloading)
            {
                if (!BotOwner.WeaponManager.HaveBullets)
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"I'm Empty! Need to reload!");
                    }

                    needToReload = true;
                }
                else if (LowAmmo)
                {
                    float randomrange = Random.Range(2f, 5f);
                    var enemy = SAIN.Core.Enemy;
                    if (BotOwner.Memory.GoalEnemy == null)
                    {
                        if (DebugMode)
                        {
                            Logger.LogDebug($"I'm low on ammo, and I have no enemy, so I should reload");
                        }

                        needToReload = true;
                    }
                    else if (enemy.LastSeen.TimeSinceSeen > randomrange)
                    {
                        if (DebugMode)
                        {
                            Logger.LogDebug($"I'm low on ammo, and I haven't seen my enemy in [{randomrange}] seconds. so I should reload. Last Saw Enemy [{SAIN.Core.Enemy.LastSeen.TimeSinceSeen}] seconds ago");
                        }

                        needToReload = true;
                    }
                    else if (!enemy.Path.RangeClose && !enemy.CanSee)
                    {
                        if (DebugMode)
                        {
                            Logger.LogDebug($"I'm low on ammo, and I can't see my enemy and he isn't close, so I should reload.");
                        }

                        needToReload = true;
                    }
                }
            }

            return needToReload;
        }

        public bool LowAmmo => AmmoRatio < LowAmmoThresh0to1;

        public float AmmoRatio
        {
            get
            {
                int currentAmmo = BotOwner.WeaponManager.Reload.BulletCount;
                int maxAmmo = BotOwner.WeaponManager.Reload.MaxBulletCount;
                return (float)currentAmmo / maxAmmo;
            }
        }

        private EnemyClass Enemy => SAIN.Core.Enemy;
        private bool DebugMode => DebugBotDecisions.Value;

        private bool FightActive = false;
        private bool DogFightActive = false;
        private float DebugTimer = 0f;

        protected ManualLogSource Logger;

        private float LastStimTime = 0f;

        private void DebugDrawPath(float enemyDistance, float minDistance, bool active)
        {
            if (DebugMode && DebugTimer < Time.time && enemyDistance < minDistance)
            {
                DebugTimer = Time.time + 1f;
                SAIN.DebugDrawList.DrawTempPath(SAIN.Core.Enemy.Path.Path, active, Color.red, Color.green, 0.1f, 1f, true);
            }
        }
    }
}