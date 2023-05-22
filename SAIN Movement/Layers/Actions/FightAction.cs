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

                if (BotOwner.Memory.GoalEnemy.CanShoot)
                {
                    /*
                    if (!SAIN.Core.Enemy.CanShoot && SAIN.Cover.Component.CurrentCover != null && SAIN.InCover && Vector3.Distance(SAIN.Cover.Component.CurrentCover.Position, BotOwner.Transform.position) < 1f)
                    {
                        var move = BotOwner.GetPlayer.MovementContext;
                        if (SAIN.Cover.Component.CurrentCover.Height <= 1.55f)
                        {
                            move.SetBlindFire(1);
                            Logger.LogWarning($"Bot is trying to blind fire! 1 [{move.BlindFire}]");
                        }
                        else if (Vector3.Dot(BotOwner.GetPlayer.Transform.right, BotOwner.Memory.GoalEnemy.CurrPosition) > 0)
                        {
                            move.SetBlindFire(-1);
                            Logger.LogWarning($"Bot is trying to blind fire! 2 [{move.BlindFire}]");
                        }
                        else
                        {
                            move.SetBlindFire(0);
                        }
                    }
                    */
                }
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