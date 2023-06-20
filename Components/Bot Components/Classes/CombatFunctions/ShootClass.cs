using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Classes.CombatFunctions
{
    public class ShootClass : GClass104
    {
        public ShootClass(BotOwner owner) : base(owner)
        {
            FriendlyFire = new FriendlyFireClass(owner);
            SAIN = owner.GetComponent<SAINComponent>();
            EFTShoot = new EFTShoot(owner);
        }

        protected SAINComponent SAIN;

        public override void Update()
        {
            if (SAIN.Enemy == null)
            {
                return;
            }

            FriendlyFire.Update();

            if (SAIN.Enemy.IsVisible && FriendlyFire.ClearShot && !SAIN.NoBushESPActive)
            {
                if (ginterface5_0 == null)
                {
                    ginterface5_0 = botOwner_0.AimingData;
                }
                botOwner_0.BotLight.TurnOn(true);
                Vector3? pointToShoot = GetPointToShoot();
                if (pointToShoot != null)
                {
                    Target = pointToShoot.Value;
                    if (ginterface5_0.IsReady)
                    {
                        ReadyToShoot();
                        EFTShoot.Update();
                    }
                }
            }
            else
            {
                botOwner_0.BotLight.TurnOn(false);
            }
        }

        protected virtual void ReadyToShoot()
        {
        }

        protected virtual Vector3? GetTarget()
        {
            GClass475 goalEnemy = botOwner_0.Memory.GoalEnemy;
            if (goalEnemy != null && goalEnemy.CanShoot && goalEnemy.IsVisible)
            {
                Vector3 value;
                if (goalEnemy.Distance < botOwner_0.Settings.FileSettings.Aiming.DIST_TO_SHOOT_TO_CENTER)
                {
                    value = goalEnemy.GetCenterPart();
                }
                else
                {
                    value = goalEnemy.GetPartToShoot();
                }
                return new Vector3?(value);
            }
            Vector3? result = null;
            if (botOwner_0.Memory.LastEnemy != null)
            {
                result = new Vector3?(botOwner_0.Memory.LastEnemy.CurrPosition + Vector3.up * botOwner_0.Settings.FileSettings.Aiming.DANGER_UP_POINT);
            }
            return result;
        }

        protected virtual Vector3? GetPointToShoot()
        {
            Vector3? target = GetTarget();
            if (target != null)
            {
                Target = target.Value;
                ginterface5_0.SetTarget(Target);
                ginterface5_0.NodeUpdate();
                return new Vector3?(Target);
            }
            return null;
        }

        protected Vector3 Target;
        private GInterface5 ginterface5_0;
        private readonly EFTShoot EFTShoot;

        public FriendlyFireClass FriendlyFire { get; private set; }
    }

    public class EFTShoot : GClass104
    {
        // Token: 0x060008AD RID: 2221 RVA: 0x0002A13D File Offset: 0x0002833D
        public EFTShoot(BotOwner bot) : base(bot)
        {
        }

        // Token: 0x060008AE RID: 2222 RVA: 0x0002A150 File Offset: 0x00028350
        public override void Update()
        {
            if (!botOwner_0.WeaponManager.HaveBullets)
            {
                return;
            }
            if (botOwner_0.ShootData.Shoot() && int_0 > botOwner_0.WeaponManager.Reload.BulletCount)
            {
                int_0 = botOwner_0.WeaponManager.Reload.BulletCount;
            }
        }

        private int int_0;
    }
}
