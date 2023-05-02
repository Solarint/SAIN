using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using Movement.Helpers;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
using static SAIN_Helpers.DebugDrawer;
using static SAIN_Helpers.SAIN_Math;

namespace SAIN.Movement.Layers.DogFight
{
    internal class UpdateShoot
    {
        private readonly BotOwner BotOwner;

        protected ManualLogSource Logger;

        public UpdateShoot(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
        }

        public void Update()
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

            if (IsTrue100(50f))
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