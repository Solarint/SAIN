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
    namespace GlobalSettings
    {
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
    }
}
