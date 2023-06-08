using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Layers
{
    internal class FightAction : CustomLogic
    {
        public FightAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            AimData = new GClass105(bot);
        }

        private readonly GClass105 AimData;

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();
        }

        public override void Stop()
        {
            BotOwner.PatrollingData.Unpause();
        }

        public override void Update()
        {
            SAIN.Movement.DecideMovementSpeed();

            if (SAIN.HasEnemyAndCanShoot)
            {
                AimData.Update();
            }

            SAIN.Steering.ManualUpdate();

            if (BotOwner.AimingData != null)
            {
                Vector3 position = BotOwner.GetPlayer.PlayerBones.WeaponRoot.position;
                Vector3 realTargetPoint = BotOwner.AimingData.RealTargetPoint;
                if (BotOwner.ShootData.Shooting && BotOwner.ShootData.ChecFriendlyFire(position, realTargetPoint))
                {
                    BotOwner.ShootData.EndShoot();
                }
            }
        }

        private ManualLogSource Logger;
    }
}