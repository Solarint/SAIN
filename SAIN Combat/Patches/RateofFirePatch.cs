using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Combat.Components;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SAIN.Combat.Configs.DebugConfig;
using static SAIN.Combat.Configs.SemiAutoConfig;
using static SAIN.Combat.Configs.FullAutoConfig;
using static SAIN.Combat.Helpers.Shoot;

namespace SAIN.Combat.Patches
{
    public class FullAutoPatch : ModulePatch
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
                permeter = weaponinfo.PerMeter / weaponinfo.FinalModifier;
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