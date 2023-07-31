using EFT;
using UnityEngine;
using Random = UnityEngine.Random;
using static EFT.InventoryLogic.Weapon;
using SAIN.Components;

namespace SAIN.Classes
{
    public class Firemode : SAINWeaponInfoAbstract
    {
        public Firemode(SAINComponent owner) : base(owner) { }

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
                    if (HasSemi())
                    {
                        mode = EFireMode.single;
                    }
                }
                else if (distance <= FullAutoSwapDist)
                {
                    if (HasFullAuto())
                    {
                        mode = EFireMode.fullauto;
                    }
                    else if (HasBurst())
                    {
                        mode = EFireMode.burst;
                    }
                    else if (HasDoubleAction())
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
            CurrentWeapon?.FireMode?.SetFireMode(fireMode);
            BotOwner?.GetPlayer?.HandsController?.FirearmsAnimator?.SetFireMode(fireMode);
        }

        public bool CanSetMode(EFireMode fireMode)
        {
            return CurrentWeapon != null && HasFireMode(fireMode) && !IsFireModeSet(fireMode);
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
