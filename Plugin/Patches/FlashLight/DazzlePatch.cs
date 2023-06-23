using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Classes;
using SAIN.Components;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SAIN.Patches
{
    public class LookDisablePatch1 : ModulePatch
    {
        private static PropertyInfo _GoalEnemyProp;
        protected override MethodBase GetTargetMethod()
        {
            _GoalEnemyProp = AccessTools.Property(typeof(BotMemoryClass), "GoalEnemy");
            return AccessTools.Method(_GoalEnemyProp.PropertyType, "CheckLookEnemy");
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }

    public class LookDisablePatch2 : ModulePatch
    {
        private static PropertyInfo _GoalEnemyProp;
        protected override MethodBase GetTargetMethod()
        {
            _GoalEnemyProp = AccessTools.Property(typeof(BotMemoryClass), "GoalEnemy");
            return AccessTools.Method(_GoalEnemyProp.PropertyType, "method_2");
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}