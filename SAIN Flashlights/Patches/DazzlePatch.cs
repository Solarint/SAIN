using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Flashlights.Helpers;
using System.Reflection;
using UnityEngine;

namespace SAIN.Flashlights.Patches
{
    public class DazzlePatch : ModulePatch
    {
        private static PropertyInfo _GoalEnemy;
        protected override MethodBase GetTargetMethod()
        {
            _GoalEnemy = AccessTools.Property(typeof(BotMemoryClass), "GoalEnemy");
            return AccessTools.Method(_GoalEnemy.PropertyType, "CheckLookEnemy");
        }
        [PatchPrefix]
        public static void Prefix(ref BotOwner ___botOwner_0, IAIDetails person, ref float ___NextTimeCheck)
        {
            if (___NextTimeCheck < Time.time)
            {
                if (person.AIData.UsingLight)
                {
                    FlashLight.EnemyWithFlashlight(___botOwner_0, person, ___NextTimeCheck);
                }
            }
        }
    }
}
