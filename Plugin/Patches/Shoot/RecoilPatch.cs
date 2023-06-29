using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Components;
using SAIN.Helpers;
using System.Reflection;
using UnityEngine;
using System;
using Aki.Reflection.Utils;
using System.Linq;
using static SAIN.UserSettings.ShootConfig;
using static SAIN.UserSettings.DifficultyConfig;

namespace SAIN.Patches
{
    public class AimOffsetPatch : ModulePatch
    {
        private static Type _aimingDataType;

        protected override MethodBase GetTargetMethod()
        {
            _aimingDataType = PatchConstants.EftTypes.Single(x => x.GetProperty("LastSpreadCount") != null && x.GetProperty("LastAimTime") != null);
            return AccessTools.Method(_aimingDataType, "method_13");
        }

        [PatchPrefix]
        public static bool PatchPrefix(GClass544 __instance, ref BotOwner ___botOwner_0, ref Vector3 ___vector3_5, ref Vector3 ___vector3_4, ref float ___float_13)
        {
            // Recoil Scatter Modifiers.
            float modifier = ScatterMultiplier.Value;
            if (SAINPlugin.BotController.GetBot(___botOwner_0.ProfileId, out var component))
            {
                if (component.Info.IsPMC)
                {
                    modifier /= PMCDifficulty.Value;
                }
                else if (component.Info.IsScav)
                {
                    modifier /= ScavDifficulty.Value;
                }
                else
                {
                    modifier /= OtherDifficulty.Value;
                }
            }
            modifier /= GlobalDifficulty.Value;

            // Applies aiming offset, recoil offset, and scatter offsets
            Vector3 finalTarget = __instance.RealTargetPoint
                + ___vector3_5
                + ___float_13

                * (___vector3_4
                + (___botOwner_0.RecoilData.RecoilOffset
                * modifier));

            if (HeadShotProtection.Value)
            {
                // Get the head position of a bot's current enemy if it exists
                Vector3? headPos = ___botOwner_0.Memory.GoalEnemy?.Person?.MainParts[BodyPartType.head].Position;
                if (headPos != null)
                {
                    // Check the distance to the bot's aiming target, and see if its really close or on the player's head
                    float dist = (headPos.Value - finalTarget).sqrMagnitude;
                    if (dist < 0.1f * 0.1f)
                    {
                        // Shift the aim target up if it was going to be a headshot
                        finalTarget += Vector3.up * 0.2f;
                    }
                }
            }
            __instance.EndTargetPoint = finalTarget;
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
            float modifier = 1f;
            var stats = ___botOwner_0.GetComponent<SAINComponent>()?.Info?.WeaponInfo;
            if (stats != null)
            {
                modifier = stats.FinalModifier;
            }

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
        public static bool PatchPrefix(ref Vector3 ___vector3_0, ref BotOwner ___botOwner_0, ref float ___float_1)
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