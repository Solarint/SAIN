using EFT;
using UnityEngine;
using Random = UnityEngine.Random;
using static EFT.InventoryLogic.Weapon;
using SAIN.Components;
using SAIN.SAINComponent;
using EFT.InventoryLogic;
using SAIN.SAINComponent.Classes.Info;
using SAIN.Preset.GlobalSettings;

namespace SAIN.SAINComponent.Classes.WeaponFunction
{
    public class Firemode : BotBase, IBotClass
    {
        public Firemode(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
            if (_nextSwapTime < Time.time) {
                _nextSwapTime = Time.time + _swapFreq;
                var manager = BotOwner?.WeaponManager;
                if (manager.Selector?.IsWeaponReady == true) {
                    checkSwapFiremode();
                }
            }
        }

        public void Dispose()
        {
        }

        private bool checkSwapMachineGun()
        {
            if (Bot.ManualShoot.Reason != EShootReason.None
                && Bot.Info.WeaponInfo.EWeaponClass == EWeaponClass.machinegun
                && CanSetMode(EFireMode.fullauto)) {
                SetFireMode(EFireMode.fullauto);
                return true;
            }
            return false;
        }

        private void checkSwapFiremode()
        {
            WeaponInfoClass weaponInfo = Bot.Info.WeaponInfo;

            if (weaponInfo == null)
                return;

            if (checkSwapMachineGun())
                return;

            if (BotOwner?.WeaponManager?.Stationary?.Taken == false) {
                if (getModeToSwap(weaponInfo, out EFireMode mode) && CanSetMode(mode)) {
                    SetFireMode(mode);
                    return;
                }

                tryCheckWeapon();
            }
        }

        private bool getModeToSwap(WeaponInfoClass weaponInfo, out EFireMode mode)
        {
            if (Bot.IsCheater) {
                if (weaponInfo.HasFireMode(EFireMode.fullauto)) {
                    mode = EFireMode.fullauto;
                    return true;
                }
                if (weaponInfo.HasFireMode(EFireMode.burst)) {
                    mode = EFireMode.burst;
                    return true;
                }
                mode = EFireMode.doublet;
                return false;
            }

            float distance = Bot.DistanceToAimTarget;
            mode = EFireMode.doublet;
            if (distance > SemiAutoSwapDist || GlobalSettingsClass.Instance.Shoot.ONLY_SEMIAUTO_TOGGLE) {
                if (weaponInfo.HasFireMode(EFireMode.single)) {
                    mode = EFireMode.single;
                }
            }
            else if (distance <= FullAutoSwapDist) {
                if (weaponInfo.HasFireMode(EFireMode.fullauto)) {
                    mode = EFireMode.fullauto;
                }
                else if (weaponInfo.HasFireMode(EFireMode.burst)) {
                    mode = EFireMode.burst;
                }
                else if (weaponInfo.HasFireMode(EFireMode.doubleaction)) {
                    mode = EFireMode.doubleaction;
                }
            }
            return mode != EFireMode.doublet;
        }

        public void SetFireMode(EFireMode fireMode)
        {
            Bot.Info.WeaponInfo.CurrentWeapon?.FireMode?.SetFireMode(fireMode);
            Player?.HandsController?.FirearmsAnimator?.SetFireMode(fireMode);
        }

        public bool CanSetMode(EFireMode fireMode)
        {
            WeaponInfoClass weaponInfo = Bot.Info.WeaponInfo;
            return weaponInfo?.CurrentWeapon != null && weaponInfo.HasFireMode(fireMode) && !weaponInfo.IsFireModeSet(fireMode);
        }

        private void tryCheckWeapon()
        {
            if (Bot.Enemy == null) {
                if (CheckMagTimer < Time.time && NextCheckTimer < Time.time) {
                    NextCheckTimer = Time.time + 30f;
                    CheckMagTimer = Time.time + 360f * Random.Range(0.5f, 1.5f);
                    Player.HandsController.FirearmsAnimator.CheckAmmo();
                }
                else if (CheckChamberTimer < Time.time && NextCheckTimer < Time.time) {
                    NextCheckTimer = Time.time + 30f;
                    CheckChamberTimer = Time.time + 360f * Random.Range(0.5f, 1.5f);
                    Player.HandsController.FirearmsAnimator.CheckChamber();
                }
            }
        }

        private float SemiAutoSwapDist => Bot.Info.WeaponInfo.SwapToSemiDist;
        private float FullAutoSwapDist => Bot.Info.WeaponInfo.SwapToAutoDist;
        private float CheckMagTimer;
        private float CheckChamberTimer;
        private float NextCheckTimer;
        private float _nextSwapTime;
        private float _swapFreq = 0.2f;
    }
}