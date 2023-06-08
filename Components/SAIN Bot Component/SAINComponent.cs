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
            BotStuck = new BotUnstuckClass(bot);

            Enemy = bot.GetOrAddComponent<EnemyComponent>();
            HearingSensor = bot.GetOrAddComponent<AudioComponent>();

            BotSquad = new SquadClass(bot);
            Talk = bot.GetOrAddComponent<BotTalkComponent>();

            Lean = bot.GetOrAddComponent<LeanComponent>();
            Decisions = bot.GetOrAddComponent<DecisionComponent>();
            Cover = bot.GetOrAddComponent<CoverComponent>();
            FlashLight = bot.GetPlayer.gameObject.AddComponent<FlashLightComponent>();

            BotStatus = new StatusClass(bot);
            SelfActions = new SelfActionClass(bot);
            Movement = new MovementClass(bot);
            Dodge = new DodgeClass(bot);
            Steering = new SteeringClass(bot);
            Grenade = new BotGrenadeClass(bot);

            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public float TimeSinceStuck => BotStuck.TimeSinceStuck;
        public float TimeStuck => BotStuck.TimeStuck;

        private void Update()
        {
            if (BotActive && !GameIsEnding)
            {
                if (SelfCheckTimer < Time.time)
                {
                    //SelfCheckTimer = Time.time + 0.25f;
                    SelfActions.Activate();
                    BotOwner.WeaponManager.UpdateWeaponsList();
                }

                BotStuck.Update();
                BotSquad.Update();
                Info.Update();
            }
        }

        public bool BotIsAtDestination => (BotOwner.Position - BotOwner.Mover.RealDestPoint).magnitude < 1;
        public bool EnemyCanShoot => Enemy.SAINEnemy?.CanShoot == true;
        public bool EnemyInLineOfSight => Enemy.SAINEnemy?.InLineOfSight == true;
        public bool EnemyIsVisible => Enemy.SAINEnemy?.IsVisible == true;
        public bool HasEnemy => Enemy.SAINEnemy != null;
        public bool BotIsStuck => BotStuck.BotIsStuck;

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

            Talk.Dispose();
            HearingSensor.Dispose();
            Lean.Dispose();
            Cover.Dispose();
            Decisions.Dispose();
            Enemy.Dispose();
            FlashLight.Dispose();

            Destroy(this);
        }

        public SAINLogicDecision CurrentDecision => Decisions.CurrentDecision;

        public Vector3 Position => BotOwner.Position;
        public Vector3 WeaponRoot => BotOwner.WeaponRoot.position;
        public Vector3 HeadPosition => BotOwner.LookSensor._headPoint;
        public Vector3 BodyPosition => BotOwner.MainParts[BodyPartType.body].Position;

        public Vector3? CurrentTargetPosition => GoalEnemyPos ?? GoalTargetPos;

        public bool HasAnyTarget => HasGoalEnemy || HasGoalTarget;
        public bool HasGoalTarget => BotOwner.Memory.GoalTarget?.GoalTarget != null;
        public bool HasGoalEnemy => BotOwner.Memory.GoalEnemy != null;
        public Vector3? GoalTargetPos => BotOwner.Memory.GoalTarget?.GoalTarget?.Position;
        public Vector3? GoalEnemyPos => BotOwner.Memory.GoalEnemy?.CurrPosition;

        public Vector3 MidPoint(Vector3 target, float lerpVal = 0.5f)
        {
            return Vector3.Lerp(BotOwner.Position, target, lerpVal);
        }

        public bool BotHasStamina => BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0f;

        public Vector3 UnderFireFromPosition { get; set; }

        public bool BotIsMoving => BotStuck.BotIsMoving;

        public bool HasEnemyAndCanShoot => BotOwner.Memory.GoalEnemy?.CanShoot == true && BotOwner.Memory.GoalEnemy?.IsVisible == true;

        public BotUnstuckClass BotStuck { get; private set; }

        public FlashLightComponent FlashLight { get; private set; }

        public AudioComponent HearingSensor { get; private set; }

        public BotTalkComponent Talk { get; private set; }

        public DecisionComponent Decisions { get; private set; }

        public CoverComponent Cover { get; private set; }

        public EnemyComponent Enemy { get; private set; }

        public BotInfoClass Info { get; private set; }

        public SquadClass BotSquad { get; private set; }

        public StatusClass BotStatus { get; private set; }

        public SelfActionClass SelfActions { get; private set; }

        public LeanComponent Lean { get; private set; }

        public BotGrenadeClass Grenade { get; private set; }

        public MovementClass Movement { get; private set; }

        public DodgeClass Dodge { get; private set; }

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

        public LastHeardSound LastHeardSound => HearingSensor.LastHeardSound;

        public Color BotColor { get; private set; }

        public BotOwner BotOwner { get; private set; }

        private static Color RandomColor => new Color(Random.value, Random.value, Random.value);

        private float SelfCheckTimer = 0f;

        private ManualLogSource Logger;
    }
}