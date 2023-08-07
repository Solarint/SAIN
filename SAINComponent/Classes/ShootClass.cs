using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class ShootClass : BaseNodeClass
    {
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(ShootClass));

        public ShootClass(BotOwner owner) : base(owner)
        {
            SAIN = owner.GetComponent<SAINComponentClass>();
            BotShoot = new BotShoot(owner);
        }

        private readonly BotShoot BotShoot;
        private BotOwner BotOwner => botOwner_0;

        private readonly SAINComponentClass SAIN;

        public override void Update()
        {
            var goalEnemy = BotOwner.Memory.GoalEnemy;
            var enemy = SAIN.Enemy;

            bool canShootSAIN = enemy != null && enemy.CanShoot;
            bool canShootEFT = goalEnemy != null && goalEnemy.CanShoot;

            if (canShootSAIN != canShootEFT)
            {
                Logger.LogWarning($"canShootSAIN {canShootSAIN} canShootEFT {canShootEFT}");
            }

            bool canShoot = canShootEFT || canShootSAIN;

            if (enemy != null && enemy.IsVisible && canShoot)
            {
                Vector3? pointToShoot = GetPointToShoot();
                if (pointToShoot != null)
                {
                    if (enemy.RealDistance < 30f)
                    {
                        BotOwner.BotLight?.TurnOn(true);
                    }
                    Target = pointToShoot.Value;
                    if (BotOwner.AimingData.IsReady)
                    {
                        //  && !SAIN.NoBushESP.NoBushESPActive && FriendlyFire.ClearShot
                        ReadyToShoot();
                        BotShoot.Update();
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
                        //BodyPartType aimPart = EFTMath.RandomBool() ? BodyPartType.leftLeg : BodyPartType.rightLeg;
                        //value = enemy.Person.MainParts[aimPart].Position;
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
                BotOwner.AimingData.SetTarget(Target);
                BotOwner.AimingData.NodeUpdate();
                return new Vector3?(Target);
            }
            return null;
        }

        protected Vector3 Target;

        public SAINFriendlyFireClass FriendlyFire => SAIN.FriendlyFireClass;
    }
}
