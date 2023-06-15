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
            return AccessTools.Method(SteeringProp.PropertyType, "method_1");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            string layer = ___botOwner_0.Brain.ActiveLayerName();
            if (SAINPlugin.SAINLayers.Contains(layer))
            {
                return false;
            }
            return true;
        }
    }
    public class HoldOrCoverDisable1 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass29), "HoldOrCover");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotLogicDecision __result)
        {
            __result = BotLogicDecision.holdPosition;
            return false;
        }
    }

    public class HoldOrCoverDisable2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass29), "HoldOrCoverRun");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotLogicDecision __result)
        {
            __result = BotLogicDecision.holdPosition;
            return false;
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
            string layer = ___botOwner_0.Brain.ActiveLayerName();
            if (SAINPlugin.SAINLayers.Contains(layer))
            {
                return false;
            }
            return true;
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
            string layer = ___botOwner_0.Brain.ActiveLayerName();
            if (SAINPlugin.SAINLayers.Contains(layer))
            {
                return false;
            }
            return true;
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
            string layer = __instance.Brain.ActiveLayerName();
            if (SAINPlugin.SAINLayers.Contains(layer))
            {
                //Logger.LogWarning(Environment.StackTrace);
                return false;
            }
            return true;
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
            string layer = ___botOwner_0.Brain.ActiveLayerName();
            if (SAINPlugin.SAINLayers.Contains(layer))
            {
                return false;
            }
            return true;
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
            string layer = ___botOwner_0.Brain.ActiveLayerName();
            if (SAINPlugin.SAINLayers.Contains(layer))
            {
                return false;
            }
            return true;
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
            string layer = ___botOwner_0.Brain.ActiveLayerName();
            if (SAINPlugin.SAINLayers.Contains(layer))
            {
                return false;
            }
            return true;
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
            string layer = ___botOwner_0.Brain.ActiveLayerName();
            if (SAINPlugin.SAINLayers.Contains(layer))
            {
                return false;
            }
            return true;
        }
    }
}
