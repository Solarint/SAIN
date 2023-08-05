using EFT;
using UnityEngine;
using Random = UnityEngine.Random;
using static EFT.InventoryLogic.Weapon;
using SAIN.Components;
using SAIN.SAINComponent;

namespace SAIN.SAINComponent.Classes.WeaponFunction
{
    public class Firemode : SAINBase, ISAINClass
    {
        public Firemode(SAINComponentClass sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        private const float SemiAutoSwapDist = 40f;
        private const float FullAutoSwapDist = 30f;

        public void CheckSwap()
        {
            if (BotOwner?.WeaponManager?.Stationary?.Taken == false)
            {
                float distance = SAIN.DistanceToAimTarget;
                EFireMode mode = EFireMode.doublet;

                if (distance > SemiAutoSwapDist)
                {
                    if (SAIN.Info.WeaponInfo.HasSemi())
                    {
                        mode = EFireMode.single;
                    }
                }
                else if (distance <= FullAutoSwapDist)
                {
                    if (SAIN.Info.WeaponInfo.HasFullAuto())
                    {
                        mode = EFireMode.fullauto;
                    }
                    else if (SAIN.Info.WeaponInfo.HasBurst())
                    {
                        mode = EFireMode.burst;
                    }
                    else if (SAIN.Info.WeaponInfo.HasDoubleAction())
                    {
                        mode = EFireMode.doubleaction;
                    }
                }

                if (mode != EFireMode.doublet && CanSetMode(mode))
                {
                    SetFireMode(mode);
                }
                else
                {
                    CheckWeapon();
                }
            }
        }

        public void SetFireMode(EFireMode fireMode)
        {
            SAIN.Info.WeaponInfo.CurrentWeapon?.FireMode?.SetFireMode(fireMode);
            Player?.HandsController?.FirearmsAnimator?.SetFireMode(fireMode);
        }

        public bool CanSetMode(EFireMode fireMode)
        {
            return SAIN.Info.WeaponInfo.CurrentWeapon != null && SAIN.Info.WeaponInfo.HasFireMode(fireMode) && !SAIN.Info.WeaponInfo.IsFireModeSet(fireMode);
        }

        private void CheckWeapon()
        {
            if (SAIN.Enemy == null)
            {
                if (CheckMagTimer < Time.time && NextCheckTimer < Time.time)
                {
                    NextCheckTimer = Time.time + 30f;
                    CheckMagTimer = Time.time + 360f * Random.Range(0.5f, 1.5f);
                    Player.HandsController.FirearmsAnimator.CheckAmmo();
                }
                else if (CheckChamberTimer < Time.time && NextCheckTimer < Time.time)
                {
                    NextCheckTimer = Time.time + 30f;
                    CheckChamberTimer = Time.time + 360f * Random.Range(0.5f, 1.5f);
                    Player.HandsController.FirearmsAnimator.CheckChamber();
                }
            }
        }

        private float CheckMagTimer;
        private float CheckChamberTimer;
        private float NextCheckTimer;
    }
}
