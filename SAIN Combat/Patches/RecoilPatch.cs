using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using Combat.Components;
using Combat.Helpers;
using System.Reflection;
using UnityEngine;
using static Combat.UserSettings.DebugConfig;
using static Combat.UserSettings.RecoilScatterConfig;

namespace Combat.Patches
{
    public class AimOffsetPatch : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {
            // Note: Can't find this based on the property type of AimingData? Is this because of it being an interface?
            return AccessTools.Method(typeof(GClass544), "method_13");
        }

        [PatchPrefix]
        public static bool PatchPrefix(GClass544 __instance, ref BotOwner ___botOwner_0, ref Vector3 ___vector3_5, ref Vector3 ___vector3_4, ref float ___float_13)
        {
            __instance.EndTargetPoint = __instance.RealTargetPoint
                + ___vector3_5
                + ___float_13

                * (___vector3_4
                + (___botOwner_0.RecoilData.RecoilOffset
                * ScatterMultiplier.Value));

            return false;
        }
    }
    public class RecoilPatch : ModulePatch
    {
        private static PropertyInfo _RecoilDataPI;

        protected override MethodBase GetTargetMethod()
        {
            _RecoilDataPI = AccessTools.Property(typeof(BotOwner), "RecoilData");
            return AccessTools.Method(_RecoilDataPI.PropertyType, "Recoil");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref Vector3 ___vector3_0, ref BotOwner ___botOwner_0, ref float ___float_0)
        {
            WeaponInfo stats = ___botOwner_0.gameObject.GetComponent<WeaponInfo>();
            float modifier = stats.FinalModifier;

            Vector3 recoilVector;
            if (___float_0 < Time.time)
            {
                Weapon weapon = ___botOwner_0.WeaponManager.CurrentWeapon;

                if (weapon.SelectedFireMode == Weapon.EFireMode.fullauto || weapon.SelectedFireMode == Weapon.EFireMode.burst)
                {
                    ___float_0 = Time.time + Shoot.FullAutoTimePerShot(weapon.Template.bFirerate) * 0.8f;
                }
                else
                {
                    ___float_0 = Time.time + (1f / (weapon.Template.SingleFireRate / 60f)) * 0.8f;
                }

                recoilVector = Shoot.Recoil(___vector3_0, weapon.Template.RecoilForceBack, weapon.Template.RecoilForceUp, modifier, ___botOwner_0.AimingData.LastDist2Target);
                ___vector3_0 = recoilVector;

                if (DebugRecoil.Value)
                {
                    DebugDrawer.DrawAimLine(___botOwner_0, recoilVector, Color.red, 0.025f, Color.red, 0.05f, 3f);
                }
            }
            return false;
        }
    }
    public class LoseRecoilPatch : ModulePatch
    {
        private static PropertyInfo _RecoilDataPI;

        protected override MethodBase GetTargetMethod()
        {
            _RecoilDataPI = AccessTools.Property(typeof(BotOwner), "RecoilData");
            return AccessTools.Method(_RecoilDataPI.PropertyType, "LosingRecoil");
        }

        [PatchPrefix]
        public static bool PatchPrefix(GClass545 __instance, ref Vector3 ___vector3_0, ref BotOwner ___botOwner_0, ref Vector3 ___vector3_1, ref float ___float_0, ref float ___float_1, ref float ___float_2)
        {
            // Repurposing float_1 as a recoil reset timer
            if (___float_1 < Time.time)
            {
                Weapon weapon = ___botOwner_0.WeaponManager.CurrentWeapon;
                if (weapon.SelectedFireMode == Weapon.EFireMode.fullauto || weapon.SelectedFireMode == Weapon.EFireMode.burst)
                {
                    ___float_1 = Time.time + (Shoot.FullAutoTimePerShot(weapon.Template.bFirerate) / 3f);
                }
                else
                {
                    ___float_1 = Time.time + ((1f / (weapon.Template.SingleFireRate / 60f)) / 3f);
                }
                ___vector3_0 = Vector3.Lerp(Vector3.zero, ___vector3_0, LerpRecoil.Value);

                if (DebugRecoil.Value && ___float_2 < Time.time)
                {
                    // Reusing float_2 as a debug timer to limit the line draws
                    ___float_2 = Time.time + 0.1f;
                    DebugDrawer.DrawAimLine(___botOwner_0, ___vector3_0, Color.gray, 0.015f, Color.gray, 0.025f, 3f);
                }
            }
            return false;
        }
    }
    public class EndRecoilPatch : ModulePatch
    {
        private static PropertyInfo _RecoilDataPI;

        protected override MethodBase GetTargetMethod()
        {
            _RecoilDataPI = AccessTools.Property(typeof(BotOwner), "RecoilData");
            return AccessTools.Method(_RecoilDataPI.PropertyType, "CheckEndRecoil");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false;
        }
    }
}