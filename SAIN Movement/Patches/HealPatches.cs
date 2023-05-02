using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using Movement.Components;
using SAIN_Helpers;
using System.CodeDom;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT.Bots;
using EFT.NPC;

namespace Movement.Patches
{
    namespace Heal
    {
        public class StopHealPatch1 : ModulePatch
        {

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GClass91), "GetDecision");
            }

            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref GStruct8<BotLogicDecision> __result)
            {
                var goalEnemy = ___botOwner_0.Memory.GoalEnemy;
                if ((goalEnemy == null || !goalEnemy.IsVisible) && ___botOwner_0.Medecine.FirstAid.Have2Do)
                {
                    __result = new GStruct8<BotLogicDecision>(BotLogicDecision.heal, "heal now");
                    return false;
                }
                return true;
            }
        }

        public class StopHealPatch2 : ModulePatch
        {

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GClass92), "GetDecision");
            }

            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref GStruct8<BotLogicDecision> __result)
            {
                if (___botOwner_0.Memory.GoalEnemy == null)
                {
                    return true;
                }

                if (!___botOwner_0.Medecine.FirstAid.Have2Do && !___botOwner_0.Medecine.SurgicalKit.HaveWork)
                {
                    return true;
                }
                else
                {
                    if (!___botOwner_0.Memory.GoalEnemy.IsVisible && ___botOwner_0.Medecine.FirstAid.Have2Do)
                    {
                        __result = new GStruct8<BotLogicDecision>(BotLogicDecision.heal, "heal now");
                        return false;
                    }
                }
                return true;
            }
        }

        public class StopHealPatch3 : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GClass93), "GetDecision");
            }

            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref GStruct8<BotLogicDecision> __result)
            {
                if (___botOwner_0.Memory.GoalEnemy == null)
                {
                    return true;
                }

                if (!___botOwner_0.Medecine.FirstAid.Have2Do && !___botOwner_0.Medecine.SurgicalKit.HaveWork)
                {
                    return true;
                }
                else
                {
                    if (!___botOwner_0.Memory.GoalEnemy.IsVisible && ___botOwner_0.Medecine.FirstAid.Have2Do)
                    {
                        __result = new GStruct8<BotLogicDecision>(BotLogicDecision.heal, "heal now");
                        return false;
                    }
                }
                return true;
            }
        }

        public class StopHealPatch5 : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GClass30), "CheckMedsToStop");
            }

            [PatchPrefix]
            public static bool PatchPrefix(BotOwner bot, ref bool __result)
            {
                if (bot.Memory.HaveEnemy)
                {
                    bot.EnemyLookData.DoCheck();
                    var goalEnemy = bot.Memory.GoalEnemy;
                    if (goalEnemy.IsVisible)
                    {
                        __result = true;
                        return false;
                    }
                }
                __result = false;
                return false;
            }
        }

        public class StopHealPatch6 : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GClass47), "method_19");
            }

            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref bool __result)
            {
                if (___botOwner_0.Medecine.SurgicalKit.HaveWork)
                {
                    __result = Time.time - ___botOwner_0.Memory.GoalEnemy.GroupInfo.EnemyLastSeenTimeReal >= 10f;
                    return false;

                }
                __result = !___botOwner_0.Memory.GoalEnemy.IsVisible && ___botOwner_0.Medecine.FirstAid.Have2Do;
                return false;
            }
        }

        public class StopHealPatch7 : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GClass30), "method_11");
            }

            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref bool __result)
            {
                __result = Time.time - ___botOwner_0.Memory.LastTimeHit < 3f;
                return false;
            }
        }

        public class StopHealPatch8 : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.PropertyGetter(typeof(BotMemoryClass), "IsInCover");
            }

            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref bool __result)
            {
                if (___botOwner_0.Medecine.FirstAid.Have2Do)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
    }
}
