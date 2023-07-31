using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;

namespace SAIN.Classes.Mover
{
    public class BlindFireClass : SAINBot
    {
        public BlindFireClass(SAINComponent bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        public void ResetBlindFire()
        {
            if (CurrentBlindFireSetting != 0)
            {
                BotOwner.GetPlayer.MovementContext.SetBlindFire(0);
            }
        }

        private Vector3 WeaponPosOffset;

        public void Update()
        {
            SAINEnemy enemy = SAIN.Enemy;
            if (enemy == null || !enemy.Seen || enemy.TimeSinceSeen > 10f || !BotOwner.WeaponManager.IsReady || !BotOwner.WeaponManager.HaveBullets || SAIN.Mover.IsSprinting)
            {
                ResetBlindFire();
                return;
            }
            if (BlindFireActive)
            {
                DebugGizmos.SingleObjects.Line(SAIN.WeaponRoot, BlindFireTargetPos, Color.magenta, 0.025f, true, 0.1f, true);
            }
            if (BlindFireTimer > Time.time)
            {
                return;
            }

            if (CurrentBlindFireSetting == 0)
            {
                WeaponPosOffset = BotOwner.WeaponRoot.position - BotPosition;
            }

            if (enemy.IsVisible)
            {
                BlindFireTargetPos = enemy.EnemyChestPosition;
            }
            else
            {
                BlindFireTargetPos = enemy.LastSeenPosition + Vector3.up * 1f;
            }

            int blindfire = CheckOverHeadBlindFire(BlindFireTargetPos);

            if (blindfire == 0)
            {
                blindfire = CheckSideBlindFire(BlindFireTargetPos);
            }

            if (blindfire == 0)
            {
                ResetBlindFire();
                BlindFireTimer = Time.time + 0.33f;
            }
            else if (!SAIN.NoBushESPActive && SAIN.FriendlyFireClass.ClearShot)
            {
                SetBlindFire(blindfire);
                SAIN.Steering.LookToPoint(BlindFireTargetPos + Random.insideUnitSphere * 1f);
                SAIN.Shoot();

                BlindFireTimer = Time.time + 1f;
            }
        }

        public Vector3 BlindFireTargetPos { get; private set; }
        public bool BlindFireActive => CurrentBlindFireSetting != 0;
        public int CurrentBlindFireSetting => GetPlayer.MovementContext.BlindFire;
        private float BlindFireTimer = 0f;

        private int CheckOverHeadBlindFire(Vector3 targetPos)
        {
            int blindfire = 0;
            LayerMask mask = LayerMaskClass.HighPolyWithTerrainMask;

            Vector3 rayShoot = WeaponPosOffset + BotPosition;
            Vector3 direction = targetPos - rayShoot;
            if (Physics.Raycast(rayShoot, direction, direction.magnitude, mask))
            {
                rayShoot = SAIN.HeadPosition + Vector3.up * 0.1f;
                direction = targetPos - rayShoot;
                if (!Physics.Raycast(rayShoot, direction, direction.magnitude, mask))
                {
                    blindfire = 1;
                }
            }
            return blindfire;
        }

        private int CheckSideBlindFire(Vector3 targetPos)
        {
            int blindfire = 0;
            LayerMask mask = LayerMaskClass.HighPolyWithTerrainMask;

            Vector3 rayShoot = WeaponPosOffset + BotPosition;
            Vector3 direction = targetPos - rayShoot;
            if (Physics.Raycast(rayShoot, direction, direction.magnitude, mask))
            {
                Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);
                Vector3 SideShoot = rotation * direction.normalized * 0.2f;
                rayShoot += SideShoot;
                direction = targetPos - rayShoot;
                if (!Physics.Raycast(rayShoot, direction, direction.magnitude, mask))
                {
                    blindfire = -1;
                }
            }
            return blindfire;
        }

        public void SetBlindFire(int value)
        {
            if (CurrentBlindFireSetting != value)
            {
                BotOwner.GetPlayer.MovementContext.SetBlindFire(value);
            }
        }

        private readonly ManualLogSource Logger;
    }
}