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
            SAINEnemyClass enemy = SAIN.Enemy;
            if (enemy == null || !enemy.Seen || enemy.TimeSinceSeen > 10f || !BotOwner.WeaponManager.IsReady || !BotOwner.WeaponManager.HaveBullets || SAIN.Mover.IsSprinting || SAIN.Cover.CoverInUse == null)
            {
                ResetBlindFire();
                return;
            }

            if (CurrentBlindFireSetting == 0)
            {
                WeaponPosOffset = BotOwner.WeaponRoot.position - SAIN.Transform.Position;
            }

            Vector3 targetPos;
            if (enemy.IsVisible)
            {
                targetPos = enemy.EnemyChestPosition;
            }
            else if (enemy.LastSeenPosition != null)
            {
                targetPos = enemy.LastSeenPosition.Value + Vector3.up * 1f;
            }
            else
            {
                ResetBlindFire();
                BlindFireTimer = Time.time + 0.5f;
                return;
            }

            int blindfire = CheckOverHeadBlindFire(targetPos);

            if (blindfire == 0)
            {
                blindfire = CheckSideBlindFire(targetPos);
            }

            if (blindfire == 0)
            {
                ResetBlindFire();
                BlindFireTimer = Time.time + 0.5f;
            }
            else if (!SAIN.NoBushESP.NoBushESPActive && SAIN.FriendlyFireClass.ClearShot)
            {
                if (BlindFireTimer < Time.time)
                {
                    BlindFireTimer = Time.time + 1f;
                    SetBlindFire(blindfire);
                }

                Vector3 start = SAIN.Position;
                Vector3 blindFireDirection = Vector.Rotate(targetPos - start, Vector.RandomRange(5), Vector.RandomRange(5), 0);
                BlindFireTargetPos = blindFireDirection + start;
                SAIN.Steering.LookToPoint(BlindFireTargetPos);
                SAIN.Shoot();
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

        private Vector3 BlindFireTargetPos;

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
                rayShoot = SAIN.Transform.Head + Vector3.up * 0.15f;
                if (!Vector.Raycast(rayShoot, targetPos, mask))
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