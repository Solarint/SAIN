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

        public ShootClass(BotOwner owner) 
            : base(owner)
        {
            SAIN = owner.GetComponent<SAINComponentClass>();
            Shoot = new BotShoot(owner);
        }

        private readonly BotShoot Shoot;
        private BotOwner BotOwner => botOwner_0;

        private readonly SAINComponentClass SAIN;

        public override void Update()
        {
            var enemy = SAIN.Enemy;
            if (enemy != null && enemy.IsVisible && enemy.CanShoot)
            {
                Vector3? pointToShoot = GetPointToShoot();
                if (pointToShoot != null)
                {
                    if (enemy.RealDistance < 30f)
                    {
                        BotOwner.BotLight?.TurnOn(true);
                    }
                    Target = pointToShoot.Value;
                    if (BotOwner.AimingData.IsReady && !SAIN.NoBushESP.NoBushESPActive && FriendlyFire.ClearShot)
                    {
                        ReadyToShoot();
                        Shoot.Update();
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
                    var transform = SAIN?.Enemy?.EnemyPerson?.Transform;
                    if (transform != null && (value - transform.Head).magnitude < 0.1f)
                    {
                        value = transform.Stomach;
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

    public class BotShoot : BaseNodeClass
    {
        public BotShoot(BotOwner bot)
            : base(bot)
        {
        }

        public override void Update()
        {
            if (!this.botOwner_0.WeaponManager.HaveBullets)
            {
                return;
            }
            Vector3 position = this.botOwner_0.GetPlayer.PlayerBones.WeaponRoot.position;
            Vector3 realTargetPoint = this.botOwner_0.AimingData.RealTargetPoint;
            if (this.botOwner_0.ShootData.ChecFriendlyFire(position, realTargetPoint))
            {
                return;
            }
            if (this.botOwner_0.ShootData.Shoot() && this.int_0 > this.botOwner_0.WeaponManager.Reload.BulletCount)
            {
                this.int_0 = this.botOwner_0.WeaponManager.Reload.BulletCount;
            }
        }

        private int int_0;
    }

    public class SAINBaseNodeClass : BaseNodeClass
    {
        public SAINBaseNodeClass(BotOwner bot)
            : base(bot)
        {
        }

        public override void Update()
        {

        }
    }
}
