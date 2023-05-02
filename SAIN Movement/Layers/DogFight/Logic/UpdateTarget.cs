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
    internal class UpdateTarget
    {
        private readonly BotOwner BotOwner;

        protected ManualLogSource Logger;

        public UpdateTarget(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            updateShoot_0 = new UpdateShoot(bot);
        }

        public void Update()
        {
            if (BotFightInterface == null)
                BotFightInterface = BotOwner.AimingData;

            BotOwner.BotLight.TurnOn(BotFightInterface.AlwaysTurnOnLight);

            Vector3? pointToShoot = GetPointToShoot();

            if (pointToShoot != null)
            {
                BotTarget = pointToShoot.Value;

                if (BotFightInterface.IsReady)
                {
                    ReadyToShoot();

                    updateShoot_0.Update();
                }
            }
        }

        protected void ReadyToShoot()
        {
        }

        protected Vector3? GetTarget()
        {
            float ShootToCenter = BotOwner.Settings.FileSettings.Aiming.DIST_TO_SHOOT_TO_CENTER;
            var goalEnemy = BotOwner.Memory.GoalEnemy;

            if (goalEnemy != null && CanShootEnemyAndVisible)
            {
                Vector3 aimTarget;

                if (goalEnemy.Distance < ShootToCenter)
                {
                    aimTarget = goalEnemy.GetCenterPart();
                }
                else
                {
                    aimTarget = goalEnemy.GetPartToShoot();
                }

                return new Vector3?(aimTarget);
            }

            Vector3? neutralTarget = null;

            if (BotOwner.Memory.LastEnemy != null)
            {
                neutralTarget = new Vector3?(BotOwner.Memory.LastEnemy.CurrPosition + Vector3.up * BotOwner.Settings.FileSettings.Aiming.DANGER_UP_POINT);
            }

            return neutralTarget;
        }

        protected Vector3? GetPointToShoot()
        {
            Vector3? target = GetTarget();

            if (target != null)
            {
                BotTarget = target.Value;

                if (TalkDelay < Time.time)
                {
                    TalkDelay = Time.time + Random(5f, 8f);

                    BotOwner.BotTalk.TrySay(EPhraseTrigger.OnFight, true);
                }

                BotFightInterface.SetTarget(BotTarget);

                BotFightInterface.NodeUpdate();

                return new Vector3?(BotTarget);
            }
            return null;
        }

        public bool CanShootEnemyAndVisible => CanShootEnemy && CanSeeEnemy;
        public bool CanShootEnemy => BotOwner.Memory.GoalEnemy.IsVisible;
        public bool CanSeeEnemy => BotOwner.Memory.GoalEnemy.CanShoot;

        private readonly UpdateShoot updateShoot_0;
        private GInterface5 BotFightInterface;
        protected Vector3 BotTarget;
        private float TalkDelay;
    }
}