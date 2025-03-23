using EFT;
using EFT.EnvironmentEffect;
using HarmonyLib;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SPT.Reflection.Patching;
using System.Collections.Generic;
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
            // Look for one of the following to find the above GClass:
            // AIDataRequestController GClass???::aidataRequestController_0
            // System.Action<EFT.InventoryLogic.Weapon> GClass???::OnStationaryTaken
            // System.Action<GClass567> GClass???::OnBecomeDrunk
            // Those probably won't change any time soon.
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
            if (__result == true) {
                return;
            }
            if (SAINEnableClass.GetSAIN(__instance.Owner, out var sain)
                //&& sain.Info.Profile.IsPMC
                && sain.EnemyController.CheckAddEnemy(__instance.Person)?.Heard == true) {
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
            if (!SAINEnableClass.GetSAIN(__instance.Owner, out var botComponent)) {
                return;
            }
            var enemy = botComponent.EnemyController.CheckAddEnemy(__instance.Person);
            __result = enemy?.EnemyKnown == true;
        }

        public static bool BotsGroupSenseRecently(EnemyInfo enemyInfo)
        {
            BotsGroup group = enemyInfo.GroupOwner;
            for (int i = 0; i < group.MembersCount; i++) {
                if (SAINEnableClass.GetSAIN(group.Member(i), out BotComponent sain)
                    && EnemySenseRecently(sain, enemyInfo)) {
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
            if (!SAINEnableClass.GetSAIN(__instance.Owner, out var botComponent)) {
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
            foreach (BotOwner bot in __instance.Bots.BotOwners) {
                if (SAINPlugin.IsBotExluded(bot)) {
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

    public class AllowRequestPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotRequestController), nameof(BotRequestController.method_2));
        }

        [PatchPrefix]
        public static bool Patch(BotOwner ____owner)
        {
            BotRequest curRequest = ____owner.BotRequestController.CurRequest;
            if (curRequest == null) {
                return false;
            }
            if (!SAINEnableClass.GetSAIN(____owner, out BotComponent sain)) {
                return true;
            }
            if (sain.HasEnemy && curRequest.Requester?.IsAI == true) {
                curRequest.Dispose();
                return false;
            }
            return true;
        }
    }

    public class FindRequestForMePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGroupRequestController), nameof(BotGroupRequestController.FindForMe));
        }

        [PatchPrefix]
        public static bool Patch(BotOwner executer, List<BotRequest> ____listOfRequests)
        {
            // Copied original code in FindForMe, but add check to see if requester is AI or not if this bot currently has an active enemy.
            // START NEW //
            if (!SAINEnableClass.GetSAIN(executer, out BotComponent sain)) {
                return true;
            }
            if (!sain.HasEnemy) {
                return true;
            }
            // END NEW //

            BotRequest botRequest = null;
            foreach (BotRequest botRequest2 in ____listOfRequests) {
                // START NEW //
                IPlayer requestor = botRequest2.Requester;
                if (requestor != null && requestor.IsAI) {
                    continue;
                }
                // END NEW //
                if ((botRequest2.CanExecuteByMyself || (Player)botRequest2.Requester != executer.GetPlayer) && (!executer.Boss.IamBoss || executer.Boss.AllowRequestSelf || executer.GetPlayer.Id != botRequest2.Requester.Id) && botRequest2.CanStartExecute(executer)) {
                    botRequest = botRequest2;
                    break;
                }
            }
            if (botRequest != null) {
                botRequest.Take(executer);
                ____listOfRequests.Remove(botRequest);
            }
            return false;
        }
    }
}