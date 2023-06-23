using BepInEx.Logging;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SAIN.Classes;
using SAIN.Classes.Sense;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SAIN.UserSettings.VisionConfig;

namespace SAIN.Components
{
    public class SAINComponent : MonoBehaviour
    {
        public List<Player> VisiblePlayers = new List<Player>();

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
            Init(BotOwner);
            BotOwner.BotsGroup.OnReportEnemy += ReportEnemy;
        }

        public void ReportEnemy(IAIDetails person, Vector3 enemyPos, Vector3 weaponRootLast, EEnemyPartVisibleType isVisibleOnlyBySense)
        {
            if (person != null)
            {
                string id = person.ProfileId;
                if (!Enemies.ContainsKey(id))
                {
                    //SAINEnemy person = new SAINEnemy(BotOwner, person, 1f);
                    //Enemies.Add(id, person);
                }
            }
        }

        public List<Vector3> ExitsToLocation { get; private set; } = new List<Vector3>();

        public void UpdateExitsToLoc(Vector3[] exits)
        {
            ExitsToLocation.Clear();
            ExitsToLocation.AddRange(exits);
        }

        public string ProfileId => BotOwner.ProfileId;
        public string SquadId => Squad.SquadID;
        public Player Player => BotOwner.GetPlayer;

        private void Init(BotOwner bot)
        {
            BotColor = RandomColor;

            // Must be first, other classes use it
            Squad = new SquadClass(bot);
            Equipment = new BotEquipmentClass(bot);

            Info = new BotInfoClass(bot);
            Vision = new VisionClass(bot);
            AILimit = new AILimitClass(bot);
            BotStuck = new BotUnstuckClass(bot);
            Hearing = new HearingSensorClass(bot);
            Talk = new BotTalkClass(bot);
            Decision = new DecisionClass(bot);
            Cover = new CoverClass(bot);
            FlashLight = bot.GetPlayer.gameObject.AddComponent<FlashLightComponent>();
            SelfActions = new SelfActionClass(bot);
            Steering = new SteeringClass(bot);
            Grenade = new BotGrenadeClass(bot);
            Mover = new SAIN_Mover(bot);
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public bool NoBushESPActive { get; private set; }
        public SAINBotController BotController => SAINPlugin.BotController;


        private void Update()
        {
            UpdatePatrolData();

            if (BotActive && !GameIsEnding)
            {
                UpdateNoBushESP();
                DebugVisibleDraw();
                UpdateHealth();
                UpdateEnemy();

                BotOwner.DoorOpener.Update();
                Vision.Update();
                Equipment.Update();
                Grenade.Update();
                Mover.Update();
                Squad.Update();
                Info.Update();
                BotStuck.Update();
                Decision.Update();
                Cover.Update();
                Talk.Update();
                SelfActions.Update();
                Steering.Update();
            }
        }

        private void UpdateNoBushESP()
        {
            if (Enemy != null)
            {
                if (NoBushTimer < Time.time)
                {
                    NoBushTimer = Time.time + 0.25f;
                    NoBushESPActive = NoBushESP();
                }
                if (NoBushESPActive)
                {
                    BotOwner.ShootData?.EndShoot();
                    BotOwner.AimingData?.LoseTarget();
                }
            }
            else
            {
                NoBushESPActive = false;
            }
        }

        private float NoBushTimer = 0f;

        private void DebugVisibleDraw()
        {
            if (VisiblePlayers.Count > 0 && DebugVision.Value && VisiblePlayerTimer < Time.time)
            {
                VisiblePlayerTimer = Time.time + 0.5f;
                foreach (var player in VisiblePlayers)
                {
                    DebugGizmos.SingleObjects.Line(HeadPosition, player.MainParts[BodyPartType.body].Position, Color.green, 0.025f, true, 0.5f, true);
                }
            }
            if (Squad.VisibleMembers != null && Squad.VisibleMembers.Count > 0 && DebugVision.Value && VisibleSquadTimer < Time.time)
            {
                VisibleSquadTimer = Time.time + 1f;
                foreach (var player in Squad.VisibleMembers)
                {
                    DebugGizmos.SingleObjects.Line(HeadPosition, player.BodyPosition, Color.yellow, 0.025f, true, 1f, true);
                }
            }
            if (Enemies.Count > 0 && DebugVision.Value && VisibleEnemyTimer < Time.time)
            {
                VisibleEnemyTimer = Time.time + 0.33f;
                foreach (var enemy in Enemies.Values)
                {
                    if (enemy.CanShoot)
                    {
                        DebugGizmos.SingleObjects.Line(WeaponRoot, enemy.EnemyChestPosition, Color.red, 0.05f, true, 0.33f, true);
                    }
                    else
                    {
                        DebugGizmos.SingleObjects.Line(WeaponRoot, enemy.EnemyChestPosition, Color.red, 0.01f, true, 0.33f, true);
                    }
                    if (enemy.InLineOfSight)
                    {
                        DebugGizmos.SingleObjects.Line(HeadPosition, enemy.EnemyChestPosition, Color.blue, 0.05f, true, 0.33f, true);
                    }
                    else
                    {
                        DebugGizmos.SingleObjects.Line(HeadPosition, enemy.EnemyChestPosition, Color.blue, 0.01f, true, 0.33f, true);
                    }
                }
            }
        }

        private float VisiblePlayerTimer;
        private float VisibleSquadTimer;
        private float VisibleEnemyTimer;

        private void UpdatePatrolData()
        {
            if (CheckActiveLayer)
            {
                BotOwner.PatrollingData.Pause();
            }
            else
            {
                if (Enemy == null)
                {
                    BotOwner.PatrollingData.Unpause();
                }
            }
        }

        public bool LayersActive => BigBrainSAIN.IsBotUsingSAINLayer(BotOwner);

        public bool CheckActiveLayer
        {
            get
            {
                if (RecheckTimer < Time.time)
                {
                    if (LayersActive)
                    {
                        RecheckTimer = Time.time + 1f;
                        Active = true;
                    }
                    else
                    {
                        RecheckTimer = Time.time + 0.05f;
                        Active = false;
                    }
                }
                return Active;
            }
        }

        private bool Active;
        private float RecheckTimer = 0f;
        private bool PatrolPaused { get; set; }
        private float DebugPatrolTimer = 0f;

        private void UpdateEnemy()
        {
            if (ClearEnemyTimer < Time.time)
            {
                ClearEnemyTimer = Time.time + 1f;
                ClearEnemies();
            }

            if (BotOwner.BotsGroup.Enemies.Count > 0)
            {
                foreach (var person in BotOwner.BotsGroup.Enemies.Keys)
                {
                    string id = person.ProfileId;
                    if (!Enemies.ContainsKey(id))
                    {
                        SAINEnemy sainEnemy = new SAINEnemy(BotOwner, person, 1f);
                        Enemies.Add(id, sainEnemy);
                    }
                }
            }

            Enemy = PickClosestEnemy();
            Enemy?.Update();
        }

        private float ClearEnemyTimer;

        private void ClearEnemies()
        {
            if (Enemies.Count > 0)
            {
                foreach (string id in Enemies.Keys)
                {
                    var enemy = Enemies[id];
                    if (enemy == null || enemy.EnemyPlayer == null || enemy.EnemyPlayer?.HealthController?.IsAlive == false)
                    {
                        EnemyIDsToRemove.Add(id);
                    }
                }

                foreach (string idToRemove in EnemyIDsToRemove)
                {
                    Enemies.Remove(idToRemove);
                }

                EnemyIDsToRemove.Clear();
            }
        }

        private readonly List<string> EnemyIDsToRemove = new List<string>();

        private SAINEnemy PickClosestEnemy()
        {
            SAINEnemy ChosenEnemy = null;

            SAINEnemy closestLos = null;
            SAINEnemy closestAny = null;
            SAINEnemy closestVisible = null;

            float closestDist = Mathf.Infinity;
            float closestAnyDist = Mathf.Infinity;
            float closestVisibleDist = Mathf.Infinity;
            float enemyDist;

            if (Enemies.Count > 1)
            {
                foreach (var enemy in Enemies.Values)
                {
                    if (enemy != null)
                    {
                        enemyDist = (enemy.Position - Position).sqrMagnitude;
                        if (enemy.EnemyLookingAtMe && enemy.IsVisible)
                        {
                            if (enemyDist < closestVisibleDist)
                            {
                                closestVisibleDist = enemyDist;
                                closestVisible = enemy;
                            }
                        }
                        else if (enemy.InLineOfSight)
                        {
                            if (enemyDist < closestDist)
                            {
                                closestDist = enemyDist;
                                closestLos = enemy;
                            }
                        }
                        else
                        {
                            if (enemyDist < closestAnyDist)
                            {
                                closestAnyDist = enemyDist;
                                closestAny = enemy;
                            }
                        }
                    }
                }
                if (closestVisible != null)
                {
                    ChosenEnemy = closestVisible;
                }
                else if (closestLos != null)
                {
                    ChosenEnemy = closestLos;
                }
                else
                {
                    ChosenEnemy = closestAny;
                }
            }
            else if (Enemies.Count == 1)
            {
                ChosenEnemy = Enemies.Values.PickRandom();
            }
            return ChosenEnemy;
        }

        public Vector3? ExfilPosition { get; set; }

        private void UpdateHealth()
        {
            if (UpdateHealthTimer < Time.time)
            {
                UpdateHealthTimer = Time.time + 0.25f;
                HealthStatus = BotOwner.GetPlayer.HealthStatus;
            }
        }

        public FriendlyFireStatus FriendlyFireStatus { get; private set; }

        private float UpdateHealthTimer = 0f;
        public float DifficultyModifier => Info.DifficultyModifier;

        public bool HasEnemy => Enemy != null;

        public SAINEnemy Enemy { get; private set; }

        public Dictionary<string, SAINEnemy> Enemies { get; private set; } = new Dictionary<string, SAINEnemy>();

        public void Dispose()
        {
            StopAllCoroutines();

            BotOwner.BotsGroup.OnReportEnemy -= ReportEnemy;

            Cover?.Dispose();
            Hearing?.Dispose();
            Cover?.CoverFinder?.Dispose();
            FlashLight?.Dispose();

            Destroy(this);
        }

        public bool NoDecisions => CurrentDecision == SAINSoloDecision.None && Decision.SquadDecision == SAINSquadDecision.None && Decision.SelfDecision == SAINSelfDecision.None;
        public SAINSoloDecision CurrentDecision => Decision.MainDecision;
        public Vector3 Position => BotOwner.Position;
        public Vector3 WeaponRoot => BotOwner.WeaponRoot.position;
        public Vector3 HeadPosition => BotOwner.LookSensor._headPoint;
        public Vector3 BodyPosition => BotOwner.MainParts[BodyPartType.body].Position;

        public bool NoBushESP()
        {
            var enemy = Enemy;
            if (enemy != null && enemy.IsVisible)
            {
                if (enemy.EnemyPlayer?.IsYourPlayer == true)
                {
                    Vector3 direction = enemy.EnemyHeadPosition - HeadPosition;
                    if (Physics.Raycast(HeadPosition, direction, out var hit, direction.magnitude, NoBushMask))
                    {
                        string ObjectName = hit.transform?.parent?.gameObject?.name;
                        foreach (string exclusion in ExclusionList)
                        {
                            if (ObjectName.ToLower().Contains(exclusion))
                            {
                                if (DebugVision.Value)
                                {
                                    Logger.LogWarning("No Bush ESP ACTIVE");
                                    DebugGizmos.SingleObjects.Line(hit.point, HeadPosition, Color.green, 0.1f, true, 0.25f, true);
                                }
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static LayerMask NoBushMask => LayerMaskClass.HighPolyWithTerrainMaskAI;
        public static List<string> ExclusionList = new List<string> { "filbert", "fibert", "tree", "pine", "plant", "birch",
        "timber", "spruce", "bush", "wood", "grass" };


        public Vector3? CurrentTargetPosition
        {
            get
            {
                if (Enemy != null)
                {
                    return Enemy.Position;
                }
                var Target = BotOwner.Memory.GoalTarget?.GoalTarget;
                if (Target != null)
                {
                    return Target.Position;
                }
                if (Time.time - BotOwner.Memory.LastTimeHit < 20f && !BotOwner.Memory.IsPeace)
                {
                    return BotOwner.Memory.LastHitPos;
                }
                var sound = BotOwner.BotsGroup.YoungestPlace(BotOwner, 500f, true);
                if (sound != null && !sound.IsCome)
                {
                    return sound.Position;
                }
                return null;
            }
        }

        public bool HasGoalTarget => BotOwner.Memory.GoalTarget?.GoalTarget != null;
        public bool HasGoalEnemy => BotOwner.Memory.GoalEnemy != null;
        public Vector3? GoalTargetPos => BotOwner.Memory.GoalTarget?.GoalTarget?.Position;
        public Vector3? GoalEnemyPos => BotOwner.Memory.GoalEnemy?.CurrPosition;

        public bool BotHasStamina => BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0f;

        public Vector3 UnderFireFromPosition { get; set; }

        public bool HasEnemyAndCanShoot => Enemy?.IsVisible == true;

        public VisionClass Vision { get; private set; }

        public BotEquipmentClass Equipment { get; private set; }

        public SAIN_Mover Mover { get; private set; }

        public AILimitClass AILimit { get; private set; }

        public BotUnstuckClass BotStuck { get; private set; }

        public FlashLightComponent FlashLight { get; private set; }

        public HearingSensorClass Hearing { get; private set; }

        public BotTalkClass Talk { get; private set; }

        public DecisionClass Decision { get; private set; }

        public CoverClass Cover { get; private set; }

        public BotInfoClass Info { get; private set; }

        public SquadClass Squad { get; private set; }

        public SelfActionClass SelfActions { get; private set; }

        public BotGrenadeClass Grenade { get; private set; }

        public SteeringClass Steering { get; private set; }

        public bool IsDead => BotOwner?.IsDead == true;
        public bool BotActive => BotOwner.BotState == EBotState.Active && !IsDead && BotOwner?.GetPlayer?.enabled == true;
        public bool GameIsEnding => GameHasEnded || Singleton<IBotGame>.Instance?.Status == GameStatus.Stopping;
        public bool GameHasEnded => Singleton<IBotGame>.Instance == null;

        public bool Healthy => HealthStatus == ETagStatus.Healthy;
        public bool Injured => HealthStatus == ETagStatus.Injured;
        public bool BadlyInjured => HealthStatus == ETagStatus.BadlyInjured;
        public bool Dying => HealthStatus == ETagStatus.Dying;

        public ETagStatus HealthStatus { get; private set; }

        public LastHeardSound LastHeardSound => Hearing.LastHeardSound;

        public Color BotColor { get; private set; }

        public BotOwner BotOwner { get; private set; }

        private static Color RandomColor => new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

        private ManualLogSource Logger;
    }
}