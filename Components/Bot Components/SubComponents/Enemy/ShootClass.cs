using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Classes.CombatFunctions
{
    public class ShootClass : BaseNodeClass
    {
        public ShootClass(BotOwner owner, SAINComponent sain) : base(owner)
        {
            SAIN = sain;
            BotShoot = new GClass183(owner);
        }

        private readonly GClass183 BotShoot;
        private BotOwner BotOwner => botOwner_0;

        private readonly SAINComponent SAIN;

        public override void Update()
        {
            var enemy = SAIN.Enemy;
            if (enemy != null && enemy.CanShoot && enemy.IsVisible)
            {
                if (AimingData == null)
                {
                    AimingData = BotOwner.AimingData;
                }

                Vector3? pointToShoot = GetPointToShoot();
                if (pointToShoot != null)
                {
                    if (enemy.RealDistance < 30f)
                    {
                        BotOwner.BotLight?.TurnOn(true);
                    }
                    Target = pointToShoot.Value;
                    if (AimingData.IsReady && !SAIN.NoBushESPActive && FriendlyFire.ClearShot)
                    {
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
                AimingData.SetTarget(Target);
                AimingData.NodeUpdate();
                return new Vector3?(Target);
            }
            return null;
        }

        protected Vector3 Target;
        private GInterface5 AimingData;

        public FriendlyFireClass FriendlyFire => SAIN.FriendlyFireClass;
    }
}
