using SPT.Reflection.Patching;
using EFT;
using EFT.EnvironmentEffect;
using HarmonyLib;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Reflection;
using UnityEngine;

namespace SAIN.Patches.Generic
{
    public static class GenericHelpers
    {
        public static bool CheckNotNull(BotOwner botOwner)
        {
            return botOwner != null &&
                botOwner.gameObject != null &&
                botOwner.gameObject.transform != null &&
                botOwner.Transform != null &&
                !botOwner.IsDead;
        }
    }

    public class StopRefillMagsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotReload), "method_1");
        }

        [PatchPrefix]
        public static bool Patch(BotOwner ___botOwner_0)
        {
            return SAINPlugin.IsBotExluded(___botOwner_0);
        }
    }

    public class SetEnvironmentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass567), "SetEnvironment");
        }

        [PatchPostfix]
        public static void Patch(GClass567 __instance, IndoorTrigger trigger)
        {
            SAINBotController.Instance?.PlayerEnviromentChanged(__instance?.Player?.ProfileId, trigger);
        }
    }

    public class AddPointToSearchPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsGroup), "AddPointToSearch");
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotsGroup __instance, BotOwner owner)
        {
            return SAINPlugin.IsBotExluded(owner);
        }
    }

    public class SetPanicPointPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotMemoryClass), "SetPanicPoint");
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotOwner ___botOwner_0)
        {
            return SAINPlugin.IsBotExluded(___botOwner_0);
        }
    }

    public class HaveSeenEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(EnemyInfo), "HaveSeen");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref bool __result, EnemyInfo __instance)
        {
            if (__result == true)
            {
                return;
            }
            if (SAINEnableClass.GetSAIN(__instance.Owner, out var sain)
                //&& sain.Info.Profile.IsPMC
                && sain.EnemyController.CheckAddEnemy(__instance.Person)?.Heard == true)
            {
                __result = true;
            }
        }
    }

    public class TurnDamnLightOffPatch : ModulePatch
    {
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(BotWeaponSelector), nameof(BotWeaponSelector.TryChangeToSlot));
		}

		[PatchPrefix]
        public static void PatchPrefix(ref BotOwner ___botOwner_0)
        {
            // Try to turn a gun's light off before swapping weapon.
            ___botOwner_0?.BotLight?.TurnOff(false, true);
        }
    }

    internal class ShallKnowEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EnemyInfo), "ShallKnowEnemy");
        }

        [PatchPostfix]
        public static void PatchPostfix(EnemyInfo __instance, ref bool __result)
        {
            if (!SAINEnableClass.GetSAIN(__instance.Owner, out var botComponent))
            {
                return;
            }
            var enemy = botComponent.EnemyController.CheckAddEnemy(__instance.Person);
            __result = enemy?.EnemyKnown == true;
        }

        public static bool BotsGroupSenseRecently(EnemyInfo enemyInfo)
        {
            BotsGroup group = enemyInfo.GroupOwner;
            for (int i = 0; i < group.MembersCount; i++)
            {
                if (SAINEnableClass.GetSAIN(group.Member(i), out BotComponent sain)
                    && EnemySenseRecently(sain, enemyInfo))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EnemySenseRecently(BotComponent sain, EnemyInfo enemyInfo)
        {
            Enemy myEnemy = sain.EnemyController.CheckAddEnemy(enemyInfo.Person);
            return myEnemy?.CheckValid() == true && myEnemy.EnemyKnown;
        }
    }

    public enum KnowEnemyReason
    {
        None = 0,
        HeardRecent = 1,
        SeenRecent = 2,
        SquadHeardRecent = 3,
        SquadSeenRecent = 4,
    }

    internal class ShallKnowEnemyLatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EnemyInfo), "ShallKnowEnemyLate");
        }

        [PatchPostfix]
        public static void PatchPostfix(EnemyInfo __instance, ref bool __result)
        {
            if (!SAINEnableClass.GetSAIN(__instance.Owner, out var botComponent))
            {
                return;
            }
            var enemy = botComponent.EnemyController.CheckAddEnemy(__instance.Person);
            __result = enemy?.EnemyKnown == true;
        }
    }

    public class GrenadeThrownActionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), "method_4");
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotsController __instance, Grenade grenade, Vector3 position, Vector3 force, float mass)
        {
            Vector3 danger = Vector.DangerPoint(position, force, mass);
            foreach (BotOwner bot in __instance.Bots.BotOwners)
            {
                if (SAINPlugin.IsBotExluded(bot))
                {
                    bot.BewareGrenade.AddGrenadeDanger(danger, grenade);
                }
            }
            return false;
        }
    }

    public class GrenadeExplosionActionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), "method_3");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false;
        }
    }

}