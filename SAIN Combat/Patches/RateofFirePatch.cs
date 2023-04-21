using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Combat.Components;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SAIN.Combat.Configs.DebugConfig;
using static SAIN.Combat.Configs.Firerate;
using static SAIN.Combat.Configs.FullAutoConfig;
using static SAIN.Combat.Helpers.ShootPatchHelpers;

namespace SAIN.Combat.Patches
{
    public class FiremodeSwapPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("ShootData")?.PropertyType?.GetMethod("Shoot");
        }
        [PatchPrefix]
        public static void PatchPrefix(ref BotOwner ___botOwner_0)
        {
            if (!BurstLengthToggle.Value || ___botOwner_0.AimingData == null)
            {
                return;
            }

            // Shortcuts
            float distance = ___botOwner_0.AimingData.LastDist2Target;
            var weapon = ___botOwner_0.WeaponManager.CurrentWeapon;

            var autoMode = Weapon.EFireMode.fullauto;
            var semiMode = Weapon.EFireMode.single;
            var burstMode = Weapon.EFireMode.burst;

            bool isAutoWeapon = weapon.WeapFireType.Contains(autoMode);
            bool isSemiWeapon = weapon.WeapFireType.Contains(semiMode);
            bool isBurstWeapon = weapon.WeapFireType.Contains(burstMode);

            bool isWeaponSetAuto = weapon.SelectedFireMode == autoMode;
            bool isWeaponSetSemi = weapon.SelectedFireMode == semiMode;
            bool isWeaponSetburst = weapon.SelectedFireMode == burstMode;

            // Swap Distances
            float semidist = 80f;
            float autodist = 75f;

            // Skip firemode swap if a bot is using a stationary weapon
            if (___botOwner_0.WeaponManager.Stationary.Taken)
            {
                return;
            }

            // Firemode Swap
            // Semiauto
            if (distance > semidist && isSemiWeapon && !isWeaponSetSemi)
            {
                weapon.FireMode.SetFireMode(semiMode);
                return;
            }

            // Fullauto and Burst
            if (distance <= autodist)
            {
                // Switch to full auto if the weapon has it
                if (isAutoWeapon && !isWeaponSetAuto)
                {
                    weapon.FireMode.SetFireMode(autoMode);
                }
                // Switch to burst fire if the weapon has it
                else if (isBurstWeapon && !isWeaponSetburst)
                {
                    weapon.FireMode.SetFireMode(burstMode);
                }
            }
        }
    }
    public class FullAutoPatch_1 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass546), "method_6");
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
    public class FullAutoPatch_2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass363), "method_6");
        }
        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref float __result)
        {
            Weapon weapon = ___botOwner_0.WeaponManager.CurrentWeapon;

            // Config Toggle Check, Null aim data check, and Make sure the weapon can actually full auto (probably not necessary)
            if (!BurstLengthToggle.Value || ___botOwner_0.AimingData == null || !weapon.WeapFireType.Contains(Weapon.EFireMode.fullauto))
            {
                return true;
            }
            // Takes the sqrt of the modifier to lower its impact
            //float burstlengthModifier = Mathf.Sqrt(weaponinfo.ShootModifier);

            float distance = ___botOwner_0.AimingData.LastDist2Target;
            float scaledDistance = FullAutoLength(___botOwner_0, distance);

            __result = scaledDistance;

            if (DebugBurst.Value) Logger.LogInfo($"Fullauto: [{___botOwner_0.name}] w/ [{weapon.WeapClass}],[{weapon.AmmoCaliber}], Burst Length = [{scaledDistance}] at [{distance}] meters");

            return false;
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
                permeter = weaponinfo.PerMeter / weaponinfo.ShootModifier;
            }

            var firemode = ___botOwner_0.WeaponManager.CurrentWeapon.SelectedFireMode;

            float finalTime = SemiAutoROF(EnemyDistance, permeter, firemode);

            if (DebugFire.Value) Logger.LogInfo($"FIRERATE: {___botOwner_0.name}, Time Between Shots: [{finalTime}], Distance: [{EnemyDistance}] is fullauto? [{firemode == Weapon.EFireMode.fullauto}]");
            /*
            if (bot.WeaponManager.CurrentWeapon.SelectedFireMode == Weapon.EFireMode.fullauto && bot.AimingData.LastDist2Target < 50f)
            {
                float rof = FullAutoROF(bot.WeaponManager.CurrentWeapon.Template.bFirerate);
                finalTime = rof;
            }
            */

            __result = finalTime;

            return false;
        }
    }
}