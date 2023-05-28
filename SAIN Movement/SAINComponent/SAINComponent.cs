using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using UnityEngine;
using SAIN.Classes;

namespace SAIN.Components
{
    public class SAINComponent : MonoBehaviour
    {
        public SAINLogicDecision CurrentDecision => Decisions.CurrentDecision;
        public Vector3 UnderFireFromPosition { get; set; }
        public Vector3 LastSoundHeardPosition { get; set; }
        public bool BotIsMoving { get; private set; }
        public bool BotActive => BotOwner.BotState == EBotState.Active && !BotOwner.IsDead;
        public bool BotUnderFire => BotOwner.Memory.IsUnderFire && Time.time - BotOwner.Memory.UnderFireTime > 1f;
        public bool InCover { get; private set; }
        public float CoverRatio { get; private set; }
        public bool HasEnemyAndCanShoot => BotOwner.Memory.GoalEnemy?.CanShoot == true && BotOwner.Memory.GoalEnemy?.IsVisible == true;

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
            Init(BotOwner);
        }

        private void Init(BotOwner bot)
        {
            BotColor = RandomColor;

            Info = new BotInfoClass(BotOwner);
            BotStatus = new StatusClass(BotOwner);
            Enemy = new EnemyClass(BotOwner);
            BotSquad = new SquadClass(BotOwner);

            HearingSensor = bot.GetOrAddComponent<AudioComponent>();
            Talk = bot.GetOrAddComponent<BotTalkComponent>();
            Lean = bot.GetOrAddComponent<LeanComponent>();
            Decisions = bot.GetOrAddComponent<DecisionComponent>();
            Cover = bot.GetOrAddComponent<CoverComponent>();

            Settings = new SettingsClass(bot);
            SelfActions = new SelfActionClass(bot);
            Movement = new MovementClass(bot);
            Dodge = new DodgeClass(bot);
            Steering = new SteeringClass(bot);
            Grenade = new GrenadeReactionClass(bot);
            DebugDrawList = new DebugGizmos.DrawLists(BotColor, BotColor);

            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private const float CheckSelfFreq = 0.2f;
        private const float CheckMoveFreq = 0.2f;

        private void Update()
        {
            if (BotActive)
            {
                if (CheckMoveTimer < Time.time)
                {
                    CheckMoveTimer = Time.time + CheckMoveFreq;
                    BotIsMoving = IsBotMoving;
                    LastPos = BotOwner.Position;
                }

                if (SelfCheckTimer < Time.time)
                {
                    SelfCheckTimer = Time.time + CheckSelfFreq;
                    SelfActions.Activate();
                }

                BotStatus.Update();
                BotSquad.ManualUpdate();
                BotOwner.WeaponManager.UpdateWeaponsList();
                Info.SetPersonality();

                var goalEnemy = BotOwner.Memory.GoalEnemy;
                if (goalEnemy != null)
                {
                    if (IsPointInVisibleSector(goalEnemy.CurrPosition))
                    {
                        Enemy.Update(goalEnemy.Person);
                    }
                }

                Settings.ManualUpdate();
            }
        }

        public Color BotColor;
        public SquadClass BotSquad { get; private set; }
        public StatusClass BotStatus { get; private set; }
        public EnemyClass Enemy { get; private set; }
        public BotInfoClass Info { get; private set; }

        public Vector3 Position => BotOwner.Transform.position;
        public Vector3 HeadPosition => BotOwner.MyHead.position;
        public Vector3 LookSensorPos => BotOwner.LookSensor._headPoint;

        public bool BotHasStamina => BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0f;

        public static LayerMask SightMask => LayerMaskClass.HighPolyWithTerrainMask;
        public static LayerMask ShootMask => LayerMaskClass.HighPolyWithTerrainMask;
        public static LayerMask CoverMask => LayerMaskClass.LowPolyColliderLayerMask;
        public static LayerMask FoliageMask => LayerMaskClass.AI;

        private static Color RandomColor => new Color(Random.value, Random.value, Random.value);

        public bool IsPointInVisibleSector(Vector3 position)
        {
            Vector3 direction = position - BotOwner.Position;

            float cos;
            if (BotOwner.NightVision.UsingNow)
            {
                cos = BotOwner.LookSensor.VISIBLE_ANGLE_NIGHTVISION;
            }
            else
            {
                cos = (BotOwner.BotLight.IsEnable ? BotOwner.LookSensor.VISIBLE_ANGLE_LIGHT : 180f);
            }

            return VectorHelpers.IsAngLessNormalized(BotOwner.LookDirection, VectorHelpers.NormalizeFastSelf(direction), cos);
        }

        public void Dispose()
        {
            StopAllCoroutines();

            Talk.Dispose();
            HearingSensor.Dispose();
            Lean.Dispose();
            Cover.Dispose();
            Decisions.Dispose();

            Destroy(this);
        }

        public AudioComponent HearingSensor { get; private set; }
        public BotTalkComponent Talk { get; private set; }
        public SelfActionClass SelfActions { get; private set; }
        public LeanComponent Lean { get; private set; }
        public GrenadeReactionClass Grenade { get; private set; }
        public MovementClass Movement { get; private set; }
        public DodgeClass Dodge { get; private set; }
        public DecisionComponent Decisions { get; private set; }
        public SteeringClass Steering { get; private set; }
        public DebugGizmos.DrawLists DebugDrawList { get; private set; }
        public CoverComponent Cover { get; private set; }
        public SettingsClass Settings { get; private set; }
        public BotOwner BotOwner { get; private set; }

        private bool IsBotMoving => Vector3.Distance(LastPos, BotOwner.Position) < 0.05f;
        private float CheckMoveTimer = 0f;
        private Vector3 LastPos = Vector3.zero;
        private float SelfCheckTimer = 0f;

        private ManualLogSource Logger;
    }
}