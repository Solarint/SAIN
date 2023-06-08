using Aki.Reflection.Patching;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SAIN.Components;
using SAIN.Helpers;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

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
            return false;
        }
    }

    public class SprintPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("Sprint", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            if (___botOwner_0.Brain.ActiveLayerName() == "SAIN Combat System")
            {
                return false;
            }
            return false;
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
            return false;
        }
    }

    public class MoverPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("Mover")?.PropertyType?.GetMethod("method_9", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner ___botOwner_0, ref Vector3[] ___vector3_0, ref int ___Int32_0)
        {
            int i = ___Int32_0;
            var way = ___vector3_0;

            if (i < way.Length - 2)
            {
                Vector3 corner = way[i];
                Vector3 nextCorner = way[i + 1];
                Vector3 bot = ___botOwner_0.Position;

                var directionA = corner - bot;
                var directionB = nextCorner - bot;
                var directionCorners = nextCorner - corner;

                float angle = Vector3.Angle(directionA.normalized, directionB.normalized);

                if (directionA.magnitude < 0.5f && angle > 30f && directionCorners.magnitude < 0.5f)
                {
                    ___botOwner_0.GetPlayer.EnableSprint(false);
                }
            }
        }
    }
}
