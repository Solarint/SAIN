using BepInEx.Logging;
using EFT;
using SAIN.Classes;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Components
{
    public class DecisionComponent : MonoBehaviour
    {
        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private const float DecisionFreq = 0.1f;
        private const float UpdateSearchFreq = 30f;
        private const float SearchRandomize = 0.25f;

        public List<SAINLogicDecision> RetreatDecisions = new List<SAINLogicDecision> { SAINLogicDecision.Reload, SAINLogicDecision.RunAwayGrenade, SAINLogicDecision.RunAway, SAINLogicDecision.Surgery, SAINLogicDecision.FirstAid };

        private void Update()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                CurrentDecision = SAINLogicDecision.None;
                return;
            }

            if (UpdateSearchTimer < Time.time)
            {
                UpdateSearchTimer = Time.time + UpdateSearchFreq;

                TimeBeforeSearch = GetTimeBeforeSearch();

                if (SAIN.BotSquad.BotInGroup)
                {
                    int i = 1;
                    float time = TimeBeforeSearch;

                    foreach (var member in SAIN.BotSquad.SquadMembers)
                    {
                        i++;
                        time += member.Value.Decisions.TimeBeforeSearch;
                    }

                    TimeBeforeSearch = time / i;
                    TimeBeforeSearch *= Random.Range(0.9f, 1.1f);

                    if (SAIN.BotSquad.IsSquadLead)
                    {
                        TimeBeforeSearch += 5f;
                    }

                    if (DebugMode)
                    {
                        Logger.LogInfo($"Time Before Search: [{TimeBeforeSearch}]");
                    }
                }
            }

            LastDecision = CurrentDecision;
            CurrentDecision = GetDecision();

            if (LastDecision != CurrentDecision)
            {
                ChangeDecisionTime = Time.time;
            }
        }

        public float ChangeDecisionTime { get; private set; }
        public float TimeSinceChangeDecision => Time.time - ChangeDecisionTime;

        public float GetTimeBeforeSearch()
        {
            var info = SAIN.Info;
            var pers = info.BotPersonality;
            var group = SAIN.BotSquad;

            float searchTime;

            if (info.IsFollower && group.BotInGroup)
            {
                searchTime = 3f;
            }
            else if (info.IsBoss && group.BotInGroup)
            {
                searchTime = 20f;
            }
            else
            {
                switch (pers)
                {
                    case BotPersonality.GigaChad:
                        searchTime = 1f;
                        break;

                    case BotPersonality.Chad:
                        searchTime = 3f;
                        break;

                    case BotPersonality.Timmy:
                        searchTime = 50f;
                        break;

                    case BotPersonality.Rat:
                        searchTime = 90f;
                        break;

                    case BotPersonality.Coward:
                        searchTime = 30f;
                        break;

                    default:
                        searchTime = 10f;
                        break;
                }
            }

            searchTime *= Random.Range(1f - SearchRandomize, 1f + SearchRandomize);

            if (DebugMode)
            {
                Logger.LogDebug($"Search Time = [{searchTime}] because: IsBoss? [{info.IsBoss}] IsFollower? [{info.IsFollower}] Personality [{pers}] SquadLead? [{group.IsSquadLead}] Squad Members: [{group.SquadMembers.Count}]");
            }

            return searchTime;
        }

        private bool CheckContinueRetreat()
        {
            if (SAIN.Enemy.SAINEnemy == null) return false;

            return RetreatDecisions.Contains(CurrentDecision) && !SAIN.Cover.BotIsAtCoverPoint && Time.time - ChangeDecisionTime < 3f && SAIN.BotHasStamina;
        }

        private SAINLogicDecision GetDecision()
        {
            if (SAIN.BotIsStuck)
            {
                if (CurrentDecision != SAINLogicDecision.Search)
                {
                    return SAINLogicDecision.Search;
                }
                else if (Time.time - ChangeDecisionTime > 10f)
                {
                    return SAINLogicDecision.MoveToCover;
                }
            }
            if (CheckContinueDecision(out SAINLogicDecision Decision))
            {
                return Decision;
            }
            if (SelfActionDecisions(out Decision))
            {
                return Decision;
            }
            if (StartRegroup())
            {
                return SAINLogicDecision.RegroupSquad;
            }
            if (GoalEnemyDecision(out Decision))
            {
                return Decision;
            }
            if (GoalTargetDecision(out Decision))
            {
                return Decision;
            }
            return SAINLogicDecision.None;
        }

        private bool CheckContinueDecision(out SAINLogicDecision Decision)
        {
            if (CheckContinueSelfAction(out Decision))
            {
                return true;
            }
            if (CheckContinueRetreat())
            {
                Decision = CurrentDecision;
                return true;
            }
            return false;
        }

        private bool SelfActionDecisions(out SAINLogicDecision Decision)
        {
            Decision = SAINLogicDecision.None;

            if (BotOwner.BewareGrenade?.GrenadeDangerPoint != null)
            {
                Decision = SAINLogicDecision.RunAwayGrenade;
            }
            else if (StartFirstAid())
            {
                Decision = SAINLogicDecision.FirstAid;
            }
            else if (StartBotReload())
            {
                Decision = SAINLogicDecision.Reload;
            }
            else if (StartUseStims())
            {
                Decision = SAINLogicDecision.Stims;
            }
            else if (StartSurgery())
            {
                Decision = SAINLogicDecision.Surgery;
            }

            return Decision != SAINLogicDecision.None;
        }

        private bool CheckContinueSelfAction(out SAINLogicDecision Decision)
        {
            if (BotOwner.Medecine.FirstAid.Using)
            {
                Decision = SAINLogicDecision.FirstAid;
                return true;
            }
            if (BotOwner.WeaponManager.Reload.Reloading)
            {
                Decision = SAINLogicDecision.Reload;
                return true;
            }
            if (BotOwner.Medecine.Stimulators.Using)
            {
                Decision = SAINLogicDecision.Stims;
                return true;
            }
            if (BotOwner.Medecine.SurgicalKit.Using)
            {
                Decision = SAINLogicDecision.Surgery;
                return true;
            }
            Decision = SAINLogicDecision.None;
            return false;
        }

        private bool GoalTargetDecision(out SAINLogicDecision Decision)
        {
            Decision = SAINLogicDecision.None;

            if (!SAIN.HasGoalTarget)
            {
                return false;
            }

            var target = BotOwner.Memory.GoalTarget.GoalTarget;
            if (target.Position != null)
            {
                if (target.IsDanger || BotOwner.Memory.IsUnderFire)
                {
                    Decision = SAINLogicDecision.MoveToCover;
                }
                else
                {
                    Decision = SAINLogicDecision.Search;
                }
            }

            return Decision != SAINLogicDecision.None;
        }

        private bool GoalEnemyDecision(out SAINLogicDecision Decision)
        {
            if (BotOwner.Memory.GoalEnemy == null)
            {
                Decision = SAINLogicDecision.None;
                return false;
            }

            if (StartDogFightAction())
            {
                Decision = SAINLogicDecision.DogFight;
            }
            else if (StartStandAndShoot())
            {
                Decision = SAINLogicDecision.StandAndShoot;
            }
            else if (StartSuppression())
            {
                Decision = SAINLogicDecision.Suppress;
            }
            else if (StartSeekEnemy(out var newDecision))
            {
                Decision = newDecision;
            }
            else if (StartHoldInCover())
            {
                Decision = SAINLogicDecision.HoldInCover;
            }
            else if (StartMoveToCover())
            {
                Decision = SAINLogicDecision.MoveToCover;

                if (StartRunForCover())
                {
                    Decision = SAINLogicDecision.RunForCover;
                }
            }
            else
            {
                Decision = SAINLogicDecision.Shoot;
            }

            return true;
        }

        private bool StartDogFightAction()
        {
            var pathStatus = SAIN.Enemy.SAINEnemy.CheckPathDistance();
            return pathStatus == SAINPathDistance.VeryClose && SAIN.EnemyIsVisible;
        }

        private bool HideFromFlankShot
        {
            get
            {
                if (BotOwner.Memory.IsUnderFire)
                {
                }
                return false;
            }
        }

        private bool StartSuppression()
        {
            if (SAIN.BotSquad.BotInGroup && SAIN.HasGoalEnemy && SAIN.BotSquad.SquadMembers != null)
            {
                bool start = false;
                foreach (var member in SAIN.BotSquad.SquadMembers.Values)
                {
                    if (member == null || member.BotOwner == BotOwner || member.BotOwner.IsDead)
                    {
                        continue;
                    }

                    if (RetreatDecisions.Contains(member.CurrentDecision) && (member.BotOwner.Position - BotOwner.Position).magnitude < 50f)
                    {
                        if (SAIN.HasEnemy && member.HasEnemy)
                        {
                            if (SAIN.Enemy.SAINEnemy.Person == member.Enemy.SAINEnemy.Person)
                            {
                                start = true;
                                break;
                            }
                        }
                    }
                }
                if (start)
                {
                    return true;
                }
            }

            return false;
        }

        private bool StartRunForCover()
        {
            return StartRunCoverTimer < Time.time && SAIN.BotHasStamina && BotOwner.CanSprintPlayer;
        }

        private float StartRunCoverTimer;

        private bool StartPeekCorner()
        {
            return false;
        }

        private bool StartMoveToCover()
        {
            bool start = !SAIN.Cover.BotIsAtCoverPoint && (SAIN.Cover.CurrentCoverPoint != null || SAIN.Cover.CurrentFallBackPoint != null);

            if (start)
            {
                if (CurrentDecision != SAINLogicDecision.MoveToCover && CurrentDecision != SAINLogicDecision.RunForCover)
                {
                    StartRunCoverTimer = Time.time + 3f * Random.Range(0.66f, 1.33f);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool StartRegroup()
        {
            if (!SAIN.BotSquad.BotInGroup)
            {
                return false;
            }

            if (SAIN.HasEnemy && BotOwner.Memory.GoalEnemy != null)
            {
                if (SAIN.HasEnemyAndCanShoot || SAIN.EnemyIsVisible)
                {
                    return false;
                }
                if (Time.time - BotOwner.Memory.GoalEnemy.PersonalLastShootTime < 20f)
                {
                    return false;
                }
            }

            if (SAIN.BotSquad.BotInGroup && SAIN.BotSquad.SquadMembers != null)
            {
                foreach (var member in SAIN.BotSquad.SquadMembers.Values)
                {
                    if (member == null || member.BotOwner == BotOwner || member.BotOwner.IsDead)
                    {
                        continue;
                    }

                    if (member.BotSquad.IsSquadLead)
                    {
                        Vector3 leadPos = member.BotOwner.Position;
                        if (SAIN.HasEnemy && BotOwner.Memory.GoalEnemy != null)
                        {
                            Vector3 directionToEnemy = BotOwner.Memory.GoalEnemy.CurrPosition - BotOwner.Position;
                            Vector3 directionToLead = leadPos - BotOwner.Position;
                            if (Vector3.Dot(directionToEnemy.normalized, directionToLead.normalized) > 0f && Vector3.Distance(BotOwner.Memory.GoalEnemy.CurrPosition, BotOwner.Position) < 30f)
                            {
                                return false;
                            }
                        }

                        float dist = Vector3.Distance(leadPos, BotOwner.Position);
                        if (CurrentDecision == SAINLogicDecision.RegroupSquad)
                        {
                            return dist > 10f;
                        }
                        else
                        {
                            return dist > 30f;
                        }
                    }
                }
            }
            return false;
        }

        private bool StartSeekEnemy(out SAINLogicDecision Decision)
        {
            Decision = SAINLogicDecision.None;

            if (SAIN.HasEnemyAndCanShoot || SAIN.EnemyIsVisible)
            {
                return false;
            }

            bool startSearchSolo = Time.time - BotOwner.Memory.GoalEnemy.PersonalLastShootTime > TimeBeforeSearch;
            if (startSearchSolo)
            {
                Decision = SAIN.BotSquad.BotInGroup ? SAINLogicDecision.GroupSearch : SAINLogicDecision.Search;
                return true;
            }

            bool start = false;
            if (SAIN.BotSquad.BotInGroup && SAIN.BotSquad.SquadMembers != null)
            {
                foreach (var member in SAIN.BotSquad.SquadMembers.Values)
                {
                    if (member == null || member.BotOwner == BotOwner || member.BotOwner.IsDead)
                    {
                        continue;
                    }

                    if (SAIN.HasEnemy && member.HasEnemy)
                    {
                        if (SAIN.Enemy.SAINEnemy.Person == member.Enemy.SAINEnemy.Person)
                        {
                            if (member.CurrentDecision == SAINLogicDecision.GroupSearch)
                            {
                                start = true;
                                break;
                            }
                            if (member.Enemy.SAINEnemy.PathDistance < 20f && member.EnemyIsVisible)
                            {
                                start = true;
                                break;
                            }
                        }
                    }
                }
            }

            Decision = start ? SAINLogicDecision.GroupSearch : SAINLogicDecision.None;
            return false;
        }

        private float TimeSinceVisionChangeSolo => SAIN.HasGoalEnemy ? Time.time - BotOwner.Memory.GoalEnemy.LastChangeVisionTime : -1f;

        private bool StartHoldInCover()
        {
            if (SAIN.Cover.BotIsAtCoverPoint)
            {
                return true;
            }
            return false;
        }

        private bool StartStandAndShoot()
        {
            if (SAIN.EnemyIsVisible || SAIN.HasEnemyAndCanShoot)
            {
                float holdGround = SAIN.Info.HoldGroundDelay;

                if (holdGround <= 0f)
                {
                    return false;
                }

                float changeVis = Time.time - BotOwner.Memory.GoalEnemy.LastChangeVisionTime;
                float lastShoot = Time.time - BotOwner.Memory.GoalEnemy.PersonalLastShootTime;

                if (changeVis < holdGround && lastShoot < holdGround)
                {
                    if (changeVis < holdGround / 2f && lastShoot < holdGround / 2f)
                    {
                        return true;
                    }
                    else
                    {
                        return SAIN.Cover.CheckLimbsForCover();
                    }
                }
            }
            return false;
        }

        private bool StartRunAway => false;

        private bool StartSurgery()
        {
            bool BotShouldHeal = false;

            if (BotOwner.Medecine.SurgicalKit.HaveWork)
            {
                if (BotOwner.Memory.GoalEnemy == null)
                {
                    BotShouldHeal = true;
                }
                else
                {
                    var enemy = SAIN.Enemy.SAINEnemy;
                    var pathStatus = enemy.CheckPathDistance();
                    bool SeenRecent = Time.time - enemy.GoalEnemy.TimeLastSeenReal < 10f;

                    if (!SeenRecent && pathStatus != SAINPathDistance.VeryClose && pathStatus != SAINPathDistance.Close)
                    {
                        BotShouldHeal = true;
                    }
                }
            }

            return BotShouldHeal;
        }

        private const float LowAmmoThresh0to1 = 0.4f;

        private bool StartUseStims()
        {
            bool takeStims = false;
            if (BotOwner.Medecine.Stimulators.HaveSmt && LastStimTime < Time.time)
            {
                var status = SAIN.BotStatus;
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
                    var enemy = SAIN.Enemy.SAINEnemy;
                    var pathStatus = enemy.CheckPathDistance();
                    bool SeenRecent = Time.time - enemy.GoalEnemy.TimeLastSeenReal < 3f;

                    if (status.Injured || status.BadlyInjured)
                    {
                        if (!enemy.InLineOfSight && pathStatus != SAINPathDistance.VeryClose && !SeenRecent)
                        {
                            takeStims = true;
                        }
                        else if (pathStatus == SAINPathDistance.Far || pathStatus == SAINPathDistance.VeryFar)
                        {
                            takeStims = true;
                        }
                    }
                    else if (status.Dying)
                    {
                        if (!enemy.InLineOfSight && !SeenRecent)
                        {
                            takeStims = true;
                        }
                        else if (pathStatus == SAINPathDistance.Far || pathStatus == SAINPathDistance.VeryFar)
                        {
                            takeStims = true;
                        }
                    }
                }
            }

            if (takeStims)
            {
                LastStimTime = Time.time + 5f;
            }

            return takeStims;
        }

        private bool StartFirstAid()
        {
            bool BotShouldHeal = false;
            var status = SAIN.BotStatus;
            if (!BotOwner.Medecine.Using && BotOwner.Medecine.FirstAid.ShallStartUse())
            {
                if (BotOwner.Memory.GoalEnemy == null)
                {
                    BotShouldHeal = true;
                }
                else
                {
                    var enemy = SAIN.Enemy.SAINEnemy;
                    var pathStatus = enemy.CheckPathDistance();
                    bool SeenRecent = Time.time - enemy.GoalEnemy.TimeLastSeenReal < 3f;

                    if (status.Injured)
                    {
                        if (!enemy.InLineOfSight && !SeenRecent && pathStatus != SAINPathDistance.VeryClose && pathStatus != SAINPathDistance.Close)
                        {
                            BotShouldHeal = true;
                        }
                    }
                    else if (status.BadlyInjured)
                    {
                        if (!enemy.InLineOfSight && pathStatus != SAINPathDistance.VeryClose)
                        {
                            BotShouldHeal = true;
                        }

                        if (enemy.InLineOfSight && (pathStatus == SAINPathDistance.Far || pathStatus == SAINPathDistance.VeryFar))
                        {
                            BotShouldHeal = true;
                        }
                    }
                    else if (status.Dying)
                    {
                        if (!enemy.InLineOfSight)
                        {
                            BotShouldHeal = true;
                        }
                        if (pathStatus != SAINPathDistance.VeryClose && pathStatus != SAINPathDistance.Close)
                        {
                            BotShouldHeal = true;
                        }
                    }
                }
            }

            return BotShouldHeal;
        }

        public bool StartCancelReload()
        {
            if (!BotOwner.WeaponManager.IsReady || BotOwner.WeaponManager.Reload.BulletCount == 0)
            {
                return false;
            }

            if (BotOwner.Memory.GoalEnemy != null && BotOwner.WeaponManager.Reload.Reloading && SAIN.Enemy.SAINEnemy != null)
            {
                var enemy = SAIN.Enemy.SAINEnemy;
                var pathStatus = enemy.CheckPathDistance();
                bool SeenRecent = Time.time - enemy.GoalEnemy.PersonalLastShootTime < 3f;

                if (SeenRecent && Vector3.Distance(BotOwner.Position, enemy.Person.Position) < 8f)
                {
                    return true;
                }

                if (!LowAmmo && enemy.IsVisible)
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"My Enemy is in sight, and I'm not low on ammo!");
                    }

                    return true;
                }
                if (pathStatus == SAINPathDistance.VeryClose)
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"My Enemy is Very Close I have ammo!");
                    }
                    return true;
                }
                if (BotOwner.WeaponManager.Reload.BulletCount > 1 && pathStatus == SAINPathDistance.Close)
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"My Enemy is Close I have more than 1 bullet!");
                    }
                    return true;
                }
            }

            return false;
        }

        private bool StartBotReload()
        {
            if (!BotOwner.WeaponManager.IsReady || !BotOwner.WeaponManager.Reload.CanReload(true))
            {
                return false;
            }

            if (StartCancelReload())
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
                    if (BotOwner.Memory.GoalEnemy == null)
                    {
                        if (DebugMode)
                        {
                            Logger.LogDebug($"I'm low on ammo, and I have no enemy, so I should reload");
                        }

                        needToReload = true;
                    }
                    else if (BotOwner.Memory.GoalEnemy.TimeLastSeen < Time.time - 3f)
                    {
                        if (DebugMode)
                        {
                            Logger.LogDebug($"I'm low on ammo, and I haven't seen my enemy in seconds. so I should reload.");
                        }

                        needToReload = true;
                    }
                    else if (BotOwner.Memory.GoalEnemy.Distance > 10f && !SAIN.HasEnemyAndCanShoot)
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

        private bool DebugMode => DebugBotDecisions.Value;

        public float TimeBeforeSearch { get; private set; } = 0f;

        protected ManualLogSource Logger;

        private float LastStimTime = 0f;

        private float DecisionTimer = 0f;

        private float UpdateSearchTimer = 0f;
        private BotOwner BotOwner => SAIN.BotOwner;

        private SAINComponent SAIN;

        public SAINLogicDecision CurrentDecision { get; private set; }

        public SAINLogicDecision LastDecision { get; private set; }

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }
    }
}