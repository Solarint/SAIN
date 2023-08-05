using EFT;
using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class ShootClass : BaseNodeClass
    {
        public ShootClass(BotOwner owner, SAINComponentClass sain) : base(owner)
        {
            SAIN = sain;
            BotShoot = new BotShoot(owner);
        }

        private readonly BotShoot BotShoot;
        private BotOwner BotOwner => botOwner_0;

        private readonly SAINComponentClass SAIN;

        public override void Update()
        {
            var enemy = SAIN.Enemy;
            if (enemy != null && enemy.CanShoot && enemy.IsVisible)
            {
                if (BotAimingData.AimingData == null)
                {
                    BotAimingData.AimingData = BotOwner.AimingData;
                }

                Vector3? pointToShoot = GetPointToShoot();
                if (pointToShoot != null)
                {
                    if (enemy.RealDistance < 30f)
                    {
                        BotOwner.BotLight?.TurnOn(true);
                    }
                    Target = pointToShoot.Value;
                    if (BotAimingData.AimingData.IsReady && !SAIN.NoBushESPActive && FriendlyFire.ClearShot)
                    {
                        ReadyToShoot();
                        BotShoot.Shoot.Update();
                    }
                }
            }
            else
            {
                BotOwner.BotLight?.TurnOff(false, false);
            }
        }

        protected virtual void ReadyToShoot()
        {
        }

        protected virtual Vector3? GetTarget()
        {
            var enemy = BotOwner.Memory.GoalEnemy;
            if (enemy != null)
            {
                Vector3 value;
                if (enemy.Distance < 3f)
                {
                    value = enemy.GetCenterPart();
                }
                else
                {
                    value = enemy.GetPartToShoot();
                    if (enemy.Person != null && (value - enemy.Person.MainParts[BodyPartType.head].Position).magnitude < 0.1f)
                    {
                        BodyPartType aimPart = EFTMath.RandomBool() ? BodyPartType.leftLeg : BodyPartType.rightLeg;
                        value = enemy.Person.MainParts[aimPart].Position;
                    }
                }
                return new Vector3?(value);
            }
            Vector3? result = null;
            if (BotOwner.Memory.LastEnemy != null)
            {
                result = new Vector3?(BotOwner.Memory.LastEnemy.CurrPosition + Vector3.up * BotOwner.Settings.FileSettings.Aiming.DANGER_UP_POINT);
            }
            return result;
        }

        protected virtual Vector3? GetPointToShoot()
        {
            Vector3? target = GetTarget();
            if (target != null)
            {
                Target = target.Value;
                BotAimingData.AimingData.SetTarget(Target);
                BotAimingData.AimingData.NodeUpdate();
                return new Vector3?(Target);
            }
            return null;
        }

        protected Vector3 Target;

        private readonly BotAimingData BotAimingData = new BotAimingData();
        public FriendlyFireClass FriendlyFire => SAIN.FriendlyFireClass;
    }
}
