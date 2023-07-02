using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.UserSettings;
using System.Reflection;
using UnityEngine;
using static SAIN.Editor.EditorSettings;

namespace SAIN.Patches
{
    public class Math
    {
        public static float VisionSpeed(float dist)
        {
            float result = 1f;
            if (dist >= CloseFarThresh.Value)
            {
                result *= FarVisionSpeed.Value;
            }
            else
            {
                result *= CloseVisionSpeed.Value;
            }
            result *= EditorSettings.VisionSpeed.Value;

            return result;
        }
    }
    //IsEnemyLookingAtMe

    public class NoAIESPPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetMethod("IsEnemyLookingAtMe", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(IAIDetails) }, null);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }

    public class VisionSpeedPatch : ModulePatch
    {
        private static PropertyInfo _GoalEnemyProp;
        protected override MethodBase GetTargetMethod()
        {
            _GoalEnemyProp = AccessTools.Property(typeof(BotMemoryClass), "GoalEnemy");
            return AccessTools.Method(_GoalEnemyProp.PropertyType, "method_7");
        }

        [PatchPostfix]
        public static void PatchPostfix(BifacialTransform BotTransform, BifacialTransform enemy, ref float __result)
        {
            float dist = (BotTransform.position - enemy.position).magnitude;
            __result *= Math.VisionSpeed(dist);
        }
    }
}