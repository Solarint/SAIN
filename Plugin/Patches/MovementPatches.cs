using Aki.Reflection.Patching;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SAIN.Components;
using SAIN.Helpers;
using System.Reflection;
using UnityEngine;
using DrakiaXYZ.BigBrain.Brains;
using UnityEngine.AI;
using SAIN.Layers;
using System;

namespace SAIN.Patches.Movement
{
    public class KickPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("DoorOpener")?.PropertyType?.GetMethod("Interact", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static void PatchPrefix(ref BotOwner ___botOwner_0, Door door, ref EInteractionType Etype)
        {
            if (___botOwner_0.Memory.GoalEnemy == null)
            {
                if (Etype == EInteractionType.Breach)
                {
                    Etype = EInteractionType.Open;
                }

                return;
            }

            if (Etype == EInteractionType.Open || Etype == EInteractionType.Breach)
            {
                bool enemyClose = Vector3.Distance(___botOwner_0.Position, ___botOwner_0.Memory.GoalEnemy.CurrPosition) < 30f;

                if (enemyClose || ___botOwner_0.Memory.IsUnderFire)
                {
                    var breakInParameters = door.GetBreakInParameters(___botOwner_0.Position);

                    if (door.BreachSuccessRoll(breakInParameters.InteractionPosition))
                    {
                        Etype = EInteractionType.Breach;
                    }
                    else 
                    {
                        Etype = EInteractionType.Open;
                    }
                }
                else
                { 
                    Etype = EInteractionType.Open; 
                }
            }
        }
    }

    public class SteerDisablePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var SteeringProp = AccessTools.Property(typeof(BotOwner), "Steering");
            return AccessTools.Method(SteeringProp.PropertyType, "ManualFixedUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            return !BigBrainSAIN.IsBotUsingSAINLayer(___botOwner_0);
        }
    }

    public class SprintDisable : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("Sprint", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            return !BigBrainSAIN.IsBotUsingSAINLayer(___botOwner_0);
        }
    }

    public class BotRunDisable : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("BotRun")?.PropertyType?.GetMethod("Run", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(CoverShootType), typeof(ShootPointClass), typeof(float), typeof(bool), typeof(bool), typeof(bool) }, null);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            return !BigBrainSAIN.IsBotUsingSAINLayer(___botOwner_0);
        }
    }

    public class StopMoveDisable : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetMethod("StopMove", BindingFlags.Instance | BindingFlags.Public );
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner __instance)
        {
            return !BigBrainSAIN.IsBotUsingSAINLayer(__instance);
        }
    }

    public class TargetSpeedDisable1 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("SetTargetMoveSpeed", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            return !BigBrainSAIN.IsBotUsingSAINLayer(___botOwner_0);
        }
    }

    public class TargetSpeedDisable2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("method_13", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            return !BigBrainSAIN.IsBotUsingSAINLayer(___botOwner_0);
        }
    }

    public class SetPoseDisable1 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("method_12", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            return !BigBrainSAIN.IsBotUsingSAINLayer(___botOwner_0);
        }
    }

    public class SetPoseDisable2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("SetPose", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            return !BigBrainSAIN.IsBotUsingSAINLayer(___botOwner_0);
        }
    }
}
