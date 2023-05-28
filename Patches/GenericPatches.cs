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
                            bot.GetComponent<SAINComponent>().Grenade.GrenadeThrown(grenade, danger);
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
                    Etype = EInteractionType.Open;

                return;
            }

            if (Etype == EInteractionType.Open || Etype == EInteractionType.Breach)
            {
                NavMeshPath navMeshPath = new NavMeshPath();
                NavMesh.CalculatePath(___botOwner_0.Transform.position, ___botOwner_0.Memory.GoalEnemy.CurrPosition, -1, navMeshPath);

                bool enemyClose = navMeshPath.CalculatePathLength() < 15f;

                if (enemyClose || ___botOwner_0.Memory.IsUnderFire)
                {
                    var breakInParameters = door.GetBreakInParameters(___botOwner_0.Position);

                    if (door.BreachSuccessRoll(breakInParameters.InteractionPosition))
                    {
                        Etype = EInteractionType.Breach;
                    }
                    else Etype = EInteractionType.Open;
                }
                else Etype = EInteractionType.Open;
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
}
