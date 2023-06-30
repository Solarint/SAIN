using EFT;
using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using static EFT.InventoryLogic.Weapon;

namespace SAIN.Classes
{
    public class Firemode : SAINWeapon
    {
        public Firemode(BotOwner owner) : base(owner) { }

        private const float SemiAutoSwapDist = 40f;
        private const float FullAutoSwapDist = 30f;

        public void CheckSwap()
        {
            if (BotOwner.WeaponManager.Stationary?.Taken == false)
            {
                float distance = SAIN.DistanceToAimTarget;
                EFireMode mode = EFireMode.doublet;

                if (distance > SemiAutoSwapDist)
                {
                    if (CurrentWeapon.WeapFireType.Contains(EFireMode.single))
                    {
                        mode = EFireMode.single;
                    }
                }
                else if (distance <= FullAutoSwapDist)
                {
                    if (CurrentWeapon.WeapFireType.Contains(EFireMode.fullauto))
                    {
                        mode = EFireMode.fullauto;
                    }
                    else if (CurrentWeapon.WeapFireType.Contains(EFireMode.burst))
                    {
                        mode = EFireMode.burst;
                    }
                    else if (CurrentWeapon.WeapFireType.Contains(EFireMode.doubleaction))
                    {
                        mode = EFireMode.doubleaction;
                    }
                }

                if (mode != EFireMode.doublet && CurrentWeapon.SelectedFireMode != mode)
                {
                    var animate = BotOwner.GetPlayer?.HandsController?.FirearmsAnimator;
                    if (animate != null)
                    {
                        BotOwner.GetPlayer.HandsController.FirearmsAnimator.SetFireMode(mode);
                    }
                    else
                    {
                        CurrentWeapon.FireMode.SetFireMode(mode);
                    }
                }
                else
                {
                    CheckWeapon();
                }
            }
        }

        private void CheckWeapon()
        {
            if (SAIN.Enemy == null)
            {
                if (CheckMagTimer < Time.time && NextCheckTimer < Time.time)
                {
                    NextCheckTimer = Time.time + 30f;
                    CheckMagTimer = Time.time + 360f * Random.Range(0.5f, 1.5f);
                    BotOwner.GetPlayer.HandsController.FirearmsAnimator.CheckAmmo();
                }
                else if (CheckChamberTimer < Time.time && NextCheckTimer < Time.time)
                {
                    NextCheckTimer = Time.time + 30f;
                    CheckChamberTimer = Time.time + 360f * Random.Range(0.5f, 1.5f);
                    BotOwner.GetPlayer.HandsController.FirearmsAnimator.CheckChamber();
                }
            }
        }

        private float CheckMagTimer;
        private float CheckChamberTimer;
        private float NextCheckTimer;
    }
}
