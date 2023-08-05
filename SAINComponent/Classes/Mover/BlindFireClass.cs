using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.SAINComponent;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover
{
    public class BlindFireClass : SAINBase, ISAINClass
    {
        public BlindFireClass(SAINComponentClass sain) : base(sain)
        {
        }

        public void Init()
        {
        }

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
                WeaponPosOffset = BotOwner.WeaponRoot.position - SAIN.Transform.Position;
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

        public void Dispose()
        {
        }


        public void ResetBlindFire()
        {
            if (CurrentBlindFireSetting != 0)
            {
                Player.MovementContext.SetBlindFire(0);
            }
        }

        private Vector3 WeaponPosOffset;

        public Vector3 BlindFireTargetPos { get; private set; }

        public bool BlindFireActive => CurrentBlindFireSetting != 0;

        public int CurrentBlindFireSetting => Player.MovementContext.BlindFire;

        private float BlindFireTimer = 0f;

        private int CheckOverHeadBlindFire(Vector3 targetPos)
        {
            int blindfire = 0;
            LayerMask mask = LayerMaskClass.HighPolyWithTerrainMask;

            Vector3 rayShoot = WeaponPosOffset + SAIN.Transform.Position;
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

            Vector3 rayShoot = WeaponPosOffset + SAIN.Transform.Position;
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
                Player.MovementContext.SetBlindFire(value);
            }
        }
    }
}