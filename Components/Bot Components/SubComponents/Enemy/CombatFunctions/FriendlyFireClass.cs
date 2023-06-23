using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.Classes
{
    public class FriendlyFireClass : SAINBot
    {
        public FriendlyFireClass(BotOwner owner) : base(owner) { }

        public FriendlyFireStatus FriendlyFireStatus { get; private set; }
        public bool ClearShot => FriendlyFireStatus != FriendlyFireStatus.FriendlyBlock;

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

        public FriendlyFireStatus CheckFriendlyFire(Vector3? target = null)
        {
            var friendlyFire = FriendlyFireStatus.None;
            if (!SAIN.Squad.BotInGroup || !BotOwner.ShootData.Shooting)
            {
                return friendlyFire;
            }
            if (target == null)
            {
                if (BotOwner.AimingData == null)
                {
                    return friendlyFire;
                }
                target = BotOwner.AimingData.EndTargetPoint;
            }
            if (BotOwner.ShootData?.ChecFriendlyFire(BotOwner.WeaponRoot.position, target.Value) == true)
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
            if (BotOwner.ShootData.Shooting == true)
            {
                BotOwner.ShootData?.EndShoot();
            }
            BotOwner.AimingData?.LoseTarget();
        }

        private float CheckFriendlyFireTimer = 0f;
    }
}
