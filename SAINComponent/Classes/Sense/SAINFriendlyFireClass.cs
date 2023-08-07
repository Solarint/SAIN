using EFT;
using SAIN.Components;
using SAIN.SAINComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class SAINFriendlyFireClass : SAINBase, ISAINClass
    {
        public SAINFriendlyFireClass(SAINComponentClass sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
            if (CheckFriendlyFireTimer < Time.time)
            {
                CheckFriendlyFireTimer = Time.time + 0.25f;
                FriendlyFireStatus = CheckFriendlyFire();
            }
            if (FriendlyFireStatus == FriendlyFireStatus.FriendlyBlock)
            {
                StopShooting();
            }
        }

        public void Dispose()
        {
        }

        public FriendlyFireStatus FriendlyFireStatus { get; private set; }
        public bool ClearShot => FriendlyFireStatus != FriendlyFireStatus.FriendlyBlock;

        public FriendlyFireStatus CheckFriendlyFire(Vector3? target = null)
        {
            var friendlyFire = FriendlyFireStatus.None;
            if (!SAIN.Squad.BotInGroup || !BotOwner.ShootData.Shooting)
            {
                return friendlyFire;
            }
            if (target == null)
            {
                target = SAIN.Enemy?.EnemyChestPosition;
            }
            if (target == null)
            {
                if (BotOwner.AimingData == null)
                {
                    return friendlyFire;
                }
                target = BotOwner.AimingData.EndTargetPoint;
            }
            if (target != null && BotOwner.ShootData?.ChecFriendlyFire(BotOwner.WeaponRoot.position, target.Value) == true)
            {
                friendlyFire = FriendlyFireStatus.FriendlyBlock;
            }
            else
            {
                friendlyFire = FriendlyFireStatus.Clear;
            }
            return friendlyFire;
        }

        public void StopShooting()
        {
            BotOwner.ShootData?.EndShoot();
            BotOwner.AimingData?.LoseTarget();
        }

        private float CheckFriendlyFireTimer = 0f;
    }
}
