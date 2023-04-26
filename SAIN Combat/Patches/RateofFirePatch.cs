using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN_Audio.Combat.Components;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SAIN_Audio.Combat.Configs.DebugConfig;
using static SAIN_Audio.Combat.Configs.FullAutoConfig;
using static SAIN_Audio.Combat.Configs.SemiAutoConfig;
using static SAIN_Audio.Combat.Helpers.Shoot;

namespace SAIN_Audio.Combat.Patches
{
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
            if (!BurstLengthToggle.Value || ___botOwner_0.AimingData == null || (!weapon.WeapFireType.Contains(Weapon.EFireMode.fullauto) && !weapon.WeapFireType.Contains(Weapon.EFireMode.burst)))
            {
                return true;
            }

            if (weapon.SelectedFireMode == Weapon.EFireMode.single) return true;

            float distance = ___botOwner_0.AimingData.LastDist2Target;
            float scaledDistance = FullAutoLength(___botOwner_0, distance);

            ___float_0 = scaledDistance * BurstLengthModifier.Value + Time.time;

            if (DebugBurst.Value) Logger.LogInfo($"Fullauto1: [{___botOwner_0.name}] w/ [{weapon.WeapClass}],[{weapon.AmmoCaliber}], Burst Length = [{scaledDistance}] at [{distance}] meters");

            return false;
        }
    }
    public class SemiAutoPatch : ModulePatch
    {
        private static PropertyInfo _WeaponManagerPI;
        private static PropertyInfo _WeaponAIPresetPI;

        protected override MethodBase GetTargetMethod()
        {
            _WeaponManagerPI = AccessTools.Property(typeof(BotOwner), "WeaponManager");
            _WeaponAIPresetPI = AccessTools.Property(_WeaponManagerPI.PropertyType, "WeaponAIPreset");
            return AccessTools.Method(_WeaponAIPresetPI.PropertyType, "method_1");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref float __result)
        {
            BotOwner bot = ___botOwner_0;

            // Config Toggle and Disables mod for sniper scavs
            if (!SemiROFToggle.Value || bot.IsRole(WildSpawnType.marksman))
            {
                return true;
            }

            float EnemyDistance = (bot.AimingData.RealTargetPoint - bot.Transform.position).magnitude;

            float permeter = 120f;

            if (!SimpleModeROF.Value)
            {
                // Grabs info we previously calculate in coroutine
                WeaponInfo weaponinfo = bot.gameObject.GetComponent<WeaponInfo>();
                permeter = weaponinfo.PerMeter / weaponinfo.FinalModifier;
            }

            var firemode = ___botOwner_0.WeaponManager.CurrentWeapon.SelectedFireMode;

            float finalTime = SemiAutoROF(EnemyDistance, permeter, firemode);

            if (DebugFire.Value) Logger.LogInfo($"FIRERATE: {___botOwner_0.name}, Time Between Shots: [{finalTime}], Distance: [{EnemyDistance}] is fullauto? [{firemode == Weapon.EFireMode.fullauto}]");

            __result = finalTime;

            return false;
        }
    }
}