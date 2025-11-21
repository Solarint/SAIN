using EFT;
using EFT.InventoryLogic;
using SAIN.Components;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Info;
using UnityEngine;
using static EFT.InventoryLogic.Weapon;
using Random = UnityEngine.Random;

namespace SAIN.SAINComponent.Classes.WeaponFunction;

public class Firemode
{
    public void CheckSwapFireMode(BotComponent bot, BotWeaponInfoClass weaponInfo)
    {
        if (_nextSwapTime < Time.time)
        {
            _nextSwapTime = Time.time + _swapFreq;
            Player player = bot.Player;
            if (player.HandsController is Player.FirearmController firearmController && (firearmController.IsInReloadOperation() || firearmController.IsInInteraction()))
            {
                return;
            }
            var manager = bot.BotOwner.WeaponManager;
            if (manager.Selector?.IsWeaponReady == true &&
                manager.Reload?.Reloading == false &&
                !bot.BotOwner.ShootData.Shooting && 
                manager.Stationary?.Taken == false)
            {
                if (GetModeToSwap(bot.IsCheater, weaponInfo, out EFireMode mode) && CanSetMode(mode, weaponInfo))
                {
                    SetFireMode(mode, weaponInfo.CurrentWeapon, player);
                    return;
                }

                TryCheckWeapon(player, bot.GoalEnemy);
            }
        }
    }

    private static bool GetModeToSwap(bool isCheater, BotWeaponInfoClass weaponInfo, out EFireMode mode)
    {
        if (weaponInfo.EWeaponClass == EWeaponClass.machinegun &&
            weaponInfo.HasFireMode(EFireMode.fullauto))
        {
            mode = EFireMode.fullauto;
            return true;
        }

        if (isCheater)
        {
            if (weaponInfo.HasFireMode(EFireMode.fullauto))
            {
                mode = EFireMode.fullauto;
                return true;
            }
            if (weaponInfo.HasFireMode(EFireMode.burst))
            {
                mode = EFireMode.burst;
                return true;
            }
            mode = EFireMode.doublet;
            return false;
        }


        float distanceToTarget = weaponInfo.Bot.DistanceToAimTarget;
        mode = EFireMode.doublet;
        if (distanceToTarget > weaponInfo.SwapToSemiDist || GlobalSettingsClass.Instance.Shoot.ONLY_SEMIAUTO_TOGGLE)
        {
            if (weaponInfo.HasFireMode(EFireMode.single))
            {
                mode = EFireMode.single;
            }
        }
        else if (distanceToTarget <= weaponInfo.SwapToAutoDist)
        {
            if (weaponInfo.HasFireMode(EFireMode.fullauto))
            {
                mode = EFireMode.fullauto;
            }
            else if (weaponInfo.HasFireMode(EFireMode.burst))
            {
                mode = EFireMode.burst;
            }
            else if (weaponInfo.HasFireMode(EFireMode.doubleaction))
            {
                mode = EFireMode.doubleaction;
            }
        }
        return mode != EFireMode.doublet;
    }

    private static void SetFireMode(EFireMode fireMode, Weapon weapon, Player player)
    {
        if (weapon?.FireMode == null || weapon.FireMode.FireMode == fireMode)
        {
            return;
        }
        weapon.FireMode.SetFireMode(fireMode);
        player.HandsController?.FirearmsAnimator?.SetFireMode(fireMode);
    }

    public static bool CanSetMode(EFireMode fireMode, BotWeaponInfoClass weaponInfo)
    {
        return weaponInfo?.CurrentWeapon != null && weaponInfo.HasFireMode(fireMode) && !weaponInfo.IsFireModeSet(fireMode);
    }

    private void TryCheckWeapon(Player player, Enemy currentEnemy)
    {
        if (currentEnemy == null)
        {
            if (CheckMagTimer < Time.time && NextCheckTimer < Time.time)
            {
                NextCheckTimer = Time.time + 30f;
                CheckMagTimer = Time.time + 360f * Random.Range(0.5f, 1.5f);
                player.HandsController.FirearmsAnimator.CheckAmmo();
            }
            else if (CheckChamberTimer < Time.time && NextCheckTimer < Time.time)
            {
                NextCheckTimer = Time.time + 30f;
                CheckChamberTimer = Time.time + 360f * Random.Range(0.5f, 1.5f);
                player.HandsController.FirearmsAnimator.CheckChamber();
            }
        }
    }

    private float CheckMagTimer;
    private float CheckChamberTimer;
    private float NextCheckTimer;
    private float _nextSwapTime;
    private float _swapFreq = 0.5f;
}