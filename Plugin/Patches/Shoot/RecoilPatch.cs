using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;

using System.Reflection;
using UnityEngine;
using System;
using Aki.Reflection.Utils;
using System.Linq;
using SAIN.Helpers;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;

namespace SAIN.Patches.Shoot
{
    public class AimOffsetPatch : ModulePatch
    {
        private static Type _aimingDataType;
        protected override MethodBase GetTargetMethod()
        {
            _aimingDataType = PatchConstants.EftTypes.Single(x => x.GetProperty("LastSpreadCount") != null && x.GetProperty("LastAimTime") != null);
            return AccessTools.Method(_aimingDataType, "method_13");
        }

        private static float DebugTimer;

        [PatchPrefix]
        public static bool PatchPrefix(ref GClass444 __instance, ref BotOwner ___botOwner_0, ref Vector3 ___vector3_5, ref Vector3 ___vector3_4, ref float ___float_13)
        {
            // Applies aiming offset, recoil offset, and scatter offsets
            Vector3 finalTarget = __instance.RealTargetPoint
                + ___vector3_5
                + ___float_13

                * (___vector3_4
                + ___botOwner_0.RecoilData.RecoilOffset);

            if (___botOwner_0?.Memory?.GoalEnemy?.Person?.IsYourPlayer == true)
            {
                float ExtraSpread = SAINNotLooking.GetSpreadIncrease(___botOwner_0);
                if (ExtraSpread > 0)
                {
                    Vector3 vectorSpread = UnityEngine.Random.insideUnitSphere * ExtraSpread;
                    finalTarget += vectorSpread;
                    if (SAINPlugin.DebugMode && DebugTimer < Time.time)
                    {
                        DebugTimer = Time.time + 1f;
                        Logger.LogDebug($"Increasing Spread because Player isn't looking. Magnitude: [{vectorSpread.magnitude}]");
                    }
                }
            }

            if (SAINPlugin.LoadedPreset.GlobalSettings.General.HeadShotProtection)
            {
                IPlayer person = ___botOwner_0.Memory.GoalEnemy?.Person;
                if (person != null && 1 < 0)
                {
                    // Get the head DrawPosition of a bot's current enemy if it exists
                    Vector3 headPos = person.MainParts[BodyPartType.head].Position;
                    // Check the Distance to the bot's aiming target, and see if its really close or on the player's head
                    float dist = (headPos - finalTarget).magnitude;
                    if (dist < 0.15f)
                    {
                        // Shift the aim target up if it was going to be a headshot

                        //Vector3 vertOffset = Vector3.up * 0.1f;
                        Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);
                        Vector3 direction = headPos - ___botOwner_0.WeaponRoot.position;
                        Vector3 right = rotation * direction.normalized * 0.2f;

                        if (EFTMath.RandomBool())
                        {
                            finalTarget += right;
                        }
                        else
                        {
                            finalTarget -= right;
                        }

                        //finalTarget -= vertOffset;

                        //DefaultLogger.LogWarning("Headshot protection activated");
                        DebugGizmos.Line(headPos, finalTarget, Color.red, 0.1f, true, 5f);
                        DebugGizmos.Sphere(finalTarget, 0.1f, Color.red, true, 10f);
                        DebugGizmos.Line(headPos, ___botOwner_0.WeaponRoot.position, Color.red, 0.025f, true, 5f);
                        DebugGizmos.Line(finalTarget, ___botOwner_0.WeaponRoot.position, Color.red, 0.025f, true, 5f);
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
        public static bool PatchPrefix(ref Vector3 ____recoilOffset, ref BotOwner ____owner)
        {
            if (SAINPlugin.BotController.GetBot(____owner.ProfileId, out var component))
            {
                Recoil recoil = component?.Info?.WeaponInfo?.Recoil;
                if (recoil == null)
                {
                    return true;
                }
                // if (___float_0 < Time.time)
                // {
                //     //___float_0 = recoil.RecoilTimeWait;
                //     ____recoilOffset = recoil.CalculateRecoil(____recoilOffset);
                // }
                ____recoilOffset = recoil.CalculateRecoil(____recoilOffset);
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
        public static bool PatchPrefix(ref Vector3 ____recoilOffset, ref BotOwner ____owner, ref float ____remainRecoilTime)
        {
            if (SAINPlugin.BotController.GetBot(____owner.ProfileId, out var component))
            {
                var recoil = component?.Info?.WeaponInfo?.Recoil;
                if (recoil == null)
                {
                    return true;
                }
                // Repurposing float_1 as a recoil Reset timer
                if (____remainRecoilTime < Time.time)
                {
                    ____recoilOffset = recoil.CalculateDecay(____recoilOffset, out float time);
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