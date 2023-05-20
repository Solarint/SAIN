using BepInEx.Logging;
using EFT;
using SAIN.Classes;
using UnityEngine;
using SAIN_Helpers;

namespace SAIN.Components
{
    public class SAINCoreComponent : MonoBehaviour
    {
        public Color BotColor;
        public SquadClass BotSquad { get; private set; }
        public MedicalClass Medical { get; private set; }
        public StatusClass BotStatus { get; private set; }
        public EnemyClass Enemy { get; private set; }
        public BotInfoClass Info { get; private set; }

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();

            Init();
        }

        private void Init()
        {
            BotColor = RandomColor;

            Info = new BotInfoClass(BotOwner);
            Medical = new MedicalClass(BotOwner);
            BotStatus = new StatusClass(BotOwner);
            Enemy = new EnemyClass(BotOwner);
            BotSquad = new SquadClass(BotOwner);
        }

        private readonly Timers Timers = new Timers();

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }

        public BotOwner BotOwner { get; private set; }

        private void Update()
        {
            if (BotOwner.GetPlayer?.HealthController?.IsAlive == false || BotOwner.BotState != EBotState.Active)
            {
                return;
            }

            BotSquad.ManualUpdate();

            // Check if a bot has meds or stims
            if (Timers.UpdateMedsTimer < Time.time)
            {
                Timers.UpdateMedsTimer = Time.time + Timers.UpdateMedsFreq;

                BotOwner.WeaponManager.UpdateWeaponsList();
                Info.SetPersonality();

                Medical.Update();
            }

            // Check bot health status
            if (Timers.CheckStatusTimer < Time.time)
            {
                Timers.CheckStatusTimer = Time.time + Timers.CheckStatusFreq;

                BotStatus.Update();
            }

            var goalEnemy = BotOwner.Memory.GoalEnemy;
            if (goalEnemy != null)
            {
                if (IsPointInVisibleSector(goalEnemy.CurrPosition))
                {
                    Enemy.Update(goalEnemy.Person);
                }
            }
            else
            {
                Enemy.CanShoot = false;
                Enemy.CanSee = false;
            }
        }

        public Vector3 Position => BotOwner.Transform.position;
        public Vector3 HeadPosition => BotOwner.MyHead.position;
        public Vector3 LookSensorPos => BotOwner.LookSensor._headPoint;

        public bool BotReloading => BotOwner.WeaponManager.IsReady && BotOwner.WeaponManager.Reload.Reloading;
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
                cos = (BotOwner.BotLight.IsEnable ? BotOwner.LookSensor.VISIBLE_ANGLE_LIGHT : 160f);
            }

            return SAIN_Math.IsAngLessNormalized(BotOwner.LookDirection, SAIN_Math.NormalizeFastSelf(direction), cos);
        }
    }

    public abstract class SAINBot
    {
        public SAINBot(BotOwner bot)
        {
            BotOwner = bot;
            Core = bot.GetComponent<SAINCoreComponent>();
        }

        protected SAINCoreComponent Core { get; private set; }
        protected BotOwner BotOwner { get; private set; }
    }

    public class Timers
    {
        public readonly float VisionRaycastFreq = Plugin.VisionRaycast.Value;
        public readonly float ShootRaycastFreq = Plugin.ShootRaycast.Value;
        public readonly float CheckStatusFreq = Plugin.CheckStatus.Value;
        public readonly float CheckPathFreq = Plugin.CheckPath.Value;
        public readonly float UpdateMedsFreq = Plugin.RefreshMeds.Value;
        public float VisionRaycastTimer = 0f;
        public float ShootRaycastTimer = 0f;
        public float CheckStatusTimer = 0f;
        public float CheckPathTimer = 0f;
        public float UpdateMedsTimer = 0f;
    }
}