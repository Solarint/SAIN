using BepInEx.Logging;
using EFT;
using UnityEngine;
using static SAIN.Helpers.EFT_Math;

namespace SAIN.Layers.Logic
{
    public class UpdateTarget : SAINBot
    {
        protected ManualLogSource Logger;

        public UpdateTarget(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            UpdateShoot = new UpdateShoot(bot);
        }

        public void ManualUpdate()
        {
            AimingData = BotOwner.AimingData;

            if (BotOwner.Memory.GoalEnemy.Distance < 20f)
            {
                LightTime = Time.time;
                BotOwner.BotLight.TurnOn();
            }
            else if (LightTime < Time.time - 2f)
            {
                BotOwner.BotLight.TurnOff();
            }

            Vector3? pointToShoot = GetPointToShoot();

            if (pointToShoot != null)
            {
                BotTarget = pointToShoot.Value;

                if (AimingData.IsReady)
                {
                    ReadyToShoot();

                    UpdateShoot.ManualUpdate();
                }
            }
        }

        private float LightTime = 0f;

        protected void ReadyToShoot()
        {
        }

        protected Vector3? GetTarget()
        {
            float ShootToCenter = BotOwner.Settings.FileSettings.Aiming.DIST_TO_SHOOT_TO_CENTER;
            var goalEnemy = BotOwner.Memory.GoalEnemy;

            if (goalEnemy != null)
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
                    TalkDelay = Time.time + Random(10f, 15f);

                    BotOwner.BotTalk.TrySay(EPhraseTrigger.OnFight, ETagStatus.Combat, true);
                }

                AimingData.SetTarget(BotTarget);

                AimingData.NodeUpdate();

                return new Vector3?(BotTarget);
            }
            return null;
        }

        private readonly UpdateShoot UpdateShoot;
        private GInterface5 AimingData;
        protected Vector3 BotTarget;
        private float TalkDelay;
    }
}