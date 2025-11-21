using EFT;
using EFT.InventoryLogic;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components.BotComponentSpace.Classes;

public class BotMagazineWeapon
{
    private static readonly List<MagazineItemClass> _preAllocMagList = new(20);

    public static bool RefillMags(BotComponent bot, BotWeaponInfo weapon, int numberToRefill = -1, bool includeActiveMag = false)
    {
        Slot slot = weapon.weapon.GetMagazineSlot();
        if (slot == null)
        {
#if DEBUG
            Logger.LogError("slot null");
#endif
            return false;
        }
        if (slot.ContainedItem is not MagazineItemClass activeMag)
        {
#if DEBUG
            Logger.LogError($"mag null :: {slot.ContainedItem?.Name} :: {slot.ContainedItem?.ShortName}");
#endif
            return false;
        }

        _preAllocMagList.Clear();
        bot.Player.InventoryController.GetReachableItemsOfTypeNonAlloc<MagazineItemClass>(_preAllocMagList, null);
        if (_preAllocMagList.Count == 0)
        {
            _preAllocMagList.Clear();
#if DEBUG
            Logger.LogDebug($"[{bot.Info.Profile.NickName}] no mags");
#endif
            return false;
        }

        int refilledMags = 0;
        int fullMags = 0;
        if (includeActiveMag)
        {
            CheckMag(weapon, ref refilledMags, ref fullMags, activeMag);
        }
        foreach (var mag in _preAllocMagList)
        {
            if (slot.CanAccept(mag))
            {
                CheckMag(weapon, ref refilledMags, ref fullMags, mag);
                if (numberToRefill < 0) continue;
                if (refilledMags >= numberToRefill) break;
            }
        }
        _preAllocMagList.Clear();

        if (refilledMags > 0 || fullMags >= numberToRefill)
        {
#if DEBUG
            Logger.LogDebug($"[{bot.Info.Profile.NickName}] success mags {refilledMags} : {fullMags}");
#endif
            return true;
        }
#if DEBUG
        Logger.LogDebug($"[{bot.Info.Profile.NickName}] failed mags {refilledMags} : {fullMags}");
#endif
        return false;
    }

    private static void CheckMag(BotWeaponInfo weapon, ref int refilled, ref int full, MagazineItemClass mag)
    {
        if (mag == null) return;
        if (mag.Count == mag.MaxCount)
        {
            full++;
            return;
        }
        weapon.Reload.method_2(weapon.weapon, mag);
        refilled++;
    }

    public static float GetAmmoRatio(MagazineItemClass magazine)
    {
        if (magazine == null) return 0.0f;

        return (float)magazine.Count / (float)magazine.MaxCount;
    }
}