using EFT;
using EFT.InventoryLogic;
using SAIN.SAINComponent;
using System;
using System.Collections.Generic;
using UnityEngine;
using static EFT.InventoryLogic.Weapon;

namespace SAIN.Components.BotComponentSpace.Classes;

public class ReloadClass : BotBase
{
    /// <summary>
    /// Returns true if any weapon was successfully refilled
    /// </summary>
    public static bool RefillMagsOnEachWeapon(BotComponent bot, BotWeaponManager weaponManager, int count = -1, bool includeActiveMag = false, params EquipmentSlot[] slotsToIgnore)
    {
        bool result = false;
        foreach (var item in weaponManager.Info)
        {
            if (item.Value?.weapon != null &&
                IsMagFed(item.Value.weapon.ReloadMode))
            {
                bool canFill = true;
                if (slotsToIgnore != null)
                    foreach (var slot in slotsToIgnore)
                        if (slot == item.Key)
                        {
                            canFill = false;
                            break;
                        }

                if (canFill && BotMagazineWeapon.RefillMags(bot, item.Value, count, includeActiveMag))
                {
                    result = true;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Returns true if this weapon was refilled
    /// </summary>
    public static bool RefillMagsInSlot(EquipmentSlot slot, BotComponent bot, BotWeaponManager weaponManager, int count = -1, bool includeActiveMag = false)
    {
        if (weaponManager.Info.TryGetValue(slot, out var weapon))
        {
            return BotMagazineWeapon.RefillMags(bot, weapon, count, includeActiveMag);
        }
        return false;
    }

    public ReloadClass(BotComponent bot) : base(bot)
    {
        _weaponManager = Bot.BotOwner.WeaponManager;
    }

    //public override void ManualUpdate()
    //{
    //    checkRefill();
    //}

    //private void checkRefill()
    //{
    //    if (_nextCheckRefillTime > Time.time)
    //    {
    //        return;
    //    }
    //    if (BotOwner.WeaponManager.Reload.Reloading)
    //    {
    //        _nextCheckRefillTime = Time.time + 3;
    //        return;
    //    }
    //    if (Bot.IsInCombat)
    //    {
    //        _nextCheckRefillTime = Time.time + 3;
    //        return;
    //    }
    //    RefillMagsOnEachWeapon(Bot, BotOwner.WeaponManager, 4);
    //    _nextCheckRefillTime = Time.time + 30;
    //}

    //private float _nextCheckRefillTime;

    private static bool IsMagFed(EReloadMode reloadMode)
    {
        return reloadMode switch {
            EReloadMode.ExternalMagazine or EReloadMode.ExternalMagazineWithInternalReloadSupport => true,
            _ => false,
        };
    }

    private readonly BotWeaponManager _weaponManager;
}