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
            if (!SAINPlugin.BotController.GetBot(___botOwner_0.ProfileId, out var bot))
            {
                return;
            }

            if (bot.Enemy == null)
            {
                if (Etype == EInteractionType.Breach)
                {
                    Etype = EInteractionType.Open;
                }

                return;
            }

            if (Etype == EInteractionType.Open || Etype == EInteractionType.Breach)
            {
                bool enemyClose = Vector3.Distance(bot.Position, bot.Enemy.Position) < 30f;

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

    public class StopMoveDisable1 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetMethod("StopMove", BindingFlags.Instance | BindingFlags.Public );
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner __instance)
        {
            return false;
        }
    }

    public class StopMoveDisable2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("Stop", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner __instance)
        {
            return false;
        }
    }

    public class SprintPause : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("SprintPause", BindingFlags.Instance | BindingFlags.Public );
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner __instance)
        {
            return false;
        }
    }

    public class PlayerSprint : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player)?.GetMethod("EnableSprint", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(Player __instance)
        {
            Player player = __instance;
            if (player.IsAI)
            {
                string Id = player.AIData.BotOwner.ProfileId;
                var Bots = SAINPlugin.BotController.Bots;
                if (Bots.ContainsKey(Id))
                {
                    if (Bots[Id].LayersActive)
                    {
                        if (!Environment.StackTrace.Contains("SAIN"))
                        {
                            Logger.LogWarning("WHY U TRY STOP SPRINT");
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }

    public class MovePause : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("MovementPause", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner __instance)
        {
            return false;
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
