using BepInEx.Logging;
using EFT;
using UnityEngine;
using static SAIN.Helpers.EFT_Math;
using static SAIN.UserSettings.DebugConfig;
using SAIN.Components;

namespace SAIN.Layers.Logic
{
    public class UpdateShoot : SAINBotExt
    {
        public UpdateShoot(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        public void ManualUpdate()
        {
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
            if (SilentUntil < Time.time && IsTrue100(3f))
            {
                BotOwner.BotTalk.TrySay(EPhraseTrigger.OnFight, true);
                SilentUntil = Time.time + 15f;
            }
        }

        public float SilentUntil;
        public bool WithTalk = true;
        private int Bullets;
        protected ManualLogSource Logger;
    }
}