using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using System.Linq;
using System;
using System.Reflection;
using UnityEngine;
using static SAIN.Helpers.Shoot;
using static SAIN.UserSettings.EditorSettings;

namespace SAIN.Patches
{
    public class AimTimePatch : ModulePatch
    {
        private static Type _aimingDataType;
        private static MethodInfo _aimingDataMethod7;

        protected override MethodBase GetTargetMethod()
        {
            _aimingDataType = PatchConstants.EftTypes.Single(x => x.GetProperty("LastSpreadCount") != null && x.GetProperty("LastAimTime") != null);
            _aimingDataMethod7 = AccessTools.Method(_aimingDataType, "method_7");
            return _aimingDataMethod7;
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner ___botOwner_0, float dist, ref float __result)
        {
            if (!FasterCQBReactions.Value)
            {
                return;
            }
            if (dist <= 30)
            {
                float min = 0.075f;
                if (___botOwner_0.IsRole(WildSpawnType.assault))
                {
                    min = 0.15f;
                }
                float scale = dist / 30f;
                scale = Mathf.Clamp(scale, min, 1f);
                float newResult = __result * scale;
                //Logger.LogWarning($"New Aim Time: [{newResult}] Old Aim Time: [{__result}] Distance: [{dist}]");
                __result = newResult;
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
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref float ___float_0)
        {
            Weapon weapon = ___botOwner_0.WeaponManager.CurrentWeapon;

            // Config Toggle Check, Null aim data check, and Make sure the weapon can actually full auto (probably not necessary)
            if (___botOwner_0.AimingData == null)
            {
                return true;
            }

            if (weapon.SelectedFireMode == Weapon.EFireMode.fullauto || weapon.SelectedFireMode == Weapon.EFireMode.burst)
            {
                float distance = ___botOwner_0.AimingData.LastDist2Target;
                float scaledDistance = FullAutoBurstLength(___botOwner_0, distance);

                ___float_0 = scaledDistance * BurstMulti.Value + Time.time;

                return false;
            }

            return true;
        }
    }

    public class UpdateFireArmPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass365), "UpdateFirearmsController");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner ___botOwner_0)
        {
            if (SAINPlugin.BotController.GetBot(___botOwner_0.ProfileId, out var component))
            {
            }
        }
    }

    public class SemiAutoPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass365), "method_1");
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