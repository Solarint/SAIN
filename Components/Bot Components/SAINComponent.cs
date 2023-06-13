using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Classes;
using SAIN.Helpers;
using System.Collections.Generic;
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
        }

        public string ProfileId => BotOwner.ProfileId;
        public string SquadId => Squad.SquadID;
        public Player Player => BotOwner.GetPlayer;

        private void Init(BotOwner bot)
        {
            BotColor = RandomColor;

            // Must be first, other classes use it
            Squad = new SquadClass(bot);

            Info = new BotInfoClass(bot);
            AILimit = new AILimitClass(bot);
            BotStuck = new BotUnstuckClass(bot);
            Hearing = new HearingSensorClass(bot);
            Talk = new BotTalkClass(bot);
            Lean = bot.GetOrAddComponent<LeanComponent>();
            Decision = new DecisionClass(bot);
            Cover = new CoverClass(bot);
            FlashLight = bot.GetPlayer.gameObject.AddComponent<FlashLightComponent>();
            SelfActions = new SelfActionClass(bot);
            Steering = new SteeringClass(bot);
            Grenade = new BotGrenadeClass(bot);
            Mover = new MoverClass(bot);
            FriendlyFire = new FriendlyFireClass(bot);
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public Player GoalEnemyPlayer => BotOwner.Memory.GoalEnemy?.Person?.GetPlayer;

        private void Update()
        {
            if (BotActive && !GameIsEnding)
            {
                if (VisiblePlayers.Count > 0 && DebugVision.Value)
                {
                    foreach (var player in VisiblePlayers)
                    {
                        DebugGizmos.SingleObjects.Line(HeadPosition, player.MainParts[BodyPartType.body].Position, Color.blue, 0.025f, true, 0.1f, true);
                    }
                }

                FriendlyFire.Update();
                UpdateHealth();
                UpdateEnemy();
                Mover.Update();
                Squad.Update();
                Info.Update();
                BotStuck.Update();
                Decision.Update();
                Cover.Update();
                Talk.Update();
                SelfActions.Update();
            }
        }

        private void UpdateEnemy()
        {
            var goalEnemy = BotOwner.Memory.GoalEnemy;
            if (goalEnemy != null)
            {
                SAINEnemy sainEnemy;
                string profile = goalEnemy.Person.ProfileId;
                if (Enemies.ContainsKey(profile))
                {
                    sainEnemy = Enemies[profile];
                }
                else
                {
                    sainEnemy = new SAINEnemy(BotOwner, goalEnemy.Person, DifficultyModifier);
                    Enemies.Add(profile, sainEnemy);
                }
                Enemy = sainEnemy;
                Enemy?.Update();
            }
            else
            {
                Enemy = null;
                Enemies.Clear();
            }
        }

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
            Lean?.Dispose();
            Cover?.CoverFinder?.Dispose();
            FlashLight?.Dispose();
            Destroy(this);
        }

        public bool NoDecisions => CurrentDecision == SAINSoloDecision.None && Decision.SquadDecision == SAINSquadDecision.None && Decision.SelfDecision == SAINSelfDecision.None;
        public SAINSoloDecision CurrentDecision => Decision.MainDecision;
        public float DistanceToMainPlayer => (Plugin.MainPlayerPosition - BotOwner.Position).magnitude;
        public Vector3 Position => BotOwner.Position;
        public Vector3 WeaponRoot => BotOwner.WeaponRoot.position;
        public Vector3 HeadPosition => BotOwner.LookSensor._headPoint;
        public Vector3 BodyPosition => BotOwner.MainParts[BodyPartType.body].Position;

        public Vector3? CurrentTargetPosition => GoalEnemyPos ?? GoalTargetPos;

        public bool HasGoalTarget => BotOwner.Memory.GoalTarget?.GoalTarget != null;
        public bool HasGoalEnemy => BotOwner.Memory.GoalEnemy != null;
        public Vector3? GoalTargetPos => BotOwner.Memory.GoalTarget?.GoalTarget?.Position;
        public Vector3? GoalEnemyPos => BotOwner.Memory.GoalEnemy?.CurrPosition;

        public bool BotHasStamina => BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0f;

        public Vector3 UnderFireFromPosition { get; set; }

        public bool HasEnemyAndCanShoot => Enemy?.IsVisible == true;

        public FriendlyFireClass FriendlyFire { get; private set; }

        public MoverClass Mover { get; private set; }

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

        public LeanComponent Lean { get; private set; }

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

        private static Color RandomColor => new Color(Random.value, Random.value, Random.value);

        private ManualLogSource Logger;
    }
}