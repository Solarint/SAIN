using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Components;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SAIN.Helpers.Shoot;
using static SAIN.UserSettings.BotShootConfig;

namespace SAIN.Patches
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