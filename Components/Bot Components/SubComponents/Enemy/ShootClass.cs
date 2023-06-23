using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Classes.CombatFunctions
{
    public class ShootClass : GClass104
    {
        public ShootClass(BotOwner owner, SAINComponent sain) : base(owner)
        {
            SAIN = sain;
            FriendlyFire = new FriendlyFireClass(owner);
        }

        private BotOwner BotOwner => botOwner_0;

        private SAINComponent SAIN;

        public override void Update()
        {
            if (BotOwner == null || SAIN == null) return;
            if (SAIN.Enemy == null)
            {
                return;
            }

            FriendlyFire.Update();

            if (SAIN.Enemy.IsVisible && SAIN.Enemy.CanShoot)
            {
                if (AimingData == null)
                {
                    AimingData = BotOwner.AimingData;
                }

                Vector3? pointToShoot = GetPointToShoot();
                if (pointToShoot != null)
                {
                    BotOwner.BotLight.TurnOn();
                    Target = pointToShoot.Value;
                    if (AimingData.IsReady)
                    {
                        ReadyToShoot();
                        Shoot();
                    }
                }
            }
            else
            {
                BotOwner.BotLight.TurnOff();
            }
        }

        protected virtual void ReadyToShoot()
        {
        }

        private void Shoot()
        {
            if (!BotOwner.WeaponManager.HaveBullets)
            {
                return;
            }
            BotOwner.ShootData.Shoot();
        }

        protected virtual Vector3? GetTarget()
        {
            var enemy = BotOwner.Memory.GoalEnemy;
            if (enemy != null && enemy.CanShoot && enemy.IsVisible)
            {
                Vector3 value = enemy.GetPartToShoot();
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

        public FriendlyFireClass FriendlyFire { get; private set; }
    }
}
