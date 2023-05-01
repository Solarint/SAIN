using EFT;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;

namespace Movement.Components
{
    public class asdbasd : MonoBehaviour
    {
        private BotOwner botOwner_0;

        public void Update()
        {
            if (BotFightInterface == null)
            {
                BotFightInterface = botOwner_0.AimingData;
            }

            botOwner_0.BotLight.TurnOn(BotFightInterface.AlwaysTurnOnLight);

            Vector3? pointToShoot = GetPointToShoot();

            if (pointToShoot != null)
            {
                BotTarget = pointToShoot.Value;

                if (BotFightInterface.IsReady)
                {
                    ReadyToShoot();

                    BotUpdate2.Update();
                }
            }
        }

        protected virtual void ReadyToShoot()
        {
        }

        protected virtual Vector3? GetTarget()
        {
            float ShootToCenter = botOwner_0.Settings.FileSettings.Aiming.DIST_TO_SHOOT_TO_CENTER;
            var goalEnemy = botOwner_0.Memory.GoalEnemy;

            if (goalEnemy != null && goalEnemy.CanShoot && goalEnemy.IsVisible)
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

            if (botOwner_0.Memory.LastEnemy != null)
            {
                neutralTarget = new Vector3?(botOwner_0.Memory.LastEnemy.CurrPosition + Vector3.up * botOwner_0.Settings.FileSettings.Aiming.DANGER_UP_POINT);
            }

            return neutralTarget;
        }

        protected virtual Vector3? GetPointToShoot()
        {
            Vector3? target = GetTarget();

            if (target != null)
            {
                BotTarget = target.Value;

                if (TalkDelay < Time.time)
                {
                    TalkDelay = Time.time + SAIN_Math.Random(5f, 8f);

                    botOwner_0.BotTalk.TrySay(EPhraseTrigger.OnFight, true);
                }

                BotFightInterface.SetTarget(BotTarget);

                BotFightInterface.NodeUpdate();

                return new Vector3?(BotTarget);
            }
            return null;
        }

        protected Vector3 BotTarget;

        private GInterface5 BotFightInterface;

        private float TalkDelay;

        private readonly GClass182 BotUpdate2;
    }
}
