using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Classes;
using SAIN.Helpers;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Components
{
    public class SAINComponent : MonoBehaviour
    {
        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
            Init(BotOwner);
        }

        private void Init(BotOwner bot)
        {
            BotColor = RandomColor;

            Info = new BotInfoClass(bot);
            AILimit = new AILimitClass(bot);
            BotStuck = new BotUnstuckClass(bot);
            Hearing = new HearingSensorClass(bot);
            BotSquad = new SquadClass(bot);
            Talk = new BotTalkClass(bot);
            Lean = bot.GetOrAddComponent<LeanComponent>();
            Decision = new DecisionClass(bot);
            Cover = new CoverClass(bot);
            FlashLight = bot.GetPlayer.gameObject.AddComponent<FlashLightComponent>();
            SelfActions = new SelfActionClass(bot);
            Steering = new SteeringClass(bot);
            Grenade = new BotGrenadeClass(bot);
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public Player GoalEnemyPlayer => BotOwner.Memory.GoalEnemy?.Person?.GetPlayer;

        private void Update()
        {
            if (BotActive && !GameIsEnding)
            {
                AILimit.UpdateAILimit();

                if (AILimit.Enabled)
                {
                    return;
                }

                if (SelfCheckTimer < Time.time)
                {
                    SelfCheckTimer = Time.time + 0.1f;
                    SelfActions.DoSelfAction();
                    BotOwner.WeaponManager.UpdateWeaponsList();
                }

                Info.Update();
                UpdateEnemy();
                BotStuck.Update();
                BotSquad.Update();
                Decision.Update();
                Cover.Update();
                Talk.Update();
            }
        }

        private void UpdateEnemy()
        {
            if (!BotActive || GameIsEnding || BotOwner.Memory.GoalEnemy == null)
            {
                Enemy = null;
                return;
            }

            var person = BotOwner.Memory.GoalEnemy.Person;
            if (Enemy == null || Enemy?.Person != person)
            {
                Enemy = new SAINEnemy(BotOwner, person);
            }

            Enemy?.Update();
        }

        public bool HasEnemy => Enemy != null;

        public SAINEnemy Enemy { get; private set; }

        public bool ShiftAwayFromCloseWall(Vector3 target, out Vector3 newPos)
        {
            const float closeDist = 0.75f;

            if (CheckTooCloseToWall(target, out var rayHit, closeDist))
            {
                var direction = (BotOwner.Position - rayHit.point).normalized * 0.8f;
                direction.y = 0f;
                var movePoint = BotOwner.Position + direction;
                if (NavMesh.SamplePosition(movePoint, out var hit, 0.1f, -1))
                {
                    newPos = hit.position;
                    return true;
                }
            }
            newPos = Vector3.zero;
            return false;
        }

        public bool CheckTooCloseToWall(Vector3 target, out RaycastHit rayHit, float checkDist = 0.75f)
        {
            Vector3 botPos = BotOwner.Position;
            Vector3 direction = target - botPos;
            botPos.y = WeaponRoot.y;
            return Physics.Raycast(BotOwner.Position, direction, out rayHit, checkDist, LayerMaskClass.HighPolyWithTerrainMask);
        }

        public void Dispose()
        {
            StopAllCoroutines();
            Lean.Dispose();
            Cover.CoverFinder.Dispose();
            FlashLight.Dispose();
            Destroy(this);
        }

        public SAINLogicDecision CurrentDecision => Decision.CurrentDecision;

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

        public Vector3 MidPoint(Vector3 target, float lerpVal = 0.5f)
        {
            return Vector3.Lerp(BotOwner.Position, target, lerpVal);
        }
        public bool BotIsAtPoint(Vector3 point, float reachDist = 1f)
        {
            return DistanceToDestination(point) < reachDist;
        }

        public float DistanceToDestination(Vector3 point)
        {
            return Vector3.Distance(point, BotOwner.Transform.position);
        }

        public bool BotHasStamina => BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0f;

        public Vector3 UnderFireFromPosition { get; set; }

        public bool HasEnemyAndCanShoot => BotOwner.Memory.GoalEnemy?.CanShoot == true && BotOwner.Memory.GoalEnemy?.IsVisible == true;

        public AILimitClass AILimit { get; private set; }

        public BotUnstuckClass BotStuck { get; private set; }

        public FlashLightComponent FlashLight { get; private set; }

        public HearingSensorClass Hearing { get; private set; }

        public BotTalkClass Talk { get; private set; }

        public DecisionClass Decision { get; private set; }

        public CoverClass Cover { get; private set; }

        public BotInfoClass Info { get; private set; }

        public SquadClass BotSquad { get; private set; }

        public SelfActionClass SelfActions { get; private set; }

        public LeanComponent Lean { get; private set; }

        public BotGrenadeClass Grenade { get; private set; }

        public SteeringClass Steering { get; private set; }

        public bool BotActive => BotOwner.BotState == EBotState.Active && !BotOwner.IsDead && BotOwner.GetPlayer.enabled;

        public bool GameIsEnding
        {
            get
            {
                var game = Singleton<IBotGame>.Instance;
                if (game == null)
                {
                    return false;
                }

                return game.Status == GameStatus.Stopping;
            }
        }

        public bool Healthy => HealthStatus == ETagStatus.Healthy;
        public bool Injured => HealthStatus == ETagStatus.Injured;
        public bool BadlyInjured => HealthStatus == ETagStatus.BadlyInjured;
        public bool Dying => HealthStatus == ETagStatus.Dying;

        public ETagStatus HealthStatus => BotOwner.GetPlayer.HealthStatus;

        public LastHeardSound LastHeardSound => Hearing.LastHeardSound;

        public Color BotColor { get; private set; }

        public BotOwner BotOwner { get; private set; }

        private static Color RandomColor => new Color(Random.value, Random.value, Random.value);

        private float SelfCheckTimer = 0f;

        private ManualLogSource Logger;
    }
}