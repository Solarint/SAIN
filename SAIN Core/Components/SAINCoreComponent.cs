using BepInEx.Logging;
using EFT;
using SAIN.Classes;
using UnityEngine;

namespace SAIN.Components
{
    public class SAINCoreComponent : MonoBehaviour
    {
        public Color BotColor;
        public Medical Medical { get; private set; }
        public BotStatus BotStatus { get; private set; }
        public Enemy Enemy { get; private set; }

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
            BotColor = RandomColor;

            Medical = new Medical(BotOwner);
            BotStatus = new BotStatus(BotOwner);
            Enemy = new Enemy(BotOwner);
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

            // Check if a bot is under fire
            if (Timers.UnderFireTimer < Time.time && BotOwner.Memory.IsUnderFire)
            {
                Timers.UnderFireTimer = Time.time + Timers.UnderFireFreq;

                UnderFire_LastTime = Time.time;
                UnderFire_LastPosition = BotPosition;
            }

            // Check if a bot has meds or stims
            if (Timers.UpdateMedsTimer < Time.time)
            {
                Timers.UpdateMedsTimer = Time.time + Timers.UpdateMedsFreq;

                Medical.Update();
            }

            // Check bot health status
            if (Timers.CheckStatusTimer < Time.time)
            {
                Timers.CheckStatusTimer = Time.time + Timers.CheckStatusFreq;

                BotStatus.Update(BotOwner.GetPlayer.HealthStatus);
            }

            if (BotOwner.Memory.GoalEnemy != null)
            {
                Enemy.Update(BotPosition, BotOwner.Memory.GoalEnemy.Person);
            }
        }

        public float UnderFire_LastTime;

        public Vector3 UnderFire_LastPosition;

        public bool BotReloading => BotOwner.WeaponManager.Reload.Reloading;
        public bool BotHasStamina => BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0f;
        public bool BotIsMoving => BotOwner.Mover.IsMoving;
        public Vector3 BotPosition => BotOwner.Transform.position;
        public float PowerLevel => BotOwner.AIData.PowerOfEquipment;

        public static LayerMask SightMask => LayerMaskClass.HighPolyWithTerrainMaskAI;
        public static LayerMask ShootMask => LayerMaskClass.HighPolyWithTerrainMask;

        private static Color RandomColor => new Color(Random.value, Random.value, Random.value);
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
        public readonly float UnderFireFreq = Plugin.UnderFire.Value;
        public float VisionRaycastTimer = 0f;
        public float ShootRaycastTimer = 0f;
        public float CheckStatusTimer = 0f;
        public float CheckPathTimer = 0f;
        public float UpdateMedsTimer = 0f;
        public float UnderFireTimer = 0f;
    }
}