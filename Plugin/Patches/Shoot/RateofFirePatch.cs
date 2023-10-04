using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SAIN.Helpers.Shoot;

namespace SAIN.Patches.Shoot
{
    public class AimTimePatch : ModulePatch
    {
        private static Type _aimingDataType;
        private static MethodInfo _aimingDataMethod7;

        protected override MethodBase GetTargetMethod()
        {
            //return AccessTools.Method(typeof(GClass544), "method_7");
            _aimingDataType = PatchConstants.EftTypes.Single(x => x.GetProperty("LastSpreadCount") != null && x.GetProperty("LastAimTime") != null);
            _aimingDataMethod7 = AccessTools.Method(_aimingDataType, "method_7");
            return _aimingDataMethod7;
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner ___botOwner_0, float dist, ref float __result)
        {
            if (!SAINPlugin.LoadedPreset.GlobalSettings.Aiming.FasterCQBReactionsGlobal)
            {
                return;
            }
            if (SAINPlugin.BotController.GetBot(___botOwner_0.ProfileId, out var component))
            {
                var settings = component.Info.FileSettings.Aiming;
                if (settings.FasterCQBReactions)
                {
                    float maxDist = settings.FasterCQBReactionsDistance;
                    if (dist <= maxDist)
                    {
                        float min = settings.FasterCQBReactionsMinimum;
                        float scale = dist / maxDist;
                        scale = Mathf.Clamp(scale, min, 1f);
                        float newResult = __result * scale;
                        __result = newResult;
                    }
                }
            }
        }
    }

    public class FullAutoPatch : ModulePatch
    {
        private static PropertyInfo _ShootData;

        protected override MethodBase GetTargetMethod()
        {
            _ShootData = AccessTools.Property(typeof(BotOwner), "ShootData");
            return AccessTools.Method(_ShootData.PropertyType, "method_6");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ____owner, ref float ___nextFingerUpTime)
        {
            Weapon weapon = ____owner.WeaponManager.CurrentWeapon;

            // Config ToggleOnOff Check, Null aim data check, and Make sure the weapon can actually full auto (probably not necessary)
            if (____owner.AimingData == null)
            {
                return true;
            }

            if (weapon.SelectedFireMode == Weapon.EFireMode.fullauto || weapon.SelectedFireMode == Weapon.EFireMode.burst)
            {
                float distance = ____owner.AimingData.LastDist2Target;
                float scaledDistance = FullAutoBurstLength(____owner, distance);

                ___nextFingerUpTime = scaledDistance + Time.time;

                return false;
            }

            return true;
        }
    }

    public class SemiAutoPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass336), "method_1");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner ___botOwner_0, ref float __result)
        {
            if (SAINPlugin.BotController.GetBot(___botOwner_0.ProfileId, out var component))
            {
                __result = component.Info.WeaponInfo.Firerate.SemiAutoROF();
            }
        }
    }
}