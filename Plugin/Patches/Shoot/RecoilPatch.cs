using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Classes;
using System.Reflection;
using UnityEngine;
using System;
using Aki.Reflection.Utils;
using System.Linq;
using static SAIN.UserSettings.EditorSettings;
using SAIN.Helpers;

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
        public static bool PatchPrefix(GClass547 __instance, ref BotOwner ___botOwner_0, ref Vector3 ___vector3_5, ref Vector3 ___vector3_4, ref float ___float_13)
        {
            // Applies aiming offset, recoil offset, and scatter offsets
            Vector3 finalTarget = __instance.RealTargetPoint
                + ___vector3_5
                + ___float_13

                * (___vector3_4
                + ___botOwner_0.RecoilData.RecoilOffset);

            if (HeadShotProtection.Value)
            {
                IAIDetails person = ___botOwner_0.Memory.GoalEnemy?.Person;
                if (person != null)
                {
                    // Get the head position of a bot's current enemy if it exists
                    Vector3 headPos = person.MainParts[BodyPartType.head].Position;
                    // Check the Distance to the bot's aiming target, and see if its really close or on the player's head
                    float dist = (headPos - finalTarget).magnitude;
                    if (dist < 0.075f)
                    {
                        // Shift the aim target up if it was going to be a headshot
                        Vector3 vertOffset = Vector3.up * 0.1f;
                        Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);
                        Vector3 direction = headPos - ___botOwner_0.WeaponRoot.position;
                        Vector3 right = rotation * direction.normalized * 0.1f;

                        if (EFTMath.RandomBool())
                        {
                            finalTarget += right;
                        }
                        else
                        {
                            finalTarget -= right;
                        }

                        finalTarget -= vertOffset;

                        //Logger.LogWarning("Headshot protection activated");
                        DebugGizmos.SingleObjects.Line(headPos, finalTarget, Color.red, 0.1f, true, 5f);
                        DebugGizmos.SingleObjects.Sphere(finalTarget, 0.1f, Color.red, true, 10f);
                        DebugGizmos.SingleObjects.Line(headPos, ___botOwner_0.WeaponRoot.position, Color.red, 0.025f, true, 5f);
                        DebugGizmos.SingleObjects.Line(finalTarget, ___botOwner_0.WeaponRoot.position, Color.red, 0.025f, true, 5f);
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
            if (SAINPlugin.BotController.GetBot(___botOwner_0.ProfileId, out var component))
            {
                Recoil recoil = component?.Info?.WeaponInfo?.Recoil;
                if (recoil == null)
                {
                    return true;
                }
                if (___float_0 < Time.time)
                {
                    //___float_0 = recoil.RecoilTimeWait;
                    ___vector3_0 = recoil.CalculateRecoil(___vector3_0);
                }
                return false;
            }
            return true;
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
            if (SAINPlugin.BotController.GetBot(___botOwner_0.ProfileId, out var component))
            {
                var recoil = component?.Info?.WeaponInfo?.Recoil;
                if (recoil == null)
                {
                    return true;
                }
                // Repurposing float_1 as a recoil reset timer
                if (___float_1 < Time.time)
                {
                    ___vector3_0 = recoil.CalculateDecay(___vector3_0, out float time);
                }
                return false;
            }
            return true;
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