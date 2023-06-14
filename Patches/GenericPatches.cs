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

namespace SAIN.Patches
{
    public class GrenadeThrownActionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotControllerClass), "method_4");
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotControllerClass __instance, Grenade grenade, Vector3 position, Vector3 force, float mass)
        {
            foreach (BotOwner bot in __instance.Bots.BotOwners)
            {
                if (!bot.IsDead && bot.BotState == EBotState.Active)
                {
                    if (Vector3.Distance(grenade.transform.position, bot.Transform.position) < 150f)
                    {
                        var danger = VectorHelpers.Point(position, force, mass);

                        if (Vector3.Distance(danger, bot.Transform.position) < 80f)
                        {
                            var component = bot.GetComponent<SAINComponent>();

                            if (component == null)
                            {
                                return true;
                            }

                            component.Grenade.EnemyGrenadeThrown(grenade, danger);
                        }
                    }
                }
            }

            return false;
        }
    }

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

    public class AddEnemyToAllGroupsInBotZonePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotControllerClass).GetMethod("AddEnemyToAllGroupsInBotZone", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return true;
        }
    }

    public class HoldOrCoverPatch1 : ModulePatch
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

    public class HoldOrCoverPatch2 : ModulePatch
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

    public class SprintPatch1 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("Sprint", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            string layer = ___botOwner_0.Brain.ActiveLayerName();
            if (Plugin.SAINLayers.Contains(layer))
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
        public static bool PatchPrefix()
        {
            return false;
        }
    }

    public class TargetSpeedPatch1 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("SetTargetMoveSpeed", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            string layer = ___botOwner_0.Brain.ActiveLayerName();
            if (Plugin.SAINLayers.Contains(layer))
            {
                return false;
            }
            return true;
        }
    }

    public class TargetSpeedPatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("method_13", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            string layer = ___botOwner_0.Brain.ActiveLayerName();
            if (Plugin.SAINLayers.Contains(layer))
            {
                return false;
            }
            return true;
        }
    }

    public class SetPosePatch1 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("method_12", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            string layer = ___botOwner_0.Brain.ActiveLayerName();
            if (Plugin.SAINLayers.Contains(layer))
            {
                return false;
            }
            return true;
        }
    }

    public class SetPosePatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("SetPose", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            string layer = ___botOwner_0.Brain.ActiveLayerName();
            if (Plugin.SAINLayers.Contains(layer))
            {
                return false;
            }
            return true;
        }
    }

    public class AddEnemyToAllGroupsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotControllerClass).GetMethod("AddEnemyToAllGroups", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return true;
        }
    }
}
