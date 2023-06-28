using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Components;
using System.Linq;
using System;
using System.Reflection;
using UnityEngine;
using static SAIN.Helpers.Shoot;
using static SAIN.UserSettings.ShootConfig;
using static SAIN.UserSettings.DifficultyConfig;
using SAIN.Classes;

namespace SAIN.Patches
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
            if (___botOwner_0.AimingData == null || (!weapon.WeapFireType.Contains(Weapon.EFireMode.fullauto) && !weapon.WeapFireType.Contains(Weapon.EFireMode.burst)))
            {
                return true;
            }

            if (weapon.SelectedFireMode != Weapon.EFireMode.single)
            {
                float distance = ___botOwner_0.AimingData.LastDist2Target;
                float scaledDistance = FullAutoLength(___botOwner_0, distance);

                ___float_0 = scaledDistance * BurstLengthModifier.Value + Time.time;

                return false;
            }

            return true;
        }
    }

    public class PrefShootDistPatch : ModulePatch
    {
        private static PropertyInfo _LookSensor;

        protected override MethodBase GetTargetMethod()
        {
            _LookSensor = AccessTools.Property(typeof(BotOwner), "LookSensor");
            return AccessTools.Method(_LookSensor.PropertyType, "Init");
        }

        [PatchPrefix]
        public static void PatchPrefix(ref BotOwner ___botOwner_0, ref float ___float_2)
        {
            Weapon weapon = ___botOwner_0.WeaponManager.CurrentWeapon;
            string WeaponClass = weapon.Template.weapClass;
            float PreferedDist;
            switch (WeaponClass)
            {
                case "assaultCarbine":
                case "assaultRifle":
                case "machinegun":
                    PreferedDist = 100f;
                    break;

                case "smg":
                    PreferedDist = 50f;
                    break;

                case "pistol":
                    PreferedDist = 30f;
                    break;

                case "marksmanRifle":
                    PreferedDist = 150f;
                    break;

                case "sniperRifle":
                    PreferedDist = 200f;
                    break;

                case "shotgun":
                    PreferedDist = 40f;
                    break;
                case "grenadeLauncher":
                case "specialWeapon":
                    PreferedDist = 100f;
                    break;

                default:
                    PreferedDist = 120f;
                    break;
            }
            ___float_2 = PreferedDist;
        }
    }

    public class SemiAutoPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass363), "method_1");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref float __result)
        {
            BotOwner bot = ___botOwner_0;

            // Config Toggle and Disables mod for sniper scavs
            if (bot.IsRole(WildSpawnType.marksman))
            {
                return true;
            }

            var component = bot.gameObject.GetComponent<SAINComponent>();
            if (component == null)
            {
                return true;
            }

            float EnemyDistance = (bot.AimingData.RealTargetPoint - bot.WeaponRoot.position).magnitude;

            var weaponInfo = component.Info.WeaponInfo;

            float permeter = weaponInfo.PerMeter / weaponInfo.FinalModifier;

            var firemode = ___botOwner_0.WeaponManager.CurrentWeapon.SelectedFireMode;

            float finalTime = SemiAutoROF(EnemyDistance, permeter, firemode);

            __result = finalTime;

            return false;
        }
    }
}