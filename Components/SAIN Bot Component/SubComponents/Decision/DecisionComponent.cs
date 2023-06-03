using BepInEx.Logging;
using EFT;
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
                    TimeBeforeSearch *= Random.Range(0.8f, 1.2f);

                    if (SAIN.BotSquad.IsSquadLead)
                    {
                        TimeBeforeSearch *= 1.25f;
                    }

                    if (DebugMode)
                    {
                        //Logger.LogInfo($"Time Before Search: [{TimeBeforeSearch}]");
                    }
                }
            }

            if (DecisionTimer < Time.time)
            {
                LastDecision = CurrentDecision;
                CurrentDecision = GetDecision();

                DecisionTimer = Time.time + 0.1f;
            }
        }

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

        private bool CheckIfFinishRetreat()
        {
            if (RetreatDecisions.Contains(CurrentDecision))
            {
                if (SAIN.CurrentTargetPosition != null)
                {
                    Vector3 pos = SAIN.CurrentTargetPosition.Value;
                    pos.y += 0.25f;
                    Vector3 direction = pos - SAIN.HeadPosition;

                    return Physics.Raycast(SAIN.HeadPosition, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
                }
            }
            return true;
        }

        private SAINLogicDecision GetDecision()
        {
            //if (!CheckIfFinishRetreat())
            //{
            //    return CurrentDecision;
            //}

            SAINLogicDecision Decision = SelfActionDecisions();

            if (Decision == SAINLogicDecision.None)
            {
                if (SAIN.HasGoalEnemy)
                {
                    Decision = GoalEnemyDecision();
                }
                else if (SAIN.HasGoalTarget)
                {
                    Decision = GoalTargetDecision();
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
            else if (BotOwner.Medecine.FirstAid.Using || StartFirstAid)
            {
                Decision = SAINLogicDecision.FirstAid;
            }
            else if (BotOwner.WeaponManager.Reload.Reloading || StartBotReload)
            {
                Decision = SAINLogicDecision.Reload;
            }
            else if (BotOwner.Medecine.Stimulators.Using || StartUseStims)
            {
                Decision = SAINLogicDecision.Stims;
            }
            else if (BotOwner.Medecine.SurgicalKit.Using || StartSurgery)
            {
                Decision = SAINLogicDecision.Surgery;
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
                    if (target.IsDanger || BotOwner.Memory.IsUnderFire)
                    {
                        Decision = SAINLogicDecision.MoveToCover;
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
                if (StartSuppression)
                {
                    Decision = SAINLogicDecision.Suppress;
                }
                else if (StartSeekEnemy)
                {
                    Decision = SAINLogicDecision.Search;
                }
                else if (StartHoldInCover)
                {
                    Decision = SAINLogicDecision.HoldInCover;
                }
                else if (StartMoveToCover())
                {
                    Decision = SAINLogicDecision.MoveToCover;
                }
                ///else if (StartDogFightAction())
                ///{
                ///    Decision = SAINLogicDecision.DogFight;
                ///}
                ///else if (StartFightAction())
                ///{
                ///    Decision = SAINLogicDecision.Fight;
                ///}
                else
                {
                    Decision = SAINLogicDecision.Skirmish;
                }
            }

            return Decision;
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

        private bool CanShoot => BotOwner.Memory.GoalEnemy?.CanShoot == true;

        private bool IsVisible => BotOwner.Memory.GoalEnemy?.IsVisible == true;

        private bool StartSuppression
        {
            get
            {
                return false;

                if (BotOwner.Memory.GoalEnemy != null)
                {
                    if (BotOwner.WeaponManager.IsReady && BotOwner.WeaponManager.HaveBullets && !LowAmmo)
                    {
                        if (!CanShoot)
                        {
                            if (Time.time - BotOwner.Memory.GoalEnemy.TimeLastSeenReal > 2f)
                            {
                                var realPos = BotOwner.Memory.GoalEnemy.CurrPosition;
                                var lastSeenPos = BotOwner.Memory.GoalEnemy.EnemyLastPosition;

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
        }

        private bool StartRunForCover => false;

        private bool StartMoveToCover()
        {
            return !SAIN.Cover.BotIsAtCoverPoint;
        }

        private bool StartSeekEnemy => !SAIN.HasEnemyAndCanShoot && BotOwner.Memory.GoalEnemy.TimeLastSeen < Time.time - TimeBeforeSearch;

        private bool StartHoldInCover
        {
            get
            {
                if (SAIN.HasEnemyAndCanShoot)
                {
                    var lean = SAIN.Lean.Lean.RayCast;
                    //if (SAIN.Cover.CheckSelfForCover(0.33f))
                    //{
                    //    return true;
                    //}
                    if ((lean.LeftHalfLos || lean.LeftLos) && !lean.RightLos && !lean.DirectLineOfSight)
                    {
                        return true;
                    }
                    else if ((lean.RightHalfLos || lean.RightLos) && !lean.LeftLos && !lean.DirectLineOfSight)
                    {
                        return true;
                    }
                    else if (SAIN.Cover.BotIsAtCoverPoint)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private bool StartRunAway => false;

        private bool StartSurgery
        {
            get
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
                        var status = SAIN.BotStatus;
                        float distance = Vector3.Distance(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.EnemyLastPosition);

                        if (status.Injured && distance > 80f)
                        {
                            BotShouldHeal = true;
                        }
                        else if (status.BadlyInjured && distance > 50f)
                        {
                            BotShouldHeal = true;
                        }
                        else if (status.Dying && distance > 40f)
                        {
                            BotShouldHeal = true;
                        }
                    }
                }

                return BotShouldHeal;
            }
        }

        private const float LowAmmoThresh0to1 = 0.4f;

        private bool StartUseStims
        {
            get
            {
                bool takeStims = false;
                if (!BotOwner.Medecine.Stimulators.Using && BotOwner.Medecine.Stimulators.HaveSmt && LastStimTime < Time.time)
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
                        float dist = Vector3.Distance(BotOwner.Memory.GoalEnemy.CurrPosition, BotOwner.Position);

                        if (status.Injured)
                        {
                            if (!SAIN.HasEnemyAndCanShoot)
                            {
                                if (dist > 30f)
                                {
                                    takeStims = true;
                                }
                            }
                            else if (dist > 100f)
                            {
                                takeStims = true;
                            }
                        }
                        else if (status.BadlyInjured)
                        {
                            if (!SAIN.HasEnemyAndCanShoot)
                            {
                                if (dist > 10f)
                                {
                                    takeStims = true;
                                }
                            }
                            else if (dist > 30f)
                            {
                                takeStims = true;
                            }
                        }
                        else if (status.Dying)
                        {
                            if (!SAIN.HasEnemyAndCanShoot)
                            {
                                takeStims = true;
                            }
                            else if (dist > 10f)
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
        }

        private bool StartFirstAid
        {
            get
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
                        float dist = Vector3.Distance(BotOwner.Memory.GoalEnemy.CurrPosition, BotOwner.Position);

                        if (status.Injured)
                        {
                            if (!SAIN.HasEnemyAndCanShoot && (dist > 100f))
                            {
                                BotShouldHeal = true;
                            }
                        }
                        else if (status.BadlyInjured)
                        {
                            if (!SAIN.HasEnemyAndCanShoot)
                            {
                                if (dist > 20f)
                                {
                                    BotShouldHeal = true;
                                }
                            }
                            else if (dist > 100f)
                            {
                                BotShouldHeal = true;
                            }
                        }
                        else if (status.Dying)
                        {
                            if (!SAIN.HasEnemyAndCanShoot)
                            {
                                BotShouldHeal = true;
                            }
                            else if (dist > 30f)
                            {
                                BotShouldHeal = true;
                            }
                        }
                    }
                }

                return BotShouldHeal;
            }
        }

        private bool ShouldCancelReload
        {
            get
            {
                if (!BotOwner.WeaponManager.IsReady)
                {
                    return false;
                }

                bool botShouldCancel = false;

                if (BotOwner.Memory.GoalEnemy != null && BotOwner.WeaponManager.Reload.Reloading && SAIN.Enemies.PriorityEnemy != null)
                {
                    if (!LowAmmo && SAIN.Enemies.PriorityEnemy.Path.RangeClose)
                    {
                        if (DebugMode)
                        {
                            Logger.LogDebug($"My Enemy is Close, and I have ammo!");
                        }

                        botShouldCancel = true;
                    }
                    if (BotOwner.WeaponManager.HaveBullets && SAIN.Enemies.PriorityEnemy.Path.RangeVeryClose)
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
        }

        private bool StartBotReload
        {
            get
            {
                if (!BotOwner.WeaponManager.IsReady || !BotOwner.WeaponManager.Reload.CanReload(true))
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