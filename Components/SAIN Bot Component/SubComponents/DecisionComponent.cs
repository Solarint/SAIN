using BepInEx.Logging;
using EFT;
using UnityEngine;
using SAIN.Classes;
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
                        Logger.LogInfo($"Time Before Search: [{TimeBeforeSearch}]");
                    }
                }
            }

            if (DecisionTimer < Time.time)
            {
                DecisionTimer = Time.time + DecisionFreq;

                LastDecision = CurrentDecision;
                CurrentDecision = GetDecision();
            }
        }

        public float GetTimeBeforeSearch()
        {
            var info = SAIN.Info;
            var pers = info.BotPersonality;
            var group = SAIN.BotSquad;

            float searchTime;

            if (info.IsFollower)
            {
                searchTime = 3f;
            }
            else if (info.IsBoss)
            {
                searchTime = 30f;
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
                        searchTime = 240f;
                        break;

                    default:
                        searchTime = 30f;
                        break;
                }

                if (group.IsSquadLead && group.BotInGroup)
                {
                    int count = group.SquadMembers.Count;
                    searchTime *= (count / 2f);
                }
            }

            searchTime *= Random.Range(1f - SearchRandomize, 1f + SearchRandomize);

            if (DebugMode)
            {
            Logger.LogDebug($"Search Time = [{searchTime}] because: IsBoss? [{info.IsBoss}] IsFollower? [{info.IsFollower}] Personality [{pers}] SquadLead? [{group.IsSquadLead}] Squad Members: [{group.SquadMembers.Count}]");

            }

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
            else if (StartFirstAid || BotOwner.Medecine.FirstAid.Using)
            {
                Decision = SAINLogicDecision.FirstAid;
            }
            else if (StartBotReload || BotOwner.WeaponManager.Reload.Reloading)
            {
                Decision = SAINLogicDecision.Reload;
            }
            else if (StartUseStims || BotOwner.Medecine.Stimulators.Using)
            {
                Decision = SAINLogicDecision.Stims;
            }
            else if (StartSurgery || BotOwner.Medecine.SurgicalKit.Using)
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
                else if (StartWalkToCover)
                {
                    Decision = SAINLogicDecision.WalkToCover;
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

        private bool StartWalkToCover => !SAIN.Cover.BotIsAtCoverPoint && SAIN.HasEnemyAndCanShoot;

        private bool StartSeekEnemy => !SAIN.HasEnemyAndCanShoot && BotOwner.Memory.GoalEnemy.TimeLastSeen < Time.time - TimeBeforeSearch;

        private bool StartHoldInCover
        {
            get
            {
                if (SAIN.HasEnemyAndCanShoot || (Time.time - BotOwner.Memory.GoalEnemy.PersonalLastSeenTime) < 3f)
                {
                    var lean = SAIN.Lean.Lean.RayCast;
                    if (SAIN.Cover.CheckSelfForCover(0.3f))
                    {
                        return true;
                    }
                    else if ((lean.LeftHalfLineOfSight || lean.LeftLineOfSight) && !lean.RightLineOfSight)
                    {
                        return true;
                    }
                    else if ((lean.RightHalfLineOfSight || lean.RightLineOfSight) && !lean.LeftLineOfSight)
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
            Destroy(this );
        }
    }
}