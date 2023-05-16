using Aki.Reflection.Patching;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SAIN.Components;
using System.Reflection;
using UnityEngine.AI;

namespace SAIN.Patches
{
    public class AddComponentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), "PreActivate");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner __instance)
        {
            __instance.gameObject.AddComponent<SAINComponent>();
        }
    }

    public class DisposeComponentPatch : ModulePatch
    {
        private static FieldInfo _ebotState_0;

        protected override MethodBase GetTargetMethod()
        {
            _ebotState_0 = AccessTools.Field(typeof(BotOwner), "ebotState_0");
            return AccessTools.Method(typeof(BotOwner), "Dispose");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner __instance)
        {
            EBotState botState = (EBotState)_ebotState_0.GetValue(__instance);
            if (botState == EBotState.PreActive)
                return;

            __instance.gameObject.GetComponent<SAINComponent>().Dispose();
        }
    }

    public class BotGlobalMindPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalsMindSettings), "Update");
        }

        [PatchPostfix]
        public static void PatchPostfix(BotGlobalsMindSettings __instance)
        {
            __instance.DOG_FIGHT_IN = 0.1f;
            __instance.DOG_FIGHT_OUT = 0.2f;
            __instance.DIST_TO_STOP_RUN_ENEMY = 0f;
            __instance.NO_RUN_AWAY_FOR_SAFE = false;
        }
    }

    public class BotGlobalMovePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalsMoveSettings), "Update");
        }

        [PatchPostfix]
        public static void PatchPostfix(BotGlobalsMoveSettings __instance)
        {
            __instance.RUN_IF_CANT_SHOOT = true;
            __instance.SEC_TO_CHANGE_TO_RUN = 0.5f;
            __instance.CHANCE_TO_RUN_IF_NO_AMMO_0_100 = 100f;
            __instance.RUN_IF_GAOL_FAR_THEN = 5f;
            __instance.RUN_TO_COVER_MIN = 2f;
        }
    }

    public class BotGlobalCorePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass559), "Update");
        }

        [PatchPostfix]
        public static void PatchPostfix(GClass559 __instance)
        {
            __instance.MIN_DIST_TO_STOP_RUN = 0f;
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
}
