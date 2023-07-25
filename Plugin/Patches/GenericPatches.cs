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
using Comfort.Common;

namespace SAIN.Patches.Generic
{
    internal class BotGroupAddEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(BotGroupClass).GetMethod("AddEnemy");
        [PatchPrefix]
        public static bool PatchPrefix(IAIDetails person)
        {
            if (person == null || (person.IsAI && person.AIData?.BotOwner?.GetPlayer == null))
            {
                return false;
            }

            return true;
        }
    }

    internal class BotMemoryAddEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(BotMemoryClass).GetMethod("AddEnemy");
        [PatchPrefix]
        public static bool PatchPrefix(IAIDetails enemy)
        {
            if (enemy == null || (enemy.IsAI && enemy.AIData?.BotOwner?.GetPlayer == null))
            {
                return false;
            }

            return true;
        }
    }

    public class InitHelper : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotControllerClass), "Init");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner __instance)
        {
            VectorHelpers.Init();
        }
    }

    public class GrenadeThrownActionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotControllerClass), "method_4");
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotControllerClass __instance, Grenade grenade, Vector3 position, Vector3 force, float mass)
        {
            Vector3 danger = VectorHelpers.DangerPoint(position, force, mass);
            foreach (BotOwner bot in __instance.Bots.BotOwners)
            {
                if (SAINPlugin.BotController.Bots.ContainsKey(bot.ProfileId))
                {
                    continue;
                }
                bot.BewareGrenade.AddGrenadeDanger(danger, grenade);
            }
            return false;
        }
    }

    public class GrenadeExplosionActionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotControllerClass), "method_3");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false;
        }
    }

    public class GetBotController : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotControllerClass).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static void PatchPrefix(BotControllerClass __instance)
        {
            SAINPlugin.BotController.DefaultController = __instance;
        }
    }

    public class GetBotSpawnerClass : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotSpawnerClass), "AddPlayer");
        }

        [PatchPostfix]
        public static void PatchPostfix(BotSpawnerClass __instance)
        {
            var controller = SAINPlugin.BotController;
            if (controller != null && controller.BotSpawnerClass == null)
            {
                controller.BotSpawnerClass = __instance;
            }
        }
    }
}
