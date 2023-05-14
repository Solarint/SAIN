using BepInEx.Logging;
using EFT;
using UnityEngine;
using static SAIN_Helpers.SAIN_Math;
using static SAIN.UserSettings.DebugConfig;
using SAIN.Components;

namespace SAIN.Layers.Logic
{
    public class UpdateShoot
    {
        private readonly BotOwner BotOwner;
        private bool DebugMode => DebugUpdateShoot.Value;

        protected ManualLogSource Logger;

        public UpdateShoot(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            SAIN = bot.GetComponent<SAINCore>();
        }

        private SAINCore SAIN;

        public void ManualUpdate()
        {
            if (!BotOwner.WeaponManager.HaveBullets)
            {
                BotOwner.WeaponManager.Reload.TryReload();
                return;
            }

            Vector3 position = BotOwner.GetPlayer.PlayerBones.WeaponRoot.position;
            Vector3 realTargetPoint = BotOwner.AimingData.RealTargetPoint;
            if (BotOwner.ShootData.ChecFriendlyFire(position, realTargetPoint))
            {
                return;
            }

            if (WithTalk)
            {
                Talk();
            }

            if (BotOwner.ShootData.Shoot() && Bullets > BotOwner.WeaponManager.Reload.BulletCount)
            {
                Bullets = BotOwner.WeaponManager.Reload.BulletCount;
            }
        }

        private void Talk()
        {
            if (SilentUntil > Time.time)
            {
                return;
            }

            if (IsTrue100(10f))
            {
                BotOwner.BotTalk.TrySay(EPhraseTrigger.OnFight, true);
                SilentUntil = Time.time + 15f;
            }
        }

        public float SilentUntil;
        public bool WithTalk = true;
        private int Bullets;
    }
}