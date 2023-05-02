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
    namespace DogFight
    {
        public class ShallStartDogFight
        {
            public static bool ShallStart(BotOwner bot)
            {
                if (bot.Memory.GoalEnemy != null)
                {
                    if (bot.Memory.GoalEnemy.Distance > 25f)
                    {
                        if (!bot.Memory.BotCurrentCoverInfo.UseDogFight(bot.Settings.FileSettings.Cover.DOG_FIGHT_AFTER_LEAVE))
                        {
                            return true;
                        }
                    }
                }
                return false;
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
                __instance.DOG_FIGHT_IN = 1f;
                __instance.DOG_FIGHT_OUT = 2f;
            }
        }

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

        public class TryStopReloadPatch : ModulePatch
        {
            private static PropertyInfo _Reload;

            private static PropertyInfo _WeaponManager;

            protected override MethodBase GetTargetMethod()
            {
                _WeaponManager = AccessTools.Property(typeof(BotOwner), "WeaponManager");

                _Reload = AccessTools.Property(_WeaponManager.PropertyType, "Reload");

                return AccessTools.Method(_Reload.PropertyType, "TryStopReload");
            }

            [PatchPostfix]
            public static void PatchPostfix(ref BotOwner ___botOwner_0, ref float ___float_4)
            {
                if (___float_4 < Time.time)
                {
                    ___float_4 = Time.time + 2f;
                    ___botOwner_0.ShootData.Shoot();
                }
            }
        }

        public class DogFightGStructPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GClass30), "EndDogFight");
            }

            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref GStruct7 __result)
            {
                if (___botOwner_0.Memory.GoalEnemy != null)
                {
                    if (___botOwner_0.Memory.GoalEnemy.Distance > 25f)
                    {
                        if (!___botOwner_0.Memory.BotCurrentCoverInfo.UseDogFight(___botOwner_0.Settings.FileSettings.Cover.DOG_FIGHT_AFTER_LEAVE))
                        {
                            __result = new GStruct7("SAIN: DogFightEnd", true);
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public class EndShootFromPlacePatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GClass30), "EndShootFromPlace");
            }

            [PatchPostfix]
            public static void PatchPostfix(ref BotOwner ___botOwner_0, ref GStruct7 __result)
            {
                var dogFight = ___botOwner_0.gameObject.GetComponent<DogFightComponent>();
                if (dogFight.StartDogFight())
                {
                    __result = new GStruct7("Start SAIN DogFight", true);
                    return;
                }
                if (dogFight.DogFightState != BotDogFightStatus.none)
                {
                    __result = new GStruct7("SAIN DogFight", true);
                }
            }
        }

        public class ManualUpdatePatch : ModulePatch
        {
            private static PropertyInfo _DogFightProperty;
            protected override MethodBase GetTargetMethod()
            {
                _DogFightProperty = AccessTools.Property(typeof(BotOwner), "DogFight");
                return AccessTools.Method(_DogFightProperty.PropertyType, "ManualUpdate");
            }
            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0)
            {
                //var dogFight = ___botOwner_0.gameObject.GetComponent<DogFightComponent>();
                //dogFight.ManualUpdate();
                return false;
            }
        }

        public class BotLogicDecisionPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GClass472), "CreateNode");
            }

            [PatchPostfix]
            public static void PatchPostfix(BotLogicDecision type)
            {
                if (type == BotLogicDecision.dogFight)
                {
                    Logger.LogWarning($"DogFight Decision Added");
                }
            }
        }

        public class IsInDogFightPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GClass30), "method_1");
            }

            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref bool __result)
            {
                var dogFight = ___botOwner_0.gameObject.GetComponent<DogFightComponent>();
                var State = dogFight.DogFightState;
                bool inDogFight = State != BotDogFightStatus.none;
                __result = inDogFight;
                return false;
            }
        }

        public class StartPatch : ModulePatch
        {
            private static PropertyInfo _DogFightProperty;

            protected override MethodBase GetTargetMethod()
            {
                _DogFightProperty = AccessTools.Property(typeof(BotOwner), "DogFight");
                return AccessTools.Method(_DogFightProperty.PropertyType, "ShallStartCauseHavePlace");
            }

            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref bool __result)
            {
                var dogFight = ___botOwner_0.gameObject.GetComponent<DogFightComponent>();
                bool start = dogFight.StartDogFight();
                __result = start;
                return false;
            }
        }

        public class UpdatePatch : ModulePatch
        {
            private static AccessTools.FieldRef<GClass126, GClass105> _gclassField;
            protected override MethodBase GetTargetMethod()
            {
                _gclassField = AccessTools.FieldRefAccess<GClass126, GClass105>("gclass105_0");
                return AccessTools.Method(typeof(GClass126), "Update");
            }

            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0, GClass126 __instance)
            {
                var goalEnemy = ___botOwner_0.Memory.GoalEnemy;
                if (goalEnemy != null && goalEnemy.CanShoot && goalEnemy.IsVisible)
                {
                    ___botOwner_0.Steering.LookToPoint(goalEnemy.CurrPosition);
                     _gclassField(__instance).Update();
                    return false;
                }
                ___botOwner_0.LookData.SetLookPointByHearing(null);
                return false;
            }
        }
    }
}
