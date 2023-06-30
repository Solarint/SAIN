using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace SAIN.Patches
{
    public class Math
    {
        public static float VisionSpeed(float dist)
        {
            float result = 1f;
            if (dist >= Difficulty.CloseFarThresh)
            {
                result *= Difficulty.FarVisionSpeed;
            }
            else
            {
                result *= Difficulty.CloseVisionSpeed;
            }
            result *= Difficulty.VisionSpeed;

            return result;
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