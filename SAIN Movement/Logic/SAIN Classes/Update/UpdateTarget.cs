using BepInEx.Logging;
using EFT;
using UnityEngine;
using static SAIN_Helpers.SAIN_Math;
using static SAIN.UserSettings.DebugConfig;
using SAIN.Components;

namespace SAIN.Layers.Logic
{
    public class UpdateTarget : SAINBotExt
    {
        protected ManualLogSource Logger;

        public UpdateTarget(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            UpdateShoot = new UpdateShoot(bot);
        }

        public void ManualUpdate()
        {
            if (BotFightInterface == null)
            {
                BotFightInterface = BotOwner.AimingData;
            }

            if (BotOwner.Memory.GoalEnemy.Distance < 30f && SAIN.Core.Enemy.CanShoot)
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

                if (BotFightInterface.IsReady)
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

            if (goalEnemy != null && SAIN.Core.Enemy.CanShoot && SAIN.Core.Enemy.CanSee)
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

                    BotOwner.BotTalk.TrySay(EPhraseTrigger.MumblePhrase, ETagStatus.Combat, true);
                }

                BotFightInterface.SetTarget(BotTarget);

                BotFightInterface.NodeUpdate();

                return new Vector3?(BotTarget);
            }
            return null;
        }

        private readonly UpdateShoot UpdateShoot;
        private GInterface5 BotFightInterface;
        protected Vector3 BotTarget;
        private float TalkDelay;
    }
}